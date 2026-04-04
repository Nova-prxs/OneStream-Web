"""OneStream API documentation loader and search engine.

Parses the 287 namespace JSON files from knowledge_base/apis_v2/ and builds
an in-memory index of classes, methods, properties, and enums.  Also creates
an FTS5 search index for the docs-section search bar.
"""

from __future__ import annotations

import json
import re
import sqlite3
from pathlib import Path
from typing import Any


# ---------------------------------------------------------------------------
# Data structures (plain dicts for speed — no ORM overhead)
# ---------------------------------------------------------------------------

def build_api_cache(kb_dir: Path) -> dict[str, Any]:
    """Load all API JSON files and return a structured cache.

    Returns dict with keys:
        namespaces  — list of {name, slug, stats, class_count}
        classes     — dict[slug, class_dict]  (slug = sanitized class name)
        class_list  — sorted list of {name, slug, namespace, type, method_count, property_count}
        enums       — dict[slug, enum_dict]
        enum_list   — sorted list of {name, slug, namespace}
    """
    apis_dir = kb_dir / "apis_v2"
    index_path = apis_dir / "INDEX.json"

    if not index_path.exists():
        return _empty_cache()

    with open(index_path) as f:
        index_data = json.load(f)

    namespaces: list[dict] = []
    classes: dict[str, dict] = {}
    class_list: list[dict] = []
    enums: dict[str, dict] = {}
    enum_list: list[dict] = []

    for ns_name in index_data.get("namespaces", []):
        ns_file = apis_dir / f"{ns_name.replace('.', '_')}.json"
        if not ns_file.exists():
            continue

        try:
            with open(ns_file) as f:
                ns_data = json.load(f)
        except (json.JSONDecodeError, OSError):
            continue

        stats = ns_data.get("statistics", {})
        ns_slug = _slugify(ns_name)

        ns_classes = []
        for cls in ns_data.get("classes", []):
            if cls.get("access") == "private" or cls.get("access") == "internal":
                continue

            cls_slug = _slugify(cls["name"])
            # Handle duplicate class names across namespaces
            unique_slug = cls_slug
            if unique_slug in classes:
                unique_slug = f"{ns_slug}__{cls_slug}"

            public_methods = [
                m for m in cls.get("methods", [])
                if m.get("access", "public") == "public"
            ]
            public_props = [
                p for p in cls.get("properties", [])
                if p.get("access", "public") == "public"
            ]

            cls_entry = {
                "name": cls["name"],
                "slug": unique_slug,
                "namespace": ns_name,
                "namespace_slug": ns_slug,
                "type": cls.get("type", "class"),
                "base_class": cls.get("base_class"),
                "interfaces": cls.get("interfaces", []),
                "modifiers": cls.get("modifiers", []),
                "methods": public_methods,
                "properties": public_props,
                "constructors": cls.get("constructors", []),
                "events": cls.get("events", []),
                "nested_types": cls.get("nested_types", []),
                "description": cls.get("description", ""),
                "method_count": len(public_methods),
                "property_count": len(public_props),
                # Will be populated by link_apis_to_rules()
                "usage_examples": [],
                "related_rules": [],
            }
            classes[unique_slug] = cls_entry
            ns_classes.append(unique_slug)

            class_list.append({
                "name": cls["name"],
                "slug": unique_slug,
                "namespace": ns_name,
                "type": cls.get("type", "class"),
                "method_count": len(public_methods),
                "property_count": len(public_props),
            })

        # Enums
        for enum in ns_data.get("classes", []):
            if enum.get("type") != "enum":
                continue
            enum_slug = _slugify(enum["name"])
            if enum_slug in enums:
                enum_slug = f"{ns_slug}__{enum_slug}"
            enums[enum_slug] = {
                "name": enum["name"],
                "slug": enum_slug,
                "namespace": ns_name,
                "members": enum.get("methods", []),
            }
            enum_list.append({
                "name": enum["name"],
                "slug": enum_slug,
                "namespace": ns_name,
            })

        # Also check for top-level enums field if present
        for enum in ns_data.get("enums", []):
            enum_slug = _slugify(enum.get("name", ""))
            if not enum_slug or enum_slug in enums:
                continue
            enums[enum_slug] = {
                "name": enum["name"],
                "slug": enum_slug,
                "namespace": ns_name,
                "members": enum.get("members", enum.get("values", [])),
            }
            enum_list.append({
                "name": enum["name"],
                "slug": enum_slug,
                "namespace": ns_name,
            })

        namespaces.append({
            "name": ns_name,
            "slug": ns_slug,
            "stats": stats,
            "class_count": len(ns_classes),
            "class_slugs": ns_classes,
        })

    # Sort
    class_list.sort(key=lambda c: c["name"].lower())
    enum_list.sort(key=lambda e: e["name"].lower())
    namespaces.sort(key=lambda n: n["name"])

    return {
        "namespaces": namespaces,
        "classes": classes,
        "class_list": class_list,
        "enums": enums,
        "enum_list": enum_list,
    }


# ---------------------------------------------------------------------------
# FTS5 search index for API docs
# ---------------------------------------------------------------------------

def build_api_search_index(api_cache: dict[str, Any]) -> sqlite3.Connection:
    """Build an FTS5 index covering classes, methods, and enums."""
    conn = sqlite3.connect(":memory:", check_same_thread=False)
    conn.execute(
        "CREATE VIRTUAL TABLE api_fts USING fts5("
        "  name, namespace, kind, detail, slug,"
        "  tokenize='porter unicode61'"
        ")"
    )

    rows: list[tuple[str, str, str, str, str]] = []

    for cls in api_cache.get("class_list", []):
        full = api_cache["classes"].get(cls["slug"], {})
        # Index the class itself
        method_names = " ".join(m["name"] for m in full.get("methods", []))
        prop_names = " ".join(p["name"] for p in full.get("properties", []))
        detail = f"{method_names} {prop_names}".strip()
        rows.append((cls["name"], cls["namespace"], cls["type"], detail, cls["slug"]))

        # Index each method individually
        for m in full.get("methods", []):
            params = ", ".join(
                f"{p.get('type', '')} {p.get('name', '')}"
                for p in m.get("parameters", [])
            )
            sig = f"{m.get('return_type', 'void')} {m['name']}({params})"
            desc = m.get("description", "")
            rows.append((
                m["name"],
                cls["namespace"],
                "method",
                f"{cls['name']}.{m['name']} {sig} {desc}",
                cls["slug"],
            ))

    for enum in api_cache.get("enum_list", []):
        full = api_cache["enums"].get(enum["slug"], {})
        members = " ".join(
            m.get("name", "") if isinstance(m, dict) else str(m)
            for m in full.get("members", [])
        )
        rows.append((enum["name"], enum["namespace"], "enum", members, enum["slug"]))

    conn.executemany(
        "INSERT INTO api_fts (name, namespace, kind, detail, slug) VALUES (?, ?, ?, ?, ?)",
        rows,
    )
    conn.commit()
    return conn


def search_api_docs(
    conn: sqlite3.Connection, query: str, limit: int = 50
) -> list[dict[str, str]]:
    """Search the API FTS5 index. Returns list of {name, namespace, kind, detail, slug, snippet}."""
    if not query.strip():
        return []

    # Sanitize query for FTS5
    safe_q = re.sub(r"[^\w\s\.]", " ", query).strip()
    if not safe_q:
        return []

    # Try exact match first, then prefix match
    terms = safe_q.split()
    fts_query = " OR ".join(f'"{t}"' for t in terms)

    try:
        rows = conn.execute(
            "SELECT name, namespace, kind, detail, slug, "
            "  snippet(api_fts, 3, '<mark>', '</mark>', '...', 40) as snippet "
            "FROM api_fts WHERE api_fts MATCH ? "
            "ORDER BY rank LIMIT ?",
            (fts_query, limit),
        ).fetchall()
    except Exception:
        # Fallback: prefix match
        fts_query = " OR ".join(f"{t}*" for t in terms)
        try:
            rows = conn.execute(
                "SELECT name, namespace, kind, detail, slug, "
                "  snippet(api_fts, 3, '<mark>', '</mark>', '...', 40) as snippet "
                "FROM api_fts WHERE api_fts MATCH ? "
                "ORDER BY rank LIMIT ?",
                (fts_query, limit),
            ).fetchall()
        except Exception:
            return []

    return [
        {
            "name": r[0],
            "namespace": r[1],
            "kind": r[2],
            "detail": r[3],
            "slug": r[4],
            "snippet": r[5],
        }
        for r in rows
    ]


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _slugify(name: str) -> str:
    """Convert a class/namespace name to a URL-safe slug."""
    return re.sub(r"[^a-zA-Z0-9]+", "-", name).strip("-").lower()


def _empty_cache() -> dict[str, Any]:
    return {
        "namespaces": [],
        "classes": {},
        "class_list": [],
        "enums": {},
        "enum_list": [],
    }
