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
- **Frontend**: Pico CSS + custom Nova Praxis theme (light/dark mode)
- **Search**: SQLite FTS5 (in-memory)
- **Icons**: Phosphor Icons
- **Fonts**: Inter + JetBrains Mono

No build step, no npm, no external APIs. Single `python app.py` command.

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
app.py              # Flask application (routes, caching, API)
lib/                # Backend modules (search, API docs, rules, extensions)
templates/          # Jinja2 templates
static/             # CSS, JS, icons
output/             # Rendered book chapters and images
content/            # Extension guide markdown
Business Rule/      # VB.NET business rule source files
OneStreamMCP/       # MCP server knowledge base
```

## License

Internal use only.
