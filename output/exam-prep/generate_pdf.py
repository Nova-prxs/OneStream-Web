#!/usr/bin/env python3
"""Generate a study PDF from the consolidated markdown study material."""

import fitz  # PyMuPDF
import re
import os
from PIL import Image as PILImage

BASE = "/Users/aurelio.santos/Desktop/OneStreamDoc/output/exam-prep"
OUTPUT = os.path.join(BASE, "OS-201_Material_de_Estudio.pdf")

# Read all section files in order
files = [
    "exam-overview.md",
    "section-01-cube.md",
    "section-02-workflow.md",
    "section-03-data-collection.md",
    "section-04-presentation.md",
    "section-05-tools.md",
    "section-06-security.md",
    "section-07-administration.md",
    "section-08-rules.md",
]

# PDF styling constants
PAGE_WIDTH = 595.28  # A4
PAGE_HEIGHT = 841.89
MARGIN_LEFT = 50
MARGIN_RIGHT = 50
MARGIN_TOP = 60
MARGIN_BOTTOM = 60
TEXT_WIDTH = PAGE_WIDTH - MARGIN_LEFT - MARGIN_RIGHT
LINE_HEIGHT = 14
FONT_SIZE_H1 = 20
FONT_SIZE_H2 = 16
FONT_SIZE_H3 = 13
FONT_SIZE_H4 = 11.5
FONT_SIZE_BODY = 10
FONT_SIZE_BULLET = 10
FONT_SIZE_TABLE = 9
HEADER_COLOR = (0.1, 0.2, 0.5)  # Dark blue
H2_COLOR = (0.15, 0.3, 0.55)
H3_COLOR = (0.2, 0.35, 0.6)
BODY_COLOR = (0.1, 0.1, 0.1)
BULLET_COLOR = (0.15, 0.15, 0.15)
ACCENT_COLOR = (0.8, 0.85, 0.95)  # Light blue for highlights
IMG_MAX_WIDTH = TEXT_WIDTH - 20  # Leave some margin for images
IMG_MAX_HEIGHT = 350  # Max image height to avoid huge images
IMG_CAPTION_SIZE = 8


def resolve_image_path(img_path, md_filepath):
    """Resolve an image path from markdown to absolute path."""
    if os.path.isabs(img_path):
        return img_path
    # Relative to the markdown file's directory
    md_dir = os.path.dirname(md_filepath)
    return os.path.join(md_dir, img_path)


def compress_image_to_jpeg(img_path, max_dim=800, quality=60):
    """Compress an image to JPEG bytes, scaling down if needed."""
    with PILImage.open(img_path) as img:
        # Convert to RGB if needed (PNG may have alpha)
        if img.mode in ('RGBA', 'P', 'LA'):
            bg = PILImage.new('RGB', img.size, (255, 255, 255))
            if img.mode == 'P':
                img = img.convert('RGBA')
            bg.paste(img, mask=img.split()[-1] if 'A' in img.mode else None)
            img = bg
        elif img.mode != 'RGB':
            img = img.convert('RGB')

        orig_w, orig_h = img.size
        # Scale down large images
        if orig_w > max_dim or orig_h > max_dim:
            scale = min(max_dim / orig_w, max_dim / orig_h)
            new_w = int(orig_w * scale)
            new_h = int(orig_h * scale)
            img = img.resize((new_w, new_h), PILImage.LANCZOS)

        import io
        buf = io.BytesIO()
        img.save(buf, format='JPEG', quality=quality, optimize=True)
        return buf.getvalue(), img.size[0], img.size[1]


def insert_image(doc, page, y, img_path, caption, page_num):
    """Insert a compressed image into the PDF."""
    if not os.path.exists(img_path):
        return page, y, page_num

    try:
        jpeg_data, img_w, img_h = compress_image_to_jpeg(img_path)

        # Scale to fit within max width/height
        scale = min(IMG_MAX_WIDTH / img_w, IMG_MAX_HEIGHT / img_h, 1.0)
        disp_w = img_w * scale
        disp_h = img_h * scale

        # Space needed: image + caption + padding
        needed = disp_h + 25
        if caption:
            needed += 15

        page, y, page_num = check_space(doc, page, y, needed, page_num)

        y += 5
        # Center the image
        x_offset = MARGIN_LEFT + (TEXT_WIDTH - disp_w) / 2
        img_rect = fitz.Rect(x_offset, y, x_offset + disp_w, y + disp_h)
        page.insert_image(img_rect, stream=jpeg_data)

        # Light border around image
        page.draw_rect(img_rect, color=(0.8, 0.8, 0.8), width=0.5)

        y += disp_h + 5

        # Caption
        if caption:
            cap_rect = fitz.Rect(MARGIN_LEFT + 10, y, PAGE_WIDTH - MARGIN_RIGHT - 10, y + 14)
            page.insert_textbox(cap_rect, caption, fontsize=IMG_CAPTION_SIZE,
                              fontname="helv", color=(0.4, 0.4, 0.4),
                              align=fitz.TEXT_ALIGN_CENTER)
            y += 15

        y += 5
    except Exception as e:
        print(f"  Warning: Could not insert image {img_path}: {e}")

    return page, y, page_num


def new_page(doc):
    page = doc.new_page(width=PAGE_WIDTH, height=PAGE_HEIGHT)
    return page, MARGIN_TOP


def add_page_number(page, num):
    """Add page number at bottom center."""
    rect = fitz.Rect(0, PAGE_HEIGHT - 35, PAGE_WIDTH, PAGE_HEIGHT - 15)
    page.insert_textbox(
        rect, str(num),
        fontsize=8, fontname="helv", color=(0.5, 0.5, 0.5),
        align=fitz.TEXT_ALIGN_CENTER
    )


def check_space(doc, page, y, needed, page_num):
    """Check if we need a new page."""
    if y + needed > PAGE_HEIGHT - MARGIN_BOTTOM:
        add_page_number(page, page_num)
        page_num += 1
        page, y = new_page(doc)
    return page, y, page_num


def clean_markdown_line(line):
    """Remove markdown formatting for plain text."""
    # Bold
    line = re.sub(r'\*\*(.+?)\*\*', r'\1', line)
    # Italic
    line = re.sub(r'\*(.+?)\*', r'\1', line)
    # Code
    line = re.sub(r'`(.+?)`', r'\1', line)
    # Links
    line = re.sub(r'\[(.+?)\]\(.+?\)', r'\1', line)
    # Images
    line = re.sub(r'!\[.*?\]\(.*?\)', '', line)
    return line.strip()


def has_bold(line):
    return '**' in line


def render_text_with_bold(page, rect, text, fontsize, base_color):
    """Render text, handling bold segments by using two passes."""
    clean = clean_markdown_line(text)
    page.insert_textbox(rect, clean, fontsize=fontsize, fontname="helv", color=base_color)


def process_table(lines, start_idx):
    """Extract table rows from markdown table."""
    rows = []
    i = start_idx
    while i < len(lines) and '|' in lines[i]:
        line = lines[i].strip()
        if line.startswith('|') and not re.match(r'^\|[\s\-\|:]+\|$', line):
            cells = [c.strip() for c in line.split('|')[1:-1]]
            rows.append(cells)
        i += 1
    return rows, i


def draw_table(doc, page, y, rows, page_num):
    """Draw a simple table."""
    if not rows:
        return page, y, page_num

    num_cols = len(rows[0])
    col_width = TEXT_WIDTH / num_cols
    row_height = 18

    for row_idx, row in enumerate(rows):
        page, y, page_num = check_space(doc, page, y, row_height + 5, page_num)

        # Background for header row
        if row_idx == 0:
            rect = fitz.Rect(MARGIN_LEFT, y, MARGIN_LEFT + TEXT_WIDTH, y + row_height)
            page.draw_rect(rect, color=None, fill=ACCENT_COLOR)

        for col_idx, cell in enumerate(row):
            if col_idx >= num_cols:
                break
            x = MARGIN_LEFT + col_idx * col_width
            cell_rect = fitz.Rect(x + 3, y + 2, x + col_width - 3, y + row_height)
            clean = clean_markdown_line(cell)
            fs = FONT_SIZE_TABLE
            fname = "hebo" if row_idx == 0 else "helv"
            color = HEADER_COLOR if row_idx == 0 else BODY_COLOR
            page.insert_textbox(cell_rect, clean[:60], fontsize=fs, fontname=fname, color=color)

        # Draw row border
        page.draw_line(
            fitz.Point(MARGIN_LEFT, y + row_height),
            fitz.Point(MARGIN_LEFT + TEXT_WIDTH, y + row_height),
            color=(0.8, 0.8, 0.8), width=0.5
        )
        y += row_height

    y += 8
    return page, y, page_num


def generate_pdf():
    doc = fitz.open()
    page_num = 1

    # --- COVER PAGE ---
    page, y = new_page(doc)

    # Blue header bar
    rect = fitz.Rect(0, 200, PAGE_WIDTH, 380)
    page.draw_rect(rect, color=None, fill=(0.1, 0.2, 0.5))

    # Title
    title_rect = fitz.Rect(MARGIN_LEFT, 215, PAGE_WIDTH - MARGIN_RIGHT, 280)
    page.insert_textbox(
        title_rect, "Material de Estudio",
        fontsize=32, fontname="hebo", color=(1, 1, 1),
        align=fitz.TEXT_ALIGN_CENTER
    )

    subtitle_rect = fitz.Rect(MARGIN_LEFT, 275, PAGE_WIDTH - MARGIN_RIGHT, 320)
    page.insert_textbox(
        subtitle_rect, "Examen OS-201",
        fontsize=26, fontname="helv", color=(0.85, 0.9, 1),
        align=fitz.TEXT_ALIGN_CENTER
    )

    desc_rect = fitz.Rect(MARGIN_LEFT, 320, PAGE_WIDTH - MARGIN_RIGHT, 370)
    page.insert_textbox(
        desc_rect, "OCP OneStream Core Platform Architect",
        fontsize=16, fontname="helv", color=(0.85, 0.9, 1),
        align=fitz.TEXT_ALIGN_CENTER
    )

    # Sections summary
    sections = [
        ("Seccion 1: Cube", "15%"),
        ("Seccion 2: Workflow", "14%"),
        ("Seccion 3: Data Collection", "13%"),
        ("Seccion 4: Presentation", "14%"),
        ("Seccion 5: Tools", "9%"),
        ("Seccion 6: Security", "10%"),
        ("Seccion 7: Administration", "9%"),
        ("Seccion 8: Rules", "16%"),
    ]

    y = 430
    for name, weight in sections:
        r = fitz.Rect(MARGIN_LEFT + 80, y, PAGE_WIDTH - MARGIN_RIGHT - 80, y + 22)
        page.insert_textbox(r, f"{name}  —  {weight}", fontsize=12, fontname="helv",
                          color=BODY_COLOR, align=fitz.TEXT_ALIGN_CENTER)
        y += 22

    # Footer
    footer_rect = fitz.Rect(MARGIN_LEFT, PAGE_HEIGHT - 80, PAGE_WIDTH - MARGIN_RIGHT, PAGE_HEIGHT - 50)
    page.insert_textbox(
        footer_rect, "Generado para preparacion del examen de certificacion OneStream",
        fontsize=9, fontname="helv", color=(0.5, 0.5, 0.5),
        align=fitz.TEXT_ALIGN_CENTER
    )
    add_page_number(page, page_num)
    page_num += 1

    # --- CONTENT PAGES ---
    for file_idx, filename in enumerate(files):
        filepath = os.path.join(BASE, filename)
        if not os.path.exists(filepath):
            continue

        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        lines = content.split('\n')

        # Start each major section on a new page (except overview which follows cover)
        page, y = new_page(doc)

        i = 0
        in_code_block = False
        in_details = False

        while i < len(lines):
            line = lines[i]

            # Insert images
            img_match = re.match(r'!\[([^\]]*)\]\(([^)]+)\)', line.strip())
            if img_match:
                caption = img_match.group(1)
                img_path = img_match.group(2)
                abs_path = resolve_image_path(img_path, filepath)
                page, y, page_num = insert_image(doc, page, y, abs_path, caption, page_num)
                i += 1
                continue

            # Skip HTML tags like <details>, <summary>, </details>
            if re.match(r'^\s*</?details', line.strip()) or re.match(r'^\s*</?summary', line.strip()):
                i += 1
                continue

            # Code blocks
            if line.strip().startswith('```'):
                in_code_block = not in_code_block
                i += 1
                continue

            if in_code_block:
                page, y, page_num = check_space(doc, page, y, LINE_HEIGHT + 2, page_num)
                clean = line.rstrip()[:100]
                rect = fitz.Rect(MARGIN_LEFT + 15, y, PAGE_WIDTH - MARGIN_RIGHT, y + LINE_HEIGHT)
                page.insert_textbox(rect, clean, fontsize=8.5, fontname="cour", color=(0.3, 0.3, 0.3))
                y += LINE_HEIGHT - 1
                i += 1
                continue

            # Horizontal rules / page separators
            if re.match(r'^---+\s*$', line.strip()):
                page, y, page_num = check_space(doc, page, y, 20, page_num)
                y += 5
                page.draw_line(
                    fitz.Point(MARGIN_LEFT, y),
                    fitz.Point(MARGIN_LEFT + TEXT_WIDTH, y),
                    color=(0.75, 0.75, 0.75), width=0.5
                )
                y += 15
                i += 1
                continue

            # Empty lines
            if not line.strip():
                y += 6
                i += 1
                continue

            # H1
            if line.startswith('# '):
                page, y, page_num = check_space(doc, page, y, 45, page_num)
                text = clean_markdown_line(line[2:])

                # Blue bar accent
                bar_rect = fitz.Rect(MARGIN_LEFT, y, MARGIN_LEFT + 4, y + 28)
                page.draw_rect(bar_rect, color=None, fill=HEADER_COLOR)

                rect = fitz.Rect(MARGIN_LEFT + 12, y + 2, PAGE_WIDTH - MARGIN_RIGHT, y + 30)
                page.insert_textbox(rect, text, fontsize=FONT_SIZE_H1, fontname="hebo", color=HEADER_COLOR)
                y += 38
                i += 1
                continue

            # H2
            if line.startswith('## '):
                page, y, page_num = check_space(doc, page, y, 35, page_num)
                y += 8
                text = clean_markdown_line(line[3:])
                rect = fitz.Rect(MARGIN_LEFT, y, PAGE_WIDTH - MARGIN_RIGHT, y + 24)
                page.insert_textbox(rect, text, fontsize=FONT_SIZE_H2, fontname="hebo", color=H2_COLOR)
                y += 28

                # Underline
                page.draw_line(
                    fitz.Point(MARGIN_LEFT, y - 4),
                    fitz.Point(MARGIN_LEFT + TEXT_WIDTH * 0.4, y - 4),
                    color=H2_COLOR, width=1
                )
                i += 1
                continue

            # H3
            if line.startswith('### '):
                page, y, page_num = check_space(doc, page, y, 30, page_num)
                y += 6
                text = clean_markdown_line(line[4:])
                rect = fitz.Rect(MARGIN_LEFT, y, PAGE_WIDTH - MARGIN_RIGHT, y + 20)
                page.insert_textbox(rect, text, fontsize=FONT_SIZE_H3, fontname="hebo", color=H3_COLOR)
                y += 24
                i += 1
                continue

            # H4
            if line.startswith('#### '):
                page, y, page_num = check_space(doc, page, y, 25, page_num)
                y += 4
                text = clean_markdown_line(line[5:])
                rect = fitz.Rect(MARGIN_LEFT, y, PAGE_WIDTH - MARGIN_RIGHT, y + 18)
                page.insert_textbox(rect, text, fontsize=FONT_SIZE_H4, fontname="hebo", color=H3_COLOR)
                y += 20
                i += 1
                continue

            # Tables
            if '|' in line and line.strip().startswith('|'):
                rows, next_i = process_table(lines, i)
                if rows:
                    page, y, page_num = draw_table(doc, page, y, rows, page_num)
                i = next_i
                continue

            # Bullet points
            if re.match(r'^(\s*)[-*]\s', line):
                indent_match = re.match(r'^(\s*)[-*]\s(.+)', line)
                if indent_match:
                    indent_level = len(indent_match.group(1)) // 2
                    text = clean_markdown_line(indent_match.group(2))
                    indent = MARGIN_LEFT + 10 + (indent_level * 15)

                    # Calculate needed height
                    chars_per_line = int((PAGE_WIDTH - MARGIN_RIGHT - indent - 15) / (FONT_SIZE_BULLET * 0.5))
                    num_lines = max(1, (len(text) // chars_per_line) + 1)
                    needed = num_lines * (LINE_HEIGHT - 1) + 4

                    page, y, page_num = check_space(doc, page, y, needed, page_num)

                    # Bullet dot
                    bullet_char = "•" if indent_level == 0 else "◦"
                    page.insert_text(fitz.Point(indent, y + 10), bullet_char,
                                   fontsize=FONT_SIZE_BULLET, fontname="helv", color=BULLET_COLOR)

                    rect = fitz.Rect(indent + 12, y, PAGE_WIDTH - MARGIN_RIGHT, y + needed + 2)
                    page.insert_textbox(rect, text, fontsize=FONT_SIZE_BULLET, fontname="helv", color=BULLET_COLOR)
                    y += needed
                i += 1
                continue

            # Numbered lists
            num_match = re.match(r'^(\s*)(\d+)\.\s(.+)', line)
            if num_match:
                indent_level = len(num_match.group(1)) // 2
                number = num_match.group(2)
                text = clean_markdown_line(num_match.group(3))
                indent = MARGIN_LEFT + 10 + (indent_level * 15)

                chars_per_line = int((PAGE_WIDTH - MARGIN_RIGHT - indent - 20) / (FONT_SIZE_BODY * 0.5))
                num_lines = max(1, (len(text) // chars_per_line) + 1)
                needed = num_lines * (LINE_HEIGHT - 1) + 4

                page, y, page_num = check_space(doc, page, y, needed, page_num)

                page.insert_text(fitz.Point(indent, y + 10), f"{number}.",
                               fontsize=FONT_SIZE_BODY, fontname="hebo", color=BULLET_COLOR)

                rect = fitz.Rect(indent + 18, y, PAGE_WIDTH - MARGIN_RIGHT, y + needed + 2)
                page.insert_textbox(rect, text, fontsize=FONT_SIZE_BODY, fontname="helv", color=BULLET_COLOR)
                y += needed
                i += 1
                continue

            # Regular paragraph text
            text = clean_markdown_line(line)
            if text:
                chars_per_line = int(TEXT_WIDTH / (FONT_SIZE_BODY * 0.52))
                num_lines = max(1, (len(text) // chars_per_line) + 1)
                needed = num_lines * (LINE_HEIGHT - 1) + 4

                page, y, page_num = check_space(doc, page, y, needed, page_num)

                rect = fitz.Rect(MARGIN_LEFT, y, PAGE_WIDTH - MARGIN_RIGHT, y + needed + 4)
                fontname = "hebo" if has_bold(line) and line.strip().startswith('**') else "helv"
                page.insert_textbox(rect, text, fontsize=FONT_SIZE_BODY, fontname=fontname, color=BODY_COLOR)
                y += needed

            i += 1

        # Add page number to last page of section
        add_page_number(page, page_num)
        page_num += 1

    doc.save(OUTPUT)
    doc.close()
    print(f"PDF generado: {OUTPUT}")
    print(f"Paginas: {page_num - 1}")


if __name__ == "__main__":
    generate_pdf()
