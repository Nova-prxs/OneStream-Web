"""Content loading, markdown rendering, and image path rewriting.

Exports
-------
build_content_cache(output_dir)
    Pre-renders every chapter to HTML and returns a structured cache dict.
"""

from __future__ import annotations

import json
import re
from pathlib import Path

import markdown

# ---------------------------------------------------------------------------
# Book display names (slug → human-readable)
# ---------------------------------------------------------------------------

BOOK_TITLES: dict[str, str] = {
    "design-reference-guide": "Design & Reference Guide",
    "finance-rules": "Finance Rules & Calculations Handbook",
    "foundation-handbook": "Foundation Handbook",
    "workspaces-assemblies": "Workspaces & Assemblies",
}


# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------


def strip_frontmatter(content: str) -> str:
    """Remove YAML frontmatter delimited by ``---`` from *content*.

    If *content* starts with ``---``, split on ``---`` (max 3 parts) and
    return everything after the closing delimiter.  Otherwise return the
    string unchanged.
    """
    if content.startswith("---"):
        parts = content.split("---", 2)
        if len(parts) >= 3:
            return parts[2].lstrip("\n")
    return content


def rewrite_image_paths(html: str, book_slug: str) -> str:
    """Rewrite relative ``images/`` src to ``/content/{book}/images/``.

    Also adds ``loading="lazy"`` to every ``<img>`` tag for performance.
    """
    # Replace src="images/..." with absolute path.
    # Python-Markdown renders ![](images/X.png) as <img alt="" src="images/X.png" />
    # so we need to handle optional attributes before src.
    html = re.sub(
        r'(<img\b[^>]*?)src="images/([^"]+)"',
        rf'\1loading="lazy" src="/content/{book_slug}/images/\2"',
        html,
    )
    return html


# ---------------------------------------------------------------------------
# Main entry point
# ---------------------------------------------------------------------------


def build_content_cache(output_dir: Path) -> dict:
    """Read *output_dir*/index.json, render every chapter, return cache.

    Returns
    -------
    dict
        ``{"books": [...], "chapters": {"file_path": {...}, ...}}``
    """

    with open(output_dir / "index.json", encoding="utf-8") as fh:
        index = json.load(fh)

    # Create a single Markdown instance with the required extensions
    md = markdown.Markdown(
        extensions=[
            "tables",
            "fenced_code",
            "codehilite",
            "toc",
            "sane_lists",
        ],
        extension_configs={
            "toc": {"toc_depth": "2-3", "permalink": False},
            "codehilite": {
                "css_class": "highlight",
                "guess_lang": False,
                "linenums": False,
            },
        },
    )

    books_list: list[dict] = []
    chapters_dict: dict[str, dict] = {}

    for book_info in index["books"]:
        book_slug: str = book_info["book"]
        book_title = BOOK_TITLES.get(book_slug, book_slug.replace("-", " ").title())
        book_chapters: list[dict] = []

        print(f"  Loading {book_slug}... {book_info['chapter_count']} chapters")

        for ch_meta in book_info["chapters"]:
            filepath = output_dir / ch_meta["file"]
            raw = filepath.read_text(encoding="utf-8")
            content = strip_frontmatter(raw)

            # CRITICAL: reset before each render so toc doesn't accumulate
            md.reset()
            html = md.convert(content)
            toc = md.toc

            # Rewrite image paths AFTER rendering
            html = rewrite_image_paths(html, book_slug)

            file_key: str = ch_meta["file"]
            chapters_dict[file_key] = {
                "html": html,
                "toc": toc,
                "metadata": ch_meta,
                "book_slug": book_slug,
                "book_title": book_title,
            }

            book_chapters.append(
                {
                    "title": ch_meta["title"],
                    "file": file_key,
                    "word_count": ch_meta.get("word_count", 0),
                    "image_count": ch_meta.get("image_count", 0),
                }
            )

        books_list.append(
            {
                "slug": book_slug,
                "title": book_title,
                "chapter_count": book_info["chapter_count"],
                "chapters": book_chapters,
            }
        )

    total_words = sum(ch["metadata"]["word_count"] for ch in chapters_dict.values())
    print(
        f"  Content loaded: {len(books_list)} books, "
        f"{len(chapters_dict)} chapters, {total_words:,} words"
    )

    return {"books": books_list, "chapters": chapters_dict}
