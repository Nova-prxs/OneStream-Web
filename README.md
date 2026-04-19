# OneStream Portal

Self-contained web application for **OS-201 OneStream Core Platform Architect** certification preparation and comprehensive API documentation.

## Features

### Exam Prep
- **4 reference books** with 60 chapters (600k+ words) rendered from Markdown
- **618 quiz questions** across 8 exam sections with answer feedback and explanations
- **Exam simulation** mode with timed sessions
- **Flashcards**, glossary, study objectives, and section summaries
- **Full-text search** across all chapters (SQLite FTS5)
- **Progress tracking** with per-section stats (localStorage)

### API Reference
- **5,323 classes** across 288 namespaces with method signatures and properties
- Full-text search across all API entries
- Namespace filtering

### Business Rules
- **827 real business rules** organized by type: Extender, Finance, Connector, Dashboard, and Assemblies
- Syntax-highlighted VB.NET code viewer
- Category-based browsing

### Extension Guide
- VS Code extension documentation with 12 features
- MCP server integration with 6 tool categories

## Tech Stack

- **Backend**: Python / Flask
- **Frontend**: Nova Design System (custom CSS, no framework) with light/dark theme
- **Search**: SQLite FTS5 (in-memory)
- **Icons**: Phosphor Icons
- **Fonts**: Inter + JetBrains Mono

No build step, no npm, no external APIs. Single `python app.py` command.

### Nova Design System

The portal UI uses a custom design system (`Nova`). All styling lives in:

- `static/css/nova-tokens.css` — design tokens (colors, spacing, typography, radii, shadows)
- `static/css/nova-app.css` — components and layout (sidebar, cards, pillars, tables, code blocks, etc.)
- `static/css/pygments.css` — Pygments default light styles (dark-mode tokens overridden in `nova-app.css`)
- `static/css/style.css` — runtime-only highlight classes injected by `static/js/highlights.js`

Brand assets in `static/img/`: `nova-logo.png` (sidebar mark), `bg-coral.jpg`, `bg-teal.jpg`, `bg-dark.png` (hero card grain). The `design_handoff_onestream_nova_portal/` directory contains the original handoff reference (do not load at runtime).

## Quick Start

```bash
# Install dependencies
pip install -r requirements.txt

# Run
python app.py
```

The app starts at `http://localhost:5001`.

## Docker

```bash
# Build and run
docker build -t onestream-prep .
docker run -d -p 5050:5001 --restart unless-stopped --name onestream-prep onestream-prep
```

## Project Structure

```
app.py                                  # Flask application (routes, caching, API)
lib/                                    # Backend modules (search, API docs, rules, extensions)
templates/                              # Jinja2 templates (extend base.html / docs_base.html)
static/css/                             # Nova Design System stylesheets
static/js/                              # highlights.js (text highlighting), theme-toggle.js
static/img/                             # Brand assets (nova-logo.png, hero textures)
output/                                 # Rendered book chapters and images
content/                                # Extension guide markdown
Business Rule/                          # VB.NET business rule source files
OneStreamMCP/                           # MCP server knowledge base
design_handoff_onestream_nova_portal/   # Original Nova design handoff (reference only)
```

## Routes

| Path | Purpose |
|---|---|
| `/` | Portal landing page |
| `/tools/pomodoro` | Pomodoro timer |
| `/exam-prep/<exam>/quiz` | Quiz selector / quiz runner |
| `/exam-prep/<exam>/exam` | Timed 60-question exam simulation |
| `/exam-prep/<exam>/progress` | Per-section progress tracking |
| `/exam-prep/<exam>/flashcards` | Flashcards |
| `/books/<book>/<chapter>` | Reference book chapter viewer |
| `/glossary`, `/highlights` | Glossary and saved highlights |
| `/docs/api/`, `/docs/api/<class>` | API reference index + class detail |
| `/docs/rules/`, `/docs/rules/<type>/<rule>` | Business rules browser + source viewer |
| `/docs/extension/` | VS Code extension + MCP server guide |
| `/search` | Full-text search across chapters |

## License

Internal use only.
