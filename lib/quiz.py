"""Quiz question parser and cache builder.

Parses all 618 OS-201 exam questions from markdown files into structured
Python data for the quiz engine.

Exports
-------
SECTIONS
    List of 8 section dicts with number, slug, name, weight, file.
parse_questions(content)
    Parse markdown content into a list of question dicts.
build_quiz_cache(exam_prep_dir)
    Process all 8 question files and return the quiz cache.
"""

from __future__ import annotations

import re
from pathlib import Path


# ---------------------------------------------------------------------------
# Section definitions
# ---------------------------------------------------------------------------

SECTIONS: list[dict] = [
    {
        "number": 1,
        "slug": "cube",
        "name": "Cube",
        "weight": 15,
        "file": "questions-01-cube.md",
    },
    {
        "number": 2,
        "slug": "workflow",
        "name": "Workflow",
        "weight": 14,
        "file": "questions-02-workflow.md",
    },
    {
        "number": 3,
        "slug": "data-collection",
        "name": "Data Collection",
        "weight": 13,
        "file": "questions-03-data-collection.md",
    },
    {
        "number": 4,
        "slug": "presentation",
        "name": "Presentation",
        "weight": 14,
        "file": "questions-04-presentation.md",
    },
    {
        "number": 5,
        "slug": "tools",
        "name": "Tools",
        "weight": 9,
        "file": "questions-05-tools.md",
    },
    {
        "number": 6,
        "slug": "security",
        "name": "Security",
        "weight": 10,
        "file": "questions-06-security.md",
    },
    {
        "number": 7,
        "slug": "administration",
        "name": "Administration",
        "weight": 9,
        "file": "questions-07-administration.md",
    },
    {
        "number": 8,
        "slug": "rules",
        "name": "Rules",
        "weight": 16,
        "file": "questions-08-rules.md",
    },
]


# ---------------------------------------------------------------------------
# Question parser
# ---------------------------------------------------------------------------

# Regex patterns
_RE_QUESTION_NUM = re.compile(r"### Question (\d+)")
_RE_DIFFICULTY = re.compile(r"Difficulty:\s*(Easy|Medium|Hard)")
_RE_OBJECTIVE = re.compile(r"\*\*(\d+\.\d+\.\d+)\*\*")
_RE_OPTION = re.compile(r"^([A-D])\)\s*(.+)", re.MULTILINE)
_RE_CORRECT = re.compile(r"\*\*Correct answer:\s*([A-D])\)")
_RE_DETAILS_OPEN = re.compile(r"<details>")
_RE_DETAILS_CLOSE = re.compile(r"</details>")


def parse_questions(content: str) -> list[dict]:
    """Parse markdown *content* into a list of question dicts.

    Each dict has keys: number, difficulty, objective, text, options,
    correct_answer, explanation.
    """
    questions: list[dict] = []

    # Split on "### Question" headers, keeping the header
    blocks = re.split(r"(?=### Question \d+)", content)

    for block in blocks:
        block = block.strip()
        if not block.startswith("### Question"):
            continue

        # Number
        m_num = _RE_QUESTION_NUM.search(block)
        if not m_num:
            continue
        number = int(m_num.group(1))

        # Difficulty
        m_diff = _RE_DIFFICULTY.search(block)
        difficulty = m_diff.group(1) if m_diff else "Unknown"

        # Objective
        m_obj = _RE_OBJECTIVE.search(block)
        objective = m_obj.group(1) if m_obj else ""

        # Split block into pre-details and details sections
        details_match = _RE_DETAILS_OPEN.search(block)
        if details_match:
            pre_details = block[: details_match.start()]
            post_details_start = details_match.end()
            details_close = _RE_DETAILS_CLOSE.search(block, post_details_start)
            if details_close:
                details_content = block[post_details_start : details_close.start()]
            else:
                details_content = block[post_details_start:]
        else:
            pre_details = block
            details_content = ""

        # Question text: between the metadata line and first option
        # The metadata line is like: **201.1.1** | Difficulty: Easy
        lines = pre_details.split("\n")
        text_lines: list[str] = []
        in_text = False
        for line in lines:
            stripped = line.strip()
            # Skip header line and metadata line
            if stripped.startswith("### Question"):
                continue
            if _RE_OBJECTIVE.search(stripped) and _RE_DIFFICULTY.search(stripped):
                in_text = True
                continue
            if in_text:
                # Stop at first option line
                if _RE_OPTION.match(stripped):
                    break
                text_lines.append(stripped)

        text = "\n".join(text_lines).strip()

        # Options
        options: list[dict] = []
        option_matches = list(_RE_OPTION.finditer(pre_details))
        for i, m_opt in enumerate(option_matches):
            letter = m_opt.group(1)
            # Option text runs until next option or end of pre_details
            start = m_opt.end()
            if i + 1 < len(option_matches):
                end = option_matches[i + 1].start()
            else:
                end = len(pre_details)
            # Get full option text (may span multiple lines)
            full_text = m_opt.group(2).strip()
            remaining = pre_details[start:end].strip()
            # If there are continuation lines before the next option
            if remaining:
                extra_lines = []
                for rl in remaining.split("\n"):
                    rl_stripped = rl.strip()
                    if rl_stripped and not _RE_OPTION.match(rl_stripped):
                        extra_lines.append(rl_stripped)
                    else:
                        break
                if extra_lines:
                    full_text = full_text + " " + " ".join(extra_lines)
            options.append({"letter": letter, "text": full_text})

        # Correct answer
        m_correct = _RE_CORRECT.search(details_content)
        correct_answer = m_correct.group(1) if m_correct else ""

        # Explanation: everything after the "Correct answer" line in details
        explanation = ""
        if m_correct:
            after_correct = details_content[m_correct.end() :].strip()
            # Remove markdown bold markers and summary tags
            after_correct = re.sub(r"</?summary>", "", after_correct)
            after_correct = re.sub(r"Show answer", "", after_correct)
            after_correct = re.sub(r"\*\*", "", after_correct)
            explanation = after_correct.strip()

        questions.append(
            {
                "number": number,
                "difficulty": difficulty,
                "objective": objective,
                "text": text,
                "options": options,
                "correct_answer": correct_answer,
                "explanation": explanation,
            }
        )

    return questions


# ---------------------------------------------------------------------------
# Cache builder
# ---------------------------------------------------------------------------


def build_quiz_cache(exam_prep_dir: Path) -> dict:
    """Read all 8 question files from *exam_prep_dir* and return quiz cache.

    Returns
    -------
    dict
        ``{"sections": [{"number": 1, "slug": "cube", "name": "Cube",
        "weight": 15, "question_count": 90, "questions": [...]}, ...]}``
    """
    sections_data: list[dict] = []

    for section in SECTIONS:
        filepath = exam_prep_dir / section["file"]
        content = filepath.read_text(encoding="utf-8")
        questions = parse_questions(content)

        print(
            f"  Quiz: {section['name']} — {len(questions)} questions "
            f"({section['weight']}% weight)"
        )

        sections_data.append(
            {
                "number": section["number"],
                "slug": section["slug"],
                "name": section["name"],
                "weight": section["weight"],
                "question_count": len(questions),
                "questions": questions,
            }
        )

    total = sum(s["question_count"] for s in sections_data)
    print(f"  Quiz loaded: {len(sections_data)} sections, {total} questions")

    return {"sections": sections_data}
