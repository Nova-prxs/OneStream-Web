"""Extract the Financial Close Guide PDF into markdown chapters with images.

Usage:
    python scripts/extract_financial_close.py <pdf_path> <output_dir>

Example:
    python scripts/extract_financial_close.py \
        "C:/Users/.../Financial Close Guide-OFC_PV900_SV202_Instructions.pdf" \
        output/financial-close-guide
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

import fitz  # PyMuPDF


# ---------------------------------------------------------------------------
# Chapter definitions (from the PDF table of contents)
# ---------------------------------------------------------------------------

BOOK_SLUG = "financial-close-guide"

CHAPTERS: list[dict] = [
    {"num": 1,  "title": "Overview",                              "start": 1,   "end": 1,    "slug": "overview"},
    {"num": 2,  "title": "Setup and Installation",                "start": 2,   "end": 17,   "slug": "setup-and-installation"},
    {"num": 3,  "title": "Account Reconciliations Settings",      "start": 18,  "end": 49,   "slug": "account-reconciliations-settings"},
    {"num": 4,  "title": "Reconciliation Administration",         "start": 50,  "end": 107,  "slug": "reconciliation-administration"},
    {"num": 5,  "title": "Notifications",                         "start": 108, "end": 110,  "slug": "notifications"},
    {"num": 6,  "title": "Security",                              "start": 111, "end": 132,  "slug": "security"},
    {"num": 7,  "title": "Using Account Reconciliations",         "start": 133, "end": 199,  "slug": "using-account-reconciliations"},
    {"num": 8,  "title": "Transaction Matching Settings",         "start": 200, "end": 249,  "slug": "transaction-matching-settings"},
    {"num": 9,  "title": "Transaction Matching Administration",   "start": 250, "end": 299,  "slug": "transaction-matching-administration"},
    {"num": 10, "title": "Using Transaction Matching",            "start": 300, "end": 340,  "slug": "using-transaction-matching"},
    {"num": 11, "title": "Integration",                           "start": 341, "end": 389,  "slug": "integration"},
    {"num": 12, "title": "Journal Entries",                       "start": 390, "end": 410,  "slug": "journal-entries"},
    {"num": 13, "title": "Troubleshooting and Best Practices",    "start": 411, "end": 430,  "slug": "troubleshooting-and-best-practices"},
    {"num": 14, "title": "Appendices",                            "start": 431, "end": 456,  "slug": "appendices"},
]


def extract_images(doc: fitz.Document, page_idx: int, chapter_num: int,
                   images_dir: Path) -> list[str]:
    """Extract images from a single page, return list of markdown refs."""
    refs: list[str] = []
    page = doc[page_idx]
    page_num = page_idx + 1  # 1-based

    for img_idx, img_info in enumerate(page.get_images(full=True)):
        xref = img_info[0]
        try:
            pix = fitz.Pixmap(doc, xref)
        except Exception:
            continue

        # Skip tiny images (icons, bullets, etc.)
        if pix.width < 50 or pix.height < 50:
            continue

        # Convert CMYK to RGB if needed
        if pix.n > 4:
            pix = fitz.Pixmap(fitz.csRGB, pix)

        img_name = f"{BOOK_SLUG}-ch{chapter_num:02d}-p{page_num}-{xref}.png"
        img_path = images_dir / img_name
        pix.save(str(img_path))
        refs.append(f"![](images/{img_name})")

    return refs


def extract_text_blocks(page: fitz.Page) -> str:
    """Extract text from a page preserving structure."""
    blocks = page.get_text("blocks")
    lines: list[str] = []

    for block in sorted(blocks, key=lambda b: (b[1], b[0])):
        text = block[4].strip()
        if not text:
            continue
        # Skip page footers
        if text.startswith("Financial Close Guide") and len(text) < 40:
            continue
        if re.match(r"^\d+$", text):  # page numbers
            continue
        lines.append(text)

    return "\n\n".join(lines)


def pages_to_markdown(doc: fitz.Document, start_page: int, end_page: int,
                      chapter_num: int, images_dir: Path) -> tuple[str, int, int]:
    """Convert a range of PDF pages to markdown with images.

    Returns (markdown_content, word_count, image_count).
    """
    sections: list[str] = []
    total_images = 0

    for page_idx in range(start_page - 1, min(end_page, len(doc))):
        page = doc[page_idx]

        # Extract text
        text = extract_text_blocks(page)

        # Extract images
        img_refs = extract_images(doc, page_idx, chapter_num, images_dir)
        total_images += len(img_refs)

        if text:
            sections.append(text)
        for ref in img_refs:
            sections.append(ref)

    content = "\n\n".join(sections)
    word_count = len(content.split())

    return content, word_count, total_images


def apply_heading_heuristics(content: str) -> str:
    """Apply heading formatting heuristics to extracted text.

    Detects lines that are likely headings (short, title-cased, standalone)
    and converts them to markdown headings.
    """
    lines = content.split("\n")
    result: list[str] = []

    # Known section headers from the TOC
    known_headers = {
        "overview", "setup and installation", "dependencies",
        "account reconciliations", "transaction matching",
        "settings", "global setup", "control lists", "column settings",
        "templates", "access control", "certifications", "uninstall",
        "reconciliation administration", "reconciliation definition",
        "reconciliation inventory", "account groups", "tracking",
        "balcheck", "autorec", "dynamic attribute mapping",
        "notifications", "security", "roles", "segregation of duties",
        "dashboard security", "workflow actions", "using account reconciliations",
        "transaction matching settings", "transaction matching administration",
        "using transaction matching", "integration", "journal entries",
        "troubleshooting", "best practices", "appendix",
        "complex expressions", "custom filter dashboard",
    }

    for line in lines:
        stripped = line.strip()
        lower = stripped.lower()

        # Check if it's a known header
        is_known = any(lower.startswith(h) or h.startswith(lower) for h in known_headers if len(lower) > 3)

        # Heuristic: short line, not ending in period, likely a heading
        if (is_known or (len(stripped) < 80 and not stripped.endswith(".")
                         and not stripped.endswith(",") and stripped
                         and stripped[0].isupper()
                         and len(stripped.split()) <= 8
                         and not stripped.startswith("![")
                         and not stripped.startswith("-")
                         and not stripped.startswith("•"))):
            if is_known and len(stripped.split()) <= 5:
                result.append(f"\n## {stripped}\n")
            elif len(stripped.split()) <= 6 and len(stripped) < 60:
                result.append(f"\n### {stripped}\n")
            else:
                result.append(stripped)
        else:
            result.append(stripped)

    return "\n".join(result)


def main() -> None:
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)

    pdf_path = Path(sys.argv[1])
    output_dir = Path(sys.argv[2])
    images_dir = output_dir / "images"
    images_dir.mkdir(parents=True, exist_ok=True)

    print(f"Opening PDF: {pdf_path}")
    doc = fitz.open(str(pdf_path))
    print(f"  Total pages: {len(doc)}")

    index_chapters: list[dict] = []

    for ch in CHAPTERS:
        print(f"  Chapter {ch['num']:2d}: {ch['title']} (pages {ch['start']}-{ch['end']})")

        content, word_count, image_count = pages_to_markdown(
            doc, ch["start"], ch["end"], ch["num"], images_dir
        )

        # Apply heading heuristics
        content = apply_heading_heuristics(content)

        # Build frontmatter
        slug = f"chapter-{ch['num']:02d}-{ch['slug']}"
        frontmatter = (
            f"---\n"
            f"title: \"{ch['title']}\"\n"
            f"book: \"{BOOK_SLUG}\"\n"
            f"chapter: {ch['num']}\n"
            f"start_page: {ch['start']}\n"
            f"end_page: {ch['end']}\n"
            f"---\n\n"
            f"# {ch['title']}\n\n"
        )

        md_path = output_dir / f"{slug}.md"
        md_path.write_text(frontmatter + content, encoding="utf-8")

        print(f"    -> {slug}.md ({word_count} words, {image_count} images)")

        index_chapters.append({
            "title": ch["title"],
            "file": f"{BOOK_SLUG}/{slug}.md",
            "book": BOOK_SLUG,
            "chapter": ch["num"],
            "start_page": ch["start"],
            "end_page": ch["end"],
            "word_count": word_count,
            "image_count": image_count,
        })

    doc.close()

    # Print summary
    total_words = sum(c["word_count"] for c in index_chapters)
    total_images = sum(c["image_count"] for c in index_chapters)
    print(f"\n  Summary: {len(index_chapters)} chapters, {total_words:,} words, {total_images} images")

    # Write a partial index for merging into output/index.json
    partial_index = {
        "book": BOOK_SLUG,
        "chapter_count": len(index_chapters),
        "chapters": index_chapters,
    }
    partial_path = output_dir / "_book_index.json"
    partial_path.write_text(json.dumps(partial_index, indent=2), encoding="utf-8")
    print(f"  Partial index written to: {partial_path}")


if __name__ == "__main__":
    main()
