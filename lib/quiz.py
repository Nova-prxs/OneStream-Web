"""Quiz question parser and cache builder — multi-exam support.

Parses exam questions from markdown files into structured Python data
for the quiz engine. Supports OS-102, OS-201, OS-300, and OS-301 exams.

Exports
-------
EXAMS
    Dict mapping exam_id to exam metadata (name, title, level, sections).
parse_questions(content)
    Parse markdown content into a list of question dicts.
build_quiz_cache(exam_prep_dir, exam_id)
    Process question files for a single exam and return the quiz cache.
build_all_quiz_caches(exam_prep_dir)
    Build quiz caches for all registered exams.
"""

from __future__ import annotations

import re
from pathlib import Path


# ---------------------------------------------------------------------------
# Exam registry — each exam has its own sections and question files
# ---------------------------------------------------------------------------

EXAMS: dict[str, dict] = {
    "os-102": {
        "id": "os-102",
        "name": "OS-102",
        "title": "OCA OneStream Administration R2",
        "level": "Associate",
        "passing_score": 70,
        "exam_questions": 60,
        "exam_duration": 90,
        "sections": [
            {"number": 1, "slug": "building-blocks", "name": "Building Blocks of Administration", "weight": 11, "file": "questions-01-building-blocks.md"},
            {"number": 2, "slug": "app-properties", "name": "Application Properties & Dimensions", "weight": 17, "file": "questions-02-app-properties.md"},
            {"number": 3, "slug": "cubes-extensibility", "name": "Cubes and Extensibility", "weight": 12, "file": "questions-03-cubes-extensibility.md"},
            {"number": 4, "slug": "workflow-start", "name": "Workflow Start", "weight": 11, "file": "questions-04-workflow-start.md"},
            {"number": 5, "slug": "importing-data", "name": "Importing Data", "weight": 13, "file": "questions-05-importing-data.md"},
            {"number": 6, "slug": "data-entry", "name": "Data Entry", "weight": 14, "file": "questions-06-data-entry.md"},
            {"number": 7, "slug": "completing-workflow", "name": "Completing the Workflow", "weight": 10, "file": "questions-07-completing-workflow.md"},
            {"number": 8, "slug": "security", "name": "Working with Security", "weight": 12, "file": "questions-08-security.md"},
        ],
    },
    "os-201": {
        "id": "os-201",
        "name": "OS-201",
        "title": "OCS Certification",
        "level": "Specialist",
        "passing_score": 70,
        "exam_questions": 60,
        "exam_duration": 90,
        "sections": [
            {"number": 1, "slug": "cube", "name": "Cube", "weight": 15, "file": "questions-01-cube.md"},
            {"number": 2, "slug": "workflow", "name": "Workflow", "weight": 14, "file": "questions-02-workflow.md"},
            {"number": 3, "slug": "data-collection", "name": "Data Collection", "weight": 13, "file": "questions-03-data-collection.md"},
            {"number": 4, "slug": "presentation", "name": "Presentation", "weight": 14, "file": "questions-04-presentation.md"},
            {"number": 5, "slug": "tools", "name": "Tools", "weight": 9, "file": "questions-05-tools.md"},
            {"number": 6, "slug": "security", "name": "Security", "weight": 10, "file": "questions-06-security.md"},
            {"number": 7, "slug": "administration", "name": "Administration", "weight": 9, "file": "questions-07-administration.md"},
            {"number": 8, "slug": "rules", "name": "Rules", "weight": 16, "file": "questions-08-rules.md"},
        ],
    },
    "os-300": {
        "id": "os-300",
        "name": "OS-300",
        "title": "OCS Reports & Dashboards",
        "level": "Specialist",
        "passing_score": 70,
        "exam_questions": 60,
        "exam_duration": 90,
        "sections": [
            {"number": 1, "slug": "workspaces", "name": "Workspaces", "weight": 17, "file": "questions-01-workspaces.md"},
            {"number": 2, "slug": "dashboard-parameters", "name": "Dashboard Parameters", "weight": 18, "file": "questions-02-dashboard-parameters.md"},
            {"number": 3, "slug": "cube-views", "name": "Cube Views", "weight": 23, "file": "questions-03-cube-views.md"},
            {"number": 4, "slug": "spreadsheet", "name": "Spreadsheet", "weight": 12, "file": "questions-04-spreadsheet.md"},
            {"number": 5, "slug": "dashboard-components", "name": "Dashboard Components", "weight": 20, "file": "questions-05-dashboard-components.md"},
            {"number": 6, "slug": "other-reporting", "name": "Other Reporting Components", "weight": 10, "file": "questions-06-other-reporting.md"},
        ],
    },
    "os-301": {
        "id": "os-301",
        "name": "OS-301",
        "title": "OCS OneStream Financial Close",
        "level": "Specialist",
        "passing_score": 70,
        "exam_questions": 60,
        "exam_duration": 90,
        "sections": [
            {"number": 1, "slug": "rcm", "name": "Reconciliation Control Manager (RCM)", "weight": 32, "file": "questions-01-rcm.md"},
            {"number": 2, "slug": "txm", "name": "Transaction Matching (TXM)", "weight": 28, "file": "questions-02-txm.md"},
            {"number": 3, "slug": "integration", "name": "Integration", "weight": 18, "file": "questions-03-integration.md"},
            {"number": 4, "slug": "general-config", "name": "General Configuration", "weight": 22, "file": "questions-04-general-config.md"},
        ],
    },
}

# Backward compatibility: keep SECTIONS pointing to OS-201
SECTIONS = EXAMS["os-201"]["sections"]


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
        lines = pre_details.split("\n")
        text_lines: list[str] = []
        in_text = False
        for line in lines:
            stripped = line.strip()
            if stripped.startswith("### Question"):
                continue
            if _RE_OBJECTIVE.search(stripped) and _RE_DIFFICULTY.search(stripped):
                in_text = True
                continue
            if in_text:
                if _RE_OPTION.match(stripped):
                    break
                text_lines.append(stripped)

        text = "\n".join(text_lines).strip()

        # Options
        options: list[dict] = []
        option_matches = list(_RE_OPTION.finditer(pre_details))
        for i, m_opt in enumerate(option_matches):
            letter = m_opt.group(1)
            start = m_opt.end()
            if i + 1 < len(option_matches):
                end = option_matches[i + 1].start()
            else:
                end = len(pre_details)
            full_text = m_opt.group(2).strip()
            remaining = pre_details[start:end].strip()
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

        # Explanation
        explanation = ""
        if m_correct:
            after_correct = details_content[m_correct.end() :].strip()
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


def build_quiz_cache(exam_prep_dir: Path, exam_id: str = "os-201") -> dict:
    """Read question files for *exam_id* from *exam_prep_dir* and return quiz cache.

    For OS-201, looks first in exam_prep_dir/os-201/, then falls back to
    exam_prep_dir/ for backward compatibility.

    Returns
    -------
    dict
        ``{"exam": {...exam_meta}, "sections": [{...section_data}, ...]}``
    """
    exam = EXAMS.get(exam_id)
    if not exam:
        raise ValueError(f"Unknown exam: {exam_id}")

    sections_data: list[dict] = []

    # Try exam subdirectory first, then fallback to root dir (OS-201 compat)
    exam_dir = exam_prep_dir / exam_id
    if not exam_dir.is_dir():
        exam_dir = exam_prep_dir  # fallback for OS-201

    for section in exam["sections"]:
        filepath = exam_dir / section["file"]
        if not filepath.exists():
            print(
                f"  Quiz: {exam_id} / {section['name']} — SKIPPED (file not found: {filepath})"
            )
            continue

        content = filepath.read_text(encoding="utf-8")
        questions = parse_questions(content)

        print(
            f"  Quiz: {exam_id} / {section['name']} — {len(questions)} questions "
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
    print(f"  Quiz loaded: {exam_id} — {len(sections_data)} sections, {total} questions")

    return {
        "exam": {
            "id": exam["id"],
            "name": exam["name"],
            "title": exam["title"],
            "level": exam["level"],
            "passing_score": exam["passing_score"],
            "exam_questions": exam["exam_questions"],
            "exam_duration": exam["exam_duration"],
        },
        "sections": sections_data,
    }


def build_all_quiz_caches(exam_prep_dir: Path) -> dict[str, dict]:
    """Build quiz caches for all registered exams.

    Returns a dict mapping exam_id to its quiz cache.
    Only includes exams that have at least one question file present.
    """
    caches: dict[str, dict] = {}

    for exam_id in EXAMS:
        exam_dir = exam_prep_dir / exam_id
        # Also check root dir for OS-201 backward compat
        has_files = exam_dir.is_dir() and any(exam_dir.glob("questions-*.md"))
        if not has_files and exam_id == "os-201":
            has_files = any(exam_prep_dir.glob("questions-*.md"))

        if has_files:
            caches[exam_id] = build_quiz_cache(exam_prep_dir, exam_id)

    return caches
