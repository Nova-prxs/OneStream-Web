# CLAUDE.md — OneStream Portal

Project context for Claude Code sessions. Read first.

## What this project is

A self-contained Flask app that serves OneStream certification prep + API/Rules documentation locally at `http://localhost:5001`. No build step, no JS framework, no external APIs. See `README.md` for routes and tech stack.

The standalone dataset-generation pipeline lives under `Business Rule/` and has its own `CLAUDE.md` — do not conflate the two.

## Frontend conventions

The UI is the **Nova Design System** (custom CSS, no framework). Pico CSS was removed; do not reintroduce it.

**Always use Nova tokens, never raw hex/rgb:**

| Use | Token |
|---|---|
| Brand coral (accents, primary CTA) | `var(--nova-coral)` |
| Brand teal (sidebar dark, secondary) | `var(--nova-teal)` |
| Brand ink (dark surface) | `var(--nova-ink)` |
| Brand aqua (light fill) | `var(--nova-aqua)` |
| Page bg | `var(--color-bg)` |
| Card surface | `var(--nova-paper)` |
| Borders | `var(--nova-line)` / `var(--nova-line-soft)` |
| Body text / muted | `var(--color-text)` / `var(--color-text-2)` / `var(--color-text-3)` |

Full token list: `static/css/nova-tokens.css`. Components: `static/css/nova-app.css`.

**Templates extend `base.html`** (sidebar shell with brand mark, nav groups, theme toggle) **or `docs_base.html`** (same shell + on-page TOC sidebar for chapter/api/rule pages).

**Reusable component classes** (search nova-app.css before inventing new ones):

- `.nv-hero` — page eyebrow + h1 block
- `.nv-crumb` — breadcrumbs
- `.nv-card`, `.nv-card--table` — card surface, table-in-card variant
- `.nv-pillar.coral|teal|ink|aqua` — hero pillar cards (with `.pillar-eyebrow`, `h3`, `p`, `.pillar-cta`, `.pillar-glyph`)
- `.nv-grid`, `.nv-grid--2|3|4` — card grids
- `.nv-btn`, `.nv-btn--primary|ghost` — buttons
- `.highlighted-code` — Pygments code block wrapper (rule_detail / chapter)
- `.nv-eyebrow`, `.nv-sidebar`, `.nv-nav-group`, `.nv-nav-label`

## Dark mode

Theme is set on `<html data-theme="light|dark">` and persisted in `localStorage` by `static/js/theme-toggle.js`. Pygments tokens are remapped for dark in `nova-app.css` (search `[data-theme="dark"] .highlight`). When adding hardcoded colors, always pair with a dark override.

## Code blocks (rule_detail / chapter)

- Wrapper class `highlighted-code` with Pygments-rendered `<table class="highlighttable">` inside.
- `tab-size: 4` enforced (do not rely on browser default 8).
- Linenos and code `pre` must share font-size + line-height to stay aligned.
- `.err` border suppressed (Pygments flags VB `$"..."` interpolated strings as errors).

## Brand mark

Sidebar logo uses `static/img/nova-logo.png` rendered via `background-image` on `.nv-brand-mark` with `background-size: contain`. The PNG already contains the full coral "nova" 2x2 glyph on transparent bg — do not paint a `background-color` underneath it.

## Local dev

```bash
python app.py    # serves on :5001
```

For UI verification, use Playwright MCP at `http://localhost:5001/`. Test light + dark before declaring a UI task done. Save artifacts under `.playwright-mcp/`.

## Don'ts

- Don't reintroduce Pico CSS or any CSS framework.
- Don't hardcode hex colors in templates or components — use Nova tokens.
- Don't create new top-level CSS files; extend `nova-app.css`.
- Don't load anything from `design_handoff_onestream_nova_portal/` at runtime — it's a reference snapshot.
- Don't add comments inside code that just describe what the code does — the design system is self-evident from token names.
