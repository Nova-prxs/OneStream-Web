"""Extension Guide — Parse Markdown content files and build a structured cache.

Exports
-------
build_extension_guide_cache(content_dir)
    Reads _meta.json and all Markdown files, renders to HTML, returns cache dict.
"""

from __future__ import annotations

import json
import re
from pathlib import Path

import markdown


def _render_markdown(text: str) -> str:
    """Render Markdown text to HTML with code highlighting and heading anchors."""
    md = markdown.Markdown(
        extensions=[
            "fenced_code",
            "codehilite",
            "tables",
            "toc",
            "attr_list",
        ],
        extension_configs={
            "codehilite": {"css_class": "highlight", "guess_lang": False},
            "toc": {"permalink": False, "slugify": _slugify},
        },
    )
    html = md.convert(text)
    toc_html = getattr(md, "toc", "")
    md.reset()
    return html, toc_html


def _slugify(value: str, separator: str = "-") -> str:
    """Simple ASCII slugify for heading anchors."""
    value = re.sub(r"[^\w\s-]", "", value.lower())
    return re.sub(r"[\s_]+", separator, value).strip(separator)


def _extract_title(text: str) -> str:
    """Extract the first # heading from Markdown text."""
    for line in text.splitlines():
        line = line.strip()
        if line.startswith("# "):
            return line[2:].strip()
    return "Untitled"


def build_extension_guide_cache(content_dir: Path) -> dict:
    """Build the extension guide cache from Markdown content files.

    Returns a dict with keys:
        pages: dict[slug, page_dict]
        sections: list of section dicts from _meta.json
        categories: dict[category_name, list of section dicts]
        stats: dict with feature_count, mcp_tool_count, setting_count, total_pages
    """
    meta_path = content_dir / "_meta.json"
    if not meta_path.exists():
        return {"pages": {}, "sections": [], "categories": {}, "stats": {}}

    with open(meta_path, encoding="utf-8") as f:
        meta = json.load(f)

    sections = meta.get("sections", [])
    pages: dict[str, dict] = {}
    categories: dict[str, list] = {}

    for section in sections:
        slug = section["slug"]
        category = section.get("category", "guide")

        # Find the Markdown file
        md_path = _find_md_file(content_dir, slug, category)
        if md_path is None or not md_path.exists():
            continue

        text = md_path.read_text(encoding="utf-8")
        title = _extract_title(text)
        html, toc_html = _render_markdown(text)

        page = {
            "slug": slug,
            "title": title,
            "category": category,
            "icon": section.get("icon", "file"),
            "html": html,
            "toc": toc_html,
            "meta_title": section.get("title", title),
        }
        pages[slug] = page

        if category not in categories:
            categories[category] = []
        categories[category].append(page)

    stats = {
        "feature_count": len(categories.get("features", [])),
        "mcp_tool_count": len(categories.get("mcp", [])),
        "total_pages": len(pages),
    }

    return {
        "pages": pages,
        "sections": sections,
        "categories": categories,
        "stats": stats,
    }


def _find_md_file(content_dir: Path, slug: str, category: str) -> Path | None:
    """Locate the Markdown file for a given slug and category."""
    # Direct match at root level
    root_path = content_dir / f"{slug}.md"
    if root_path.exists():
        return root_path

    # Category subdirectory mappings
    subdir_map = {
        "features": "features",
        "mcp": "mcp-server",
    }

    subdir = subdir_map.get(category)
    if subdir:
        # Try slug directly
        sub_path = content_dir / subdir / f"{slug}.md"
        if sub_path.exists():
            return sub_path

        # Try removing category prefix (e.g., "mcp-connection" -> "connection-tools")
        if slug.startswith("mcp-"):
            alt_slug = slug[4:] + "-tools"
            alt_path = content_dir / subdir / f"{alt_slug}.md"
            if alt_path.exists():
                return alt_path
            # Also try just the suffix
            alt_path2 = content_dir / subdir / f"{slug[4:]}.md"
            if alt_path2.exists():
                return alt_path2

    return None
