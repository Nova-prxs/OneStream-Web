"""Business Rules browser — loads VB.NET/C# rules and links them to APIs.

Parses rules from Business Rule/{Extender,Finance,Connector,...}/ folders,
extracts API references from code, and builds a bidirectional index.
"""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from pygments import highlight as _pygments_highlight
from pygments.formatters import HtmlFormatter as _HtmlFormatter
from pygments.lexers import get_lexer_by_name as _get_lexer


# Well-known OneStream API class prefixes to detect in code
_API_PATTERNS: list[re.Pattern[str]] = [
    re.compile(r"\b(BRApi\.\w+)\b"),
    re.compile(r"\b(SessionInfo)\b"),
    re.compile(r"\b(DataBuffer\w*)\b"),
    re.compile(r"\b(DashboardExtenderArgs\w*)\b"),
    re.compile(r"\b(ExtenderArgs\w*)\b"),
    re.compile(r"\b(FinanceRulesApi\w*)\b"),
    re.compile(r"\b(ConnectorArgs\w*)\b"),
    re.compile(r"\b(XFExpression\w*)\b"),
    re.compile(r"\b(MemberInfo)\b"),
    re.compile(r"\b(DimConstants)\b"),
    re.compile(r"\b(TimeDimProfile\w*)\b"),
    re.compile(r"\b(DataBufferCellPk\w*)\b"),
]

# Regex to extract full qualified calls like OneStream.Shared.Wcf.ClassName
_QUALIFIED_NS = re.compile(r"\b(OneStream\.\w+\.\w+)\.(\w+)\b")

# Regex to detect using statements in C#/VB
_USING_RE = re.compile(r"(?:using|Imports)\s+(OneStream\.\w+(?:\.\w+)*)\b")

RULE_TYPES = [
    ("Extender", "Extender Business Rules"),
    ("Finance", "Finance Business Rules"),
    ("Connector", "Connector Business Rules"),
    ("DashboardStringFunction", "Dashboard String Functions"),
    ("Assemblies", "Assemblies & SQL"),
]

# Subcategory classification for Assemblies based on parent directory name
_ASSEMBLY_SUBCATEGORIES: list[tuple[str, list[str]]] = [
    ("SQL Tables", ["sql_tables"]),
    ("SQL Migrations", ["sql_migrations"]),
    ("SQL Views & Functions", ["sql_views", "sql_functions", "sql"]),
    ("Dashboard & Navigation", ["dashboard_solution helpers", "dashboard_navigation helpers",
                                 "dashboard_graphs", "dashboard_data management",
                                 "dashboard", "navigation", "Navigation"]),
    ("Data Management", ["data management_extensibility rules", "data management_import data",
                         "data management_export data", "data management_export extensible document",
                         "data management_ETL Mapping"]),
    ("Service Factory", ["service factory", "service_factory", "service",
                         "factory_services", "factory"]),
    ("Calculations", ["calculations", "functions", "helper functions"]),
    ("Spreadsheets", ["spreadsheet_CashFlow", "spreadsheet_Cash Debt Position",
                      "spreadsheet_Extensible Document", "spreadsheet_Treasury Monitoring"]),
]


def _classify_assembly_rule(rule_file: Path) -> str:
    """Classify an Assembly rule file into a subcategory by parent dir name."""
    parent = rule_file.parent.name
    for subcat_name, dir_patterns in _ASSEMBLY_SUBCATEGORIES:
        if parent in dir_patterns:
            return subcat_name
    # Check partial matches
    parent_lower = parent.lower()
    if "sql" in parent_lower:
        return "SQL Views & Functions"
    if "dashboard" in parent_lower or "navigation" in parent_lower:
        return "Dashboard & Navigation"
    if "data management" in parent_lower or "import" in parent_lower or "export" in parent_lower:
        return "Data Management"
    if "service" in parent_lower or "factory" in parent_lower:
        return "Service Factory"
    if "spreadsheet" in parent_lower:
        return "Spreadsheets"
    if "calc" in parent_lower or "helper" in parent_lower or "function" in parent_lower:
        return "Calculations"
    if "extensibility" in parent_lower:
        return "Data Management"
    if "query" in parent_lower or "Queries" == parent:
        return "SQL Views & Functions"
    return "Other"


def build_rules_cache(rules_dir: Path) -> dict[str, Any]:
    """Load all business rules and extract metadata.

    Returns dict with keys:
        types       — list of {slug, name, description, count}
        rules       — dict[type_slug/rule_slug, rule_dict]
        rules_list  — sorted list of {name, slug, type_slug, type_name, apis_used}
        api_to_rules — dict[api_class_name, list of rule_slugs]
    """
    types: list[dict] = []
    rules: dict[str, dict] = {}
    rules_list: list[dict] = []
    api_to_rules: dict[str, list[str]] = {}

    for type_dir_name, type_desc in RULE_TYPES:
        type_path = rules_dir / type_dir_name
        if not type_path.is_dir():
            continue

        type_slug = type_dir_name.lower()
        type_rules: list[str] = []

        # Assemblies has nested subdirectories; other types are flat
        rule_files = sorted(
            type_path.rglob("*") if type_dir_name == "Assemblies" else type_path.iterdir()
        )
        for rule_file in rule_files:
            if not rule_file.is_file():
                continue
            if rule_file.suffix not in (".vb", ".cs", ".sql", ".txt"):
                continue

            try:
                code = rule_file.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue

            rule_name = rule_file.stem
            rule_slug = _slugify(rule_name)

            # For Assemblies, include workspace/assembly path to avoid slug collisions
            workspace = ""
            if type_dir_name == "Assemblies":
                rel = rule_file.relative_to(type_path)
                parts = rel.parts
                if len(parts) > 2:
                    workspace = f"{parts[0]} / {parts[1]}"
                    workspace_slug = f"{_slugify(parts[0])}-{_slugify(parts[1])}"
                elif len(parts) > 1:
                    workspace = parts[0]
                    workspace_slug = _slugify(parts[0])
                else:
                    workspace_slug = ""
                full_slug = f"{type_slug}/{workspace_slug}/{rule_slug}" if workspace_slug else f"{type_slug}/{rule_slug}"
            else:
                full_slug = f"{type_slug}/{rule_slug}"

            # Extract API references
            apis_found = _extract_api_refs(code)
            namespaces_used = _USING_RE.findall(code)

            # Extract a short description (first comment or class summary)
            description = _extract_description(code)

            # Extract a code snippet (first 30 meaningful lines)
            snippet = _extract_snippet(code, max_lines=30)

            # Classify Assemblies into subcategories
            subcategory = (
                _classify_assembly_rule(rule_file)
                if type_dir_name == "Assemblies"
                else ""
            )

            rule_entry = {
                "name": rule_name,
                "slug": rule_slug,
                "full_slug": full_slug,
                "type_slug": type_slug,
                "type_name": type_desc,
                "workspace": workspace,
                "subcategory": subcategory,
                "file_name": rule_file.name,
                "code": code,
                "code_html": _highlight_code(code, rule_file.suffix),
                "snippet": snippet,
                "description": description,
                "apis_used": sorted(apis_found),
                "namespaces": namespaces_used,
                "line_count": code.count("\n") + 1,
                "language": "vbnet" if rule_file.suffix == ".vb" else "csharp",
            }

            rules[full_slug] = rule_entry
            type_rules.append(full_slug)

            rules_list.append({
                "name": rule_name,
                "slug": rule_slug,
                "full_slug": full_slug,
                "type_slug": type_slug,
                "type_name": type_desc,
                "workspace": workspace,
                "subcategory": subcategory,
                "api_count": len(apis_found),
                "line_count": rule_entry["line_count"],
                "language": rule_entry["language"],
            })

            # Build reverse index: api -> rules
            for api_name in apis_found:
                if api_name not in api_to_rules:
                    api_to_rules[api_name] = []
                api_to_rules[api_name].append(full_slug)

        # Build subcategory groups for Assemblies
        subcategories: list[dict] = []
        if type_dir_name == "Assemblies":
            subcat_map: dict[str, list[str]] = {}
            for slug in type_rules:
                rule = rules[slug]
                sc = rule.get("subcategory", "Other")
                if sc not in subcat_map:
                    subcat_map[sc] = []
                subcat_map[sc].append(slug)
            # Ordered by the predefined list, then "Other" last
            ordered_names = [s[0] for s in _ASSEMBLY_SUBCATEGORIES] + ["Other"]
            for sc_name in ordered_names:
                if sc_name in subcat_map:
                    subcategories.append({
                        "name": sc_name,
                        "count": len(subcat_map[sc_name]),
                        "rule_slugs": subcat_map[sc_name],
                    })

        types.append({
            "slug": type_slug,
            "name": type_desc,
            "dir_name": type_dir_name,
            "count": len(type_rules),
            "rule_slugs": type_rules,
            "subcategories": subcategories,
        })

    rules_list.sort(key=lambda r: r["name"].lower())

    return {
        "types": types,
        "rules": rules,
        "rules_list": rules_list,
        "api_to_rules": api_to_rules,
    }


def link_apis_to_rules(
    api_cache: dict[str, Any],
    rules_cache: dict[str, Any],
) -> None:
    """Populate api_cache classes with usage_examples and related_rules from rules.

    Mutates api_cache in place (called once at startup after both caches are built).
    """
    api_to_rules = rules_cache["api_to_rules"]
    classes = api_cache["classes"]

    # Build a lookup: lowercased class name -> class slug
    name_to_slug: dict[str, str] = {}
    for slug, cls in classes.items():
        name_to_slug[cls["name"].lower()] = slug

    for api_name, rule_slugs in api_to_rules.items():
        # Try to match api_name to a class
        # api_name could be "BRApi.Utilities", "SessionInfo", "DataBuffer", etc.
        parts = api_name.split(".")
        lookup_name = parts[-1].lower()  # last part

        cls_slug = name_to_slug.get(lookup_name)
        if not cls_slug:
            # Try the full name
            cls_slug = name_to_slug.get(api_name.lower())
        if not cls_slug:
            continue

        cls = classes[cls_slug]
        for rule_slug in rule_slugs[:20]:  # Cap at 20 related rules
            rule = rules_cache["rules"].get(rule_slug)
            if not rule:
                continue

            # Add to related_rules
            if rule_slug not in [r["full_slug"] for r in cls["related_rules"]]:
                cls["related_rules"].append({
                    "name": rule["name"],
                    "full_slug": rule["full_slug"],
                    "type_name": rule["type_name"],
                    "type_slug": rule["type_slug"],
                })

            # Extract usage snippet for this specific API from the rule code
            if len(cls["usage_examples"]) < 10:
                snippet = _extract_api_usage_snippet(rule["code"], api_name)
                if snippet:
                    cls["usage_examples"].append({
                        "rule_name": rule["name"],
                        "rule_slug": rule["full_slug"],
                        "type_name": rule["type_name"],
                        "code": snippet,
                        "language": rule["language"],
                    })


# ---------------------------------------------------------------------------
# Extraction helpers
# ---------------------------------------------------------------------------

def _extract_api_refs(code: str) -> set[str]:
    """Extract OneStream API class references from source code."""
    refs: set[str] = set()
    for pattern in _API_PATTERNS:
        for match in pattern.finditer(code):
            refs.add(match.group(1))

    # Also extract from qualified namespace references
    for match in _QUALIFIED_NS.finditer(code):
        refs.add(match.group(2))

    # Clean up: remove very common non-API names
    refs.discard("String")
    refs.discard("Object")
    refs.discard("Exception")
    refs.discard("List")
    refs.discard("Dictionary")
    refs.discard("Nothing")
    refs.discard("Integer")
    refs.discard("Boolean")

    return refs


def _extract_description(code: str) -> str:
    """Extract a short description from code comments or class name."""
    # Try XML doc comment
    m = re.search(r"///\s*<summary>\s*(.*?)\s*</summary>", code, re.DOTALL)
    if m:
        return re.sub(r"\s*///\s*", " ", m.group(1)).strip()[:200]

    # Try first single-line comment
    m = re.search(r"(?:^|\n)\s*(?://|')\s*(.{10,200})", code)
    if m:
        return m.group(1).strip()[:200]

    return ""


def _extract_snippet(code: str, max_lines: int = 30) -> str:
    """Extract a meaningful code snippet (skip imports and empty lines)."""
    lines = code.splitlines()
    meaningful: list[str] = []
    in_body = False

    for line in lines:
        stripped = line.strip()
        if not in_body:
            if stripped.startswith(("using ", "Imports ", "namespace ", "#")):
                continue
            if stripped == "" or stripped == "{":
                continue
            in_body = True

        meaningful.append(line)
        if len(meaningful) >= max_lines:
            break

    return "\n".join(meaningful)


def _extract_api_usage_snippet(code: str, api_name: str, context: int = 5) -> str:
    """Extract lines around the first usage of an API in the code."""
    lines = code.splitlines()
    for i, line in enumerate(lines):
        if api_name in line:
            start = max(0, i - context)
            end = min(len(lines), i + context + 1)
            return "\n".join(lines[start:end])
    return ""


def _clean_code(code: str) -> str:
    """Strip leading/trailing blank lines and collapse 3+ consecutive blanks to 1."""
    lines = code.splitlines()
    # Strip leading empty lines
    while lines and lines[0].strip() == "":
        lines.pop(0)
    # Strip trailing empty lines
    while lines and lines[-1].strip() == "":
        lines.pop()
    # Collapse consecutive blank lines (3+ → 1)
    result: list[str] = []
    blank_count = 0
    for line in lines:
        if line.strip() == "":
            blank_count += 1
            if blank_count <= 1:
                result.append(line)
        else:
            blank_count = 0
            result.append(line)
    return "\n".join(result)


def _highlight_code(code: str, suffix: str) -> str:
    """Syntax-highlight code using Pygments. Returns HTML."""
    lang = "vbnet" if suffix == ".vb" else "csharp"
    try:
        cleaned = _clean_code(code)
        lexer = _get_lexer(lang)
        formatter = _HtmlFormatter(nowrap=False, cssclass="highlight", linenos=True)
        return _pygments_highlight(cleaned, lexer, formatter)
    except Exception:
        return ""


def _slugify(name: str) -> str:
    """Convert a name to a URL-safe slug."""
    return re.sub(r"[^a-zA-Z0-9]+", "-", name).strip("-").lower()
