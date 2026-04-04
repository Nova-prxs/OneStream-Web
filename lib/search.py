"""SQLite FTS5 full-text search across all OneStream chapters.

Exports
-------
build_search_index(content_cache)
    Creates an in-memory SQLite FTS5 index from the pre-rendered content cache.
search_chapters(conn, query, limit=20)
    Searches the FTS5 index and returns ranked results with highlighted snippets.
"""

from __future__ import annotations

import re
import sqlite3


_TAG_RE = re.compile(r"<[^>]+>")
# FTS5 special characters that can cause query syntax errors
_FTS5_SPECIAL_RE = re.compile(r'["\*\^\(\)\{\}\[\]:!]')


def build_search_index(content_cache: dict) -> sqlite3.Connection:
    """Create an in-memory SQLite FTS5 index from *content_cache*.

    Parameters
    ----------
    content_cache : dict
        Cache returned by ``build_content_cache()`` with shape
        ``{"books": [...], "chapters": {"file_key": {...}, ...}}``.

    Returns
    -------
    sqlite3.Connection
        In-memory SQLite connection with a populated ``chapters_fts`` table.
    """
    conn = sqlite3.connect(":memory:", check_same_thread=False)
    conn.execute(
        "CREATE VIRTUAL TABLE chapters_fts USING fts5("
        "title, body, book_title, file_key, "
        "tokenize='porter unicode61'"
        ")"
    )

    for file_key, chapter in content_cache["chapters"].items():
        title = chapter["metadata"]["title"]
        book_title = chapter["book_title"]
        # Strip HTML tags to get plain text for indexing
        body = _TAG_RE.sub(" ", chapter["html"])
        conn.execute(
            "INSERT INTO chapters_fts(title, body, book_title, file_key) "
            "VALUES (?, ?, ?, ?)",
            (title, body, book_title, file_key),
        )

    conn.commit()
    return conn


def search_chapters(
    conn: sqlite3.Connection,
    query: str,
    limit: int = 20,
) -> list[dict]:
    """Search the FTS5 index for *query* and return ranked results.

    Parameters
    ----------
    conn : sqlite3.Connection
        Connection returned by ``build_search_index()``.
    query : str
        User search query.
    limit : int
        Maximum number of results (default 20).

    Returns
    -------
    list[dict]
        Each dict has keys: ``title``, ``snippet``, ``book_title``,
        ``file_key``, ``score``.
    """
    if not query or not query.strip():
        return []

    # Sanitize: remove FTS5 special characters to prevent syntax errors
    sanitized = _FTS5_SPECIAL_RE.sub(" ", query).strip()
    if not sanitized:
        return []

    try:
        cursor = conn.execute(
            "SELECT title, "
            "snippet(chapters_fts, 1, '<mark>', '</mark>', '...', 32) as snippet, "
            "book_title, file_key, rank "
            "FROM chapters_fts WHERE chapters_fts MATCH ? "
            "ORDER BY rank "
            "LIMIT ?",
            (sanitized, limit),
        )
    except sqlite3.OperationalError:
        # Malformed query that slipped past sanitization
        return []

    results = []
    for row in cursor.fetchall():
        results.append(
            {
                "title": row[0],
                "snippet": row[1],
                "book_title": row[2],
                "file_key": row[3],
                "score": row[4],
            }
        )

    return results


_STOP_WORDS = frozenset(
    "the a an is are was were be been being have has had do does did will "
    "would shall should may might can could that this these those with from "
    "into through during before after above below between under again further "
    "then once here there when where which what who whom how each every all "
    "both some most other such only same than also very just about over "
    "following correct answer question option because however therefore "
    "used using within does does not".split()
)


def find_related_chapter(
    conn: sqlite3.Connection,
    question_text: str,
    explanation: str,
    section_name: str = "",
) -> dict | None:
    """Find the most relevant chapter for a quiz question.

    Uses the question text, explanation, and section name to search the
    FTS5 index and returns the top-ranked chapter link.

    Returns
    -------
    dict or None
        ``{"book_slug": ..., "chapter_slug": ..., "title": ..., "book_title": ...}``
    """
    raw = f"{section_name} {question_text} {explanation}"
    sanitized = _FTS5_SPECIAL_RE.sub(" ", raw).strip().lower()
    if not sanitized:
        return None

    # Extract distinctive words, removing stop words and short words
    words = [w for w in sanitized.split() if len(w) > 3 and w not in _STOP_WORDS]
    if not words:
        return None

    # Deduplicate while preserving order
    seen: set[str] = set()
    unique: list[str] = []
    for w in words:
        if w not in seen:
            seen.add(w)
            unique.append(w)

    top_words = unique[:10]

    # Strategy: try progressively broader queries
    # 1. First try top 4-5 words as implicit AND (most precise)
    # 2. Fall back to OR with all words (broadest)
    row = None
    for attempt in [
        " ".join(top_words[:5]),           # AND top 5
        " ".join(top_words[:3]),           # AND top 3
        " OR ".join(top_words[:8]),        # OR top 8
    ]:
        try:
            cursor = conn.execute(
                "SELECT title, book_title, file_key, rank "
                "FROM chapters_fts WHERE chapters_fts MATCH ? "
                "ORDER BY rank "
                "LIMIT 1",
                (attempt,),
            )
            row = cursor.fetchone()
            if row:
                break
        except sqlite3.OperationalError:
            continue

    if not row:
        return None

    file_key = row[2]
    parts = file_key.split("/")
    book_slug = parts[0]
    chapter_slug = parts[1].replace(".md", "") if len(parts) > 1 else ""

    return {
        "book_slug": book_slug,
        "chapter_slug": chapter_slug,
        "title": row[0],
        "book_title": row[1],
    }
