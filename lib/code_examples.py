"""Generate C# usage examples for OneStream API classes.

For each class, generates a code snippet showing how to:
  - Instantiate or obtain a reference
  - Access properties (get/set)
  - Call key methods with realistic parameter names

All examples use only real method/property names from the parsed JSON data.
"""

from __future__ import annotations

from typing import Any


# Common variable names for known OneStream types
_TYPE_DEFAULTS: dict[str, str] = {
    "SessionInfo": "si",
    "string": '"value"',
    "String": '"value"',
    "int": "0",
    "Int32": "0",
    "long": "0L",
    "Int64": "0L",
    "bool": "true",
    "Boolean": "true",
    "decimal": "0m",
    "Decimal": "0m",
    "double": "0.0",
    "Double": "0.0",
    "float": "0f",
    "object": "null",
    "void": "",
    "byte[]": "new byte[0]",
    "Guid": "Guid.Empty",
    "DateTime": "DateTime.Now",
    "DataTable": "new DataTable()",
    "Dictionary<string, string>": 'new Dictionary<string, string>()',
    "Dictionary": 'new Dictionary<string, string>()',
    "List<string>": 'new List<string>()',
}

# Classes typically available via args or BRApi (not instantiated with new)
_ARG_CLASSES: dict[str, str] = {
    "DashboardExtenderArgs": "args",
    "DashboardDynamicGridArgs": "args",
    "DashboardDynamicGridGetDataArgs": "args.GetDataArgs",
    "DashboardDynamicGridSaveDataArgs": "args.SaveDataArgs",
    "ExtenderArgs": "args",
    "FinanceRulesArgs": "args",
    "ConnectorArgs": "args",
    "SessionInfo": "si",
    "BRGlobals": "globals",
    "PageInstanceInfo": "args.PageInstanceInfo",
    "DashboardComponent": "args.Component",
}


def generate_class_example(cls: dict[str, Any]) -> str:
    """Generate a C# code example for a class, using only its real members."""
    name = cls["name"]
    properties = cls.get("properties", [])
    methods = cls.get("methods", [])
    constructors = cls.get("constructors", [])
    base_class = cls.get("base_class", "")

    lines: list[str] = []
    lines.append(f"// Example: Using {name}")
    lines.append(f"// Namespace: {cls.get('namespace', '')}")
    lines.append("")

    var_name = _get_var_name(name)

    # --- How to get a reference ---
    if name in _ARG_CLASSES:
        ref = _ARG_CLASSES[name]
        lines.append(f"// '{name}' is available as a parameter in the rule entry point")
        if ref == var_name:
            # Already the parameter itself, just show a comment
            lines.append(f"// {name} {var_name}  <-- passed into Main()")
        else:
            lines.append(f"{name} {_camel(name)} = {ref};")
    elif _has_parameterless_constructor(constructors):
        lines.append(f"// Create a new instance")
        lines.append(f"var {var_name} = new {name}();")
    elif constructors:
        ctor = _pick_best_constructor(constructors)
        params_str = _format_ctor_params(ctor)
        lines.append(f"// Create a new instance")
        lines.append(f"var {var_name} = new {name}({params_str});")
    else:
        lines.append(f"// Obtain a reference to {name}")
        lines.append(f"{name} {var_name} = /* obtain from API */;")

    lines.append("")

    # --- Access properties ---
    real_props = [
        p for p in properties
        if p.get("access", "public") == "public"
        and p.get("name", "")
        and not p["name"].startswith("_")
    ]
    if real_props:
        lines.append("// Access properties")
        for p in real_props[:8]:
            pname = p["name"]
            ptype = p.get("return_type") or p.get("type") or "var"
            lines.append(f"{ptype} {_camel(pname)} = {var_name}.{pname};")

        # Show setting a property if there are settable ones
        settable = [p for p in real_props if p.get("set", True)]
        if settable:
            lines.append("")
            lines.append("// Set properties")
            for p in settable[:3]:
                pname = p["name"]
                ptype = p.get("return_type") or p.get("type") or "object"
                default = _default_for_type(ptype)
                # Avoid self-referential: args.Foo = args.Foo
                if default == f"{var_name}.{pname}":
                    default = f"new {ptype}()"
                lines.append(f"{var_name}.{pname} = {default};")
        lines.append("")

    # --- Call methods ---
    real_methods = _filter_real_methods(methods, name, properties)
    if real_methods:
        lines.append("// Call methods")
        for m in real_methods[:8]:
            mname = m["name"]
            ret = m.get("return_type", "void") or "void"
            params = m.get("parameters", [])
            real_params = [
                p for p in params
                if p.get("name", "") not in ("param", "")
                and not p.get("type", "").startswith("var")
                and "." not in p.get("type", "")
            ]
            params_str = ", ".join(
                _default_for_type(p.get("type", "object"))
                for p in real_params
            )
            if ret and ret != "void" and not ret.startswith("throw"):
                lines.append(f"var {_camel(mname)}Result = {var_name}.{mname}({params_str});")
            else:
                lines.append(f"{var_name}.{mname}({params_str});")
        lines.append("")

    return "\n".join(lines)


def generate_all_examples(api_cache: dict[str, Any]) -> None:
    """Generate and attach code examples to all classes in the cache.

    Mutates api_cache["classes"] in place, adding a "code_example" string field.
    """
    for slug, cls in api_cache["classes"].items():
        if cls.get("methods") or cls.get("properties"):
            cls["code_example"] = generate_class_example(cls)
        else:
            cls["code_example"] = ""


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _filter_real_methods(
    methods: list[dict], class_name: str, properties: list[dict] | None = None,
) -> list[dict]:
    """Filter out noise methods (catch, throw, typeof, property accessors, etc.)."""
    skip_names = {
        "catch", "throw", "XFException", "ArgumentNullException",
        "ApplicationException", "typeof", "lock", "using",
        "throw new", "BinaryReader", "BinaryWriter", "MemoryStream",
        "object", "Dictionary", "DataBufferCellPk", "DataBufferCell",
        "DataBufferConstantInfo",
    }
    # Property names should not appear as method calls
    prop_names: set[str] = set()
    if properties:
        for p in properties:
            prop_names.add(p.get("name", ""))
    # Also skip methods whose name matches a known type (these are property getters
    # misdetected as methods in the ILSpy extraction)
    type_like = {
        "AuthenticationToken", "ApplicationToken", "WorkflowUnitClusterPk",
        "DataCellPk", "ThreadSafeObject", "AppServerCallInfo",
        "PageInstanceInfo", "DashboardDynamicGridGetDataArgs",
        "DashboardDynamicGridSaveDataArgs",
    }

    seen: set[str] = set()
    result: list[dict] = []
    for m in methods:
        name = m.get("name", "")
        if not name or name in skip_names:
            continue
        if name.startswith("_") or name == class_name:
            continue
        # Skip if it's a property accessor disguised as a method
        if name in prop_names or name in type_like:
            continue
        ret = m.get("return_type", "")
        if ret and ("throw" in ret or ret == "si," or ret == "?"):
            continue
        # Skip methods returning 'object' with same name as another method (accessor variant)
        if ret == "object" and name in seen:
            continue
        access = m.get("access", "public")
        if access in ("private", "internal"):
            continue
        # Deduplicate by name (keep first overload)
        if name in seen:
            continue
        seen.add(name)
        result.append(m)
    return result


def _has_parameterless_constructor(constructors: list[dict]) -> bool:
    return any(
        not c.get("parameters") or len(c["parameters"]) == 0
        for c in constructors
    )


def _pick_best_constructor(constructors: list[dict]) -> dict:
    """Pick the constructor with fewest real params (but > 0)."""
    valid = [
        c for c in constructors
        if c.get("parameters")
        and all(p.get("name") != "param" for p in c["parameters"])
    ]
    if not valid:
        return constructors[0]
    return min(valid, key=lambda c: len(c.get("parameters", [])))


def _format_ctor_params(ctor: dict) -> str:
    params = ctor.get("parameters", [])
    parts = []
    for p in params:
        ptype = p.get("type", "object")
        parts.append(_default_for_type(ptype))
    return ", ".join(parts)


def _default_for_type(type_name: str) -> str:
    """Return a sensible default value for a C# type."""
    if not type_name:
        return "null"
    # Strip nullability
    clean = type_name.rstrip("?").strip()
    if clean in _TYPE_DEFAULTS:
        return _TYPE_DEFAULTS[clean]
    # Generic collections
    if "Dictionary" in clean:
        return "new Dictionary<string, string>()"
    if "List" in clean:
        return f"new {clean}()"
    if "[]" in clean:
        return f"new {clean.replace('[]', '')}[0]"
    # Enums — use default
    if clean.endswith("Type") or clean.endswith("Enum") or clean.endswith("Flags"):
        return f"default({clean})"
    # Known args
    if clean in _ARG_CLASSES:
        return _ARG_CLASSES[clean]
    # Fallback
    return f"default({clean})"


def _get_var_name(class_name: str) -> str:
    """Generate a camelCase variable name from a class name."""
    if class_name in _ARG_CLASSES:
        return _ARG_CLASSES[class_name]
    # camelCase: first letter lower
    if len(class_name) <= 2:
        return class_name.lower()
    return class_name[0].lower() + class_name[1:]


def _camel(name: str) -> str:
    """Convert PascalCase to camelCase for local variable names."""
    if not name:
        return "value"
    if len(name) <= 2:
        return name.lower()
    return name[0].lower() + name[1:]
