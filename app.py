"""OneStream Portal — Flask application entry point.

Two sections:
  /exam-prep/  — OS-201 Certification prep (study guide, quizzes, flashcards)
  /docs/       — API reference + business rules browser with search
"""

from pathlib import Path

import re as _re

from flask import Flask, abort, redirect, render_template, request, send_from_directory

app = Flask(__name__, static_folder="static")

OUTPUT_DIR = Path("output")
EXAM_PREP_DIR = Path("output/exam-prep")
KNOWLEDGE_BASE_DIR = Path("OneStreamMCP/knowledge_base")
RULES_DIR = Path("Business Rule")
EXTENSION_GUIDE_DIR = Path("content/extension-guide")


# ===========================================================================
# Lazy-load caches
# ===========================================================================


def get_content_cache():
    """Lazy-load content cache (handles Flask reloader child process)."""
    if "CONTENT_CACHE" not in app.config:
        from lib.content import build_content_cache

        app.config["CONTENT_CACHE"] = build_content_cache(OUTPUT_DIR)
    return app.config["CONTENT_CACHE"]


def get_quiz_cache():
    """Lazy-load quiz cache (handles Flask reloader child process)."""
    if "QUIZ_CACHE" not in app.config:
        from lib.quiz import build_quiz_cache

        app.config["QUIZ_CACHE"] = build_quiz_cache(EXAM_PREP_DIR)
        _enrich_quiz_with_chapter_links()
        _build_chapter_questions_map()
    return app.config["QUIZ_CACHE"]


def _enrich_quiz_with_chapter_links():
    """Add related_chapter to each quiz question using FTS5 search."""
    from lib.search import find_related_chapter

    conn = get_search_index()
    quiz_cache = app.config["QUIZ_CACHE"]
    linked = 0
    for section in quiz_cache["sections"]:
        for question in section["questions"]:
            result = find_related_chapter(
                conn,
                question["text"],
                question.get("explanation", ""),
                section["name"],
            )
            question["related_chapter"] = result
            if result:
                linked += 1
    total = sum(s["question_count"] for s in quiz_cache["sections"])
    print(f"  Quiz linking: {linked}/{total} questions linked to chapters")


def _build_chapter_questions_map():
    """Build reverse mapping: chapter file_key -> related quiz questions."""
    quiz_cache = app.config["QUIZ_CACHE"]
    chapter_questions: dict[str, list[dict]] = {}
    for section in quiz_cache["sections"]:
        for q in section["questions"]:
            rc = q.get("related_chapter")
            if rc:
                key = f"{rc['book_slug']}/{rc['chapter_slug']}.md"
                if key not in chapter_questions:
                    chapter_questions[key] = []
                chapter_questions[key].append(
                    {
                        "text": q["text"],
                        "options": q["options"],
                        "correct_answer": q["correct_answer"],
                        "explanation": q.get("explanation", ""),
                        "difficulty": q["difficulty"],
                        "objective": q.get("objective", ""),
                        "section_name": section["name"],
                    }
                )
    app.config["CHAPTER_QUESTIONS"] = chapter_questions
    print(f"  Chapter quiz map: {len(chapter_questions)} chapters with questions")


def get_search_index():
    """Lazy-load FTS5 search index (handles Flask reloader child process)."""
    if "SEARCH_INDEX" not in app.config:
        from lib.search import build_search_index

        app.config["SEARCH_INDEX"] = build_search_index(get_content_cache())
    return app.config["SEARCH_INDEX"]


def get_api_cache():
    """Lazy-load API documentation cache."""
    if "API_CACHE" not in app.config:
        from lib.api_docs import build_api_cache
        from lib.rules_browser import build_rules_cache, link_apis_to_rules
        from lib.code_examples import generate_all_examples

        api_cache = build_api_cache(KNOWLEDGE_BASE_DIR)
        rules_cache = build_rules_cache(RULES_DIR)
        link_apis_to_rules(api_cache, rules_cache)
        generate_all_examples(api_cache)

        app.config["API_CACHE"] = api_cache
        app.config["RULES_CACHE"] = rules_cache
    return app.config["API_CACHE"]


def get_rules_cache():
    """Lazy-load business rules cache."""
    if "RULES_CACHE" not in app.config:
        get_api_cache()  # triggers both caches
    return app.config["RULES_CACHE"]


def get_api_search_index():
    """Lazy-load FTS5 search index for API docs."""
    if "API_SEARCH_INDEX" not in app.config:
        from lib.api_docs import build_api_search_index

        app.config["API_SEARCH_INDEX"] = build_api_search_index(get_api_cache())
    return app.config["API_SEARCH_INDEX"]


def get_extension_guide_cache():
    """Lazy-load extension guide cache."""
    if "EXTENSION_GUIDE_CACHE" not in app.config:
        from lib.extension_guide import build_extension_guide_cache

        app.config["EXTENSION_GUIDE_CACHE"] = build_extension_guide_cache(
            EXTENSION_GUIDE_DIR
        )
    return app.config["EXTENSION_GUIDE_CACHE"]


# ===========================================================================
# Template filters
# ===========================================================================


@app.template_filter("chapter_slug")
def chapter_slug_filter(file_path):
    """Extract chapter slug from file path: 'book/chapter-08-cubes.md' -> 'chapter-08-cubes'."""
    return file_path.rsplit("/", 1)[-1].replace(".md", "")


# ===========================================================================
# Portal landing page
# ===========================================================================


@app.route("/")
def portal():
    """Landing page with links to Exam Prep and API Docs."""
    api_cache = get_api_cache()
    rules_cache = get_rules_cache()
    total_methods = sum(
        c["method_count"] for c in api_cache["classes"].values()
    )
    api_stats = {
        "class_count": len(api_cache["classes"]),
        "method_count": total_methods,
        "rule_count": len(rules_cache["rules"]),
    }
    ext_cache = get_extension_guide_cache()
    ext_stats = ext_cache.get("stats", {})
    return render_template("portal.html", api_stats=api_stats, ext_stats=ext_stats)


# ===========================================================================
# Exam Prep routes (previously at /)
# ===========================================================================


@app.route("/exam-prep/")
def index():
    """Home page listing all books."""
    cache = get_content_cache()
    return render_template("index.html", books=cache["books"])


@app.route("/exam-prep/guide/<book_slug>")
def book_view(book_slug):
    """Book page listing chapters with titles and word counts."""
    cache = get_content_cache()
    book = next((b for b in cache["books"] if b["slug"] == book_slug), None)
    if not book:
        abort(404)
    return render_template("book.html", book=book)


@app.route("/exam-prep/guide/<book_slug>/<chapter_slug>")
def chapter_view(book_slug, chapter_slug):
    """Chapter reading view with TOC, content, and prev/next navigation."""
    cache = get_content_cache()
    file_key = f"{book_slug}/{chapter_slug}.md"
    chapter_data = cache["chapters"].get(file_key)
    if not chapter_data:
        abort(404)
    book = next((b for b in cache["books"] if b["slug"] == book_slug), None)
    chapters = book["chapters"]
    current_idx = next(
        (i for i, c in enumerate(chapters) if c["file"] == file_key), None
    )
    prev_chapter = (
        chapters[current_idx - 1] if current_idx and current_idx > 0 else None
    )
    next_chapter = (
        chapters[current_idx + 1]
        if current_idx is not None and current_idx < len(chapters) - 1
        else None
    )
    get_quiz_cache()
    mini_quiz = app.config.get("CHAPTER_QUESTIONS", {}).get(file_key, [])[:10]

    return render_template(
        "chapter.html",
        chapter=chapter_data,
        book=book,
        prev_chapter=prev_chapter,
        next_chapter=next_chapter,
        mini_quiz=mini_quiz,
    )


@app.route("/content/<book>/images/<filename>")
def serve_image(book, filename):
    """Serve chapter images from output/{book}/images/."""
    images_dir = OUTPUT_DIR / book / "images"
    if not images_dir.is_dir():
        abort(404)
    response = send_from_directory(images_dir, filename)
    response.cache_control.max_age = 86400 * 30
    response.cache_control.public = True
    return response


@app.route("/exam-prep/quiz")
def quiz_sections():
    """Quiz landing page listing all 8 exam sections."""
    cache = get_quiz_cache()
    total_questions = sum(s["question_count"] for s in cache["sections"])
    return render_template(
        "quiz_sections.html",
        sections=cache["sections"],
        total_questions=total_questions,
    )


@app.route("/exam-prep/quiz/<section_slug>")
def quiz_section(section_slug):
    """Interactive quiz for a single exam section."""
    cache = get_quiz_cache()
    section = next((s for s in cache["sections"] if s["slug"] == section_slug), None)
    if not section:
        abort(404)
    return render_template("quiz.html", section=section)


@app.route("/exam-prep/exam")
def exam_simulation():
    """Exam simulation page — 60 weighted random questions with timer."""
    cache = get_quiz_cache()
    return render_template("exam.html", sections=cache["sections"])


@app.route("/api/quiz/<section_slug>")
def api_quiz_section(section_slug):
    """API endpoint returning quiz questions as JSON (for lighter page loads)."""
    from flask import jsonify

    cache = get_quiz_cache()
    section = next((s for s in cache["sections"] if s["slug"] == section_slug), None)
    if not section:
        abort(404)
    return jsonify(section)


@app.route("/exam-prep/search")
def search():
    """Full-text search across all chapters."""
    from lib.search import search_chapters

    query = request.args.get("q", "").strip()
    results = []
    if query:
        conn = get_search_index()
        results = search_chapters(conn, query)
    return render_template("search.html", query=query, results=results)


@app.route("/exam-prep/progress")
def progress():
    """Progress dashboard showing per-section quiz stats."""
    cache = get_quiz_cache()
    return render_template("progress.html", sections=cache["sections"])


@app.route("/exam-prep/review")
def review():
    """Review failed questions across all sections."""
    cache = get_quiz_cache()
    return render_template("review.html", sections=cache["sections"])


@app.route("/exam-prep/objectives")
def objectives():
    """Study plan organized by certification objectives."""
    cache = get_quiz_cache()
    obj_map: dict[str, dict] = {}
    for section in cache["sections"]:
        for q in section["questions"]:
            obj = q.get("objective", "")
            if not obj:
                continue
            if obj not in obj_map:
                obj_map[obj] = {
                    "code": obj,
                    "section_name": section["name"],
                    "section_slug": section["slug"],
                    "questions": [],
                }
            obj_map[obj]["questions"].append(q)
    sorted_obj = sorted(obj_map.values(), key=lambda x: x["code"])
    return render_template(
        "objectives.html", objectives=sorted_obj, sections=cache["sections"]
    )


@app.route("/exam-prep/glossary")
def glossary():
    """Searchable glossary of OneStream terms from chapter headings."""
    cache = get_content_cache()
    terms: dict[str, dict] = {}
    for file_key, chapter in cache["chapters"].items():
        book_slug = chapter["book_slug"]
        ch_slug = file_key.rsplit("/", 1)[-1].replace(".md", "")
        for m in _re.finditer(
            r'<h[23][^>]*id="([^"]*)"[^>]*>(.*?)</h[23]>', chapter["html"]
        ):
            hid = m.group(1)
            text = _re.sub(r"<[^>]+>", "", m.group(2)).strip()
            if len(text) < 3 or len(text) > 100:
                continue
            key = text.lower()
            if key not in terms:
                terms[key] = {
                    "term": text,
                    "book_slug": book_slug,
                    "chapter_slug": ch_slug,
                    "chapter_title": chapter["metadata"]["title"],
                    "book_title": chapter["book_title"],
                    "anchor": hid,
                }
    sorted_terms = sorted(terms.values(), key=lambda x: x["term"].lower())
    return render_template("glossary.html", terms=sorted_terms)


@app.route("/exam-prep/flashcards")
def flashcards():
    """Spaced repetition flashcards from quiz questions."""
    cache = get_quiz_cache()
    return render_template("flashcards.html", sections=cache["sections"])


@app.route("/exam-prep/summary")
def summary():
    """Cheat sheets / key facts per exam section."""
    cache = get_quiz_cache()
    return render_template("summary.html", sections=cache["sections"])


@app.route("/exam-prep/highlights")
def highlights():
    """Highlights summary — all user-highlighted text (client-side localStorage)."""
    return render_template("highlights.html")


# ===========================================================================
# Legacy redirects (301) — old routes without /exam-prep/ prefix
# ===========================================================================

_LEGACY_ROUTES = [
    "/quiz", "/exam", "/search", "/progress", "/review",
    "/objectives", "/glossary", "/flashcards", "/summary", "/highlights",
]


@app.route("/guide/<path:rest>")
def legacy_guide(rest):
    return redirect(f"/exam-prep/guide/{rest}", code=301)


for _old in _LEGACY_ROUTES:

    def _make_redirect(old_path):
        def _redirect_fn(**kwargs):
            new = old_path.replace("/", "/exam-prep/", 1)
            qs = request.query_string.decode()
            return redirect(f"{new}?{qs}" if qs else new, code=301)

        _redirect_fn.__name__ = f"legacy_{old_path.strip('/')}"
        return _redirect_fn

    app.add_url_rule(_old, endpoint=f"legacy_{_old.strip('/')}", view_func=_make_redirect(_old))


# ===========================================================================
# API Documentation routes
# ===========================================================================


@app.route("/docs/")
def docs_index():
    """API reference index — list all classes by namespace."""
    api_cache = get_api_cache()
    return render_template(
        "api_index.html",
        classes=api_cache["class_list"],
        namespaces=api_cache["namespaces"],
    )


@app.route("/docs/search")
def docs_search():
    """Search across API classes, methods, and enums."""
    from lib.api_docs import search_api_docs

    query = request.args.get("q", "").strip()
    results = []
    if query:
        conn = get_api_search_index()
        results = search_api_docs(conn, query)
    return render_template("api_search.html", query=query, results=results)


@app.route("/docs/<class_slug>")
def docs_class(class_slug):
    """API class detail page with methods, properties, and usage examples."""
    api_cache = get_api_cache()
    cls = api_cache["classes"].get(class_slug)
    if not cls:
        abort(404)
    return render_template("api_detail.html", cls=cls)


# ===========================================================================
# Business Rules routes
# ===========================================================================


@app.route("/docs/rules/")
def rules_index():
    """Business rules catalog — list all types."""
    rules_cache = get_rules_cache()
    total = len(rules_cache["rules"])
    return render_template(
        "rules_index.html",
        rule_types=rules_cache["types"],
        total_rules=total,
    )


@app.route("/docs/rules/<type_slug>")
def rules_type(type_slug):
    """List all rules of a given type."""
    rules_cache = get_rules_cache()
    rule_type = next(
        (t for t in rules_cache["types"] if t["slug"] == type_slug), None
    )
    if not rule_type:
        abort(404)
    rules = [
        rules_cache["rules"][slug]
        for slug in rule_type["rule_slugs"]
        if slug in rules_cache["rules"]
    ]
    # Build a map for subcategory template lookups
    rules_map = {r["full_slug"]: r for r in rules}
    subcategories = rule_type.get("subcategories", [])
    return render_template(
        "rules_type.html",
        rule_type=rule_type,
        rules=rules,
        rules_map=rules_map,
        subcategories=subcategories,
    )


@app.route("/docs/rules/<type_slug>/<rule_slug>")
@app.route("/docs/rules/<type_slug>/<workspace_slug>/<rule_slug>")
def rule_detail(type_slug, rule_slug, workspace_slug=None):
    """Individual business rule detail page."""
    rules_cache = get_rules_cache()
    if workspace_slug:
        full_slug = f"{type_slug}/{workspace_slug}/{rule_slug}"
    else:
        full_slug = f"{type_slug}/{rule_slug}"
    rule = rules_cache["rules"].get(full_slug)
    if not rule:
        abort(404)
    return render_template("rule_detail.html", rule=rule)


# ===========================================================================
# Extension Guide routes
# ===========================================================================


@app.route("/docs/extension/")
def extension_index():
    """Extension guide landing page."""
    cache = get_extension_guide_cache()
    return render_template(
        "extension_index.html",
        sections=cache["sections"],
        categories=cache["categories"],
        stats=cache["stats"],
    )


@app.route("/docs/extension/<page_slug>")
def extension_page(page_slug):
    """Individual extension guide page."""
    cache = get_extension_guide_cache()
    page = cache["pages"].get(page_slug)
    if not page:
        abort(404)
    return render_template(
        "extension_page.html",
        page=page,
        sections=cache["sections"],
        categories=cache["categories"],
    )


# ===========================================================================
# Startup
# ===========================================================================

if __name__ == "__main__":
    from lib.content import build_content_cache
    from lib.quiz import build_quiz_cache
    from lib.search import build_search_index
    from lib.api_docs import build_api_cache, build_api_search_index
    from lib.rules_browser import build_rules_cache, link_apis_to_rules
    from lib.code_examples import generate_all_examples

    # Exam prep content
    cache = build_content_cache(OUTPUT_DIR)
    app.config["CONTENT_CACHE"] = cache
    total_books = len(cache["books"])
    total_chapters = len(cache["chapters"])
    total_words = sum(ch["metadata"]["word_count"] for ch in cache["chapters"].values())
    print(
        f"\n  Startup summary: {total_books} books, {total_chapters} chapters, {total_words:,} words"
    )

    quiz_cache = build_quiz_cache(EXAM_PREP_DIR)
    app.config["QUIZ_CACHE"] = quiz_cache
    total_questions = sum(s["question_count"] for s in quiz_cache["sections"])
    print(
        f"  Quiz loaded: {len(quiz_cache['sections'])} sections, {total_questions} questions"
    )

    search_conn = build_search_index(cache)
    app.config["SEARCH_INDEX"] = search_conn
    row_count = search_conn.execute("SELECT COUNT(*) FROM chapters_fts").fetchone()[0]
    print(f"  Search index: {row_count} chapters indexed")

    _enrich_quiz_with_chapter_links()
    _build_chapter_questions_map()

    # API docs + rules
    api_cache = build_api_cache(KNOWLEDGE_BASE_DIR)
    rules_cache = build_rules_cache(RULES_DIR)
    link_apis_to_rules(api_cache, rules_cache)
    generate_all_examples(api_cache)
    app.config["API_CACHE"] = api_cache
    app.config["RULES_CACHE"] = rules_cache

    api_search_conn = build_api_search_index(api_cache)
    app.config["API_SEARCH_INDEX"] = api_search_conn

    print(
        f"  API docs: {len(api_cache['classes'])} classes, "
        f"{len(api_cache['enums'])} enums, "
        f"{len(rules_cache['rules'])} rules"
    )
    api_fts_count = api_search_conn.execute("SELECT COUNT(*) FROM api_fts").fetchone()[0]
    print(f"  API search index: {api_fts_count} entries")
    print()

    import os

    host = os.environ.get("HOST", "127.0.0.1")
    debug = os.environ.get("FLASK_DEBUG", "1") == "1"
    app.run(host=host, port=5001, debug=debug)
