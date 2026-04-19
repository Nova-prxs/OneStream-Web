# Handoff: OneStream Nova Portal

## Overview

A complete UI redesign of the OneStream study/reference portal under the **"Nova"** visual identity — a warm, editorial, brand-forward treatment that replaces the original utilitarian Bootstrap-style layout with a custom design language inspired by Nova's logo (coral + teal + aqua + bone) and Gilroy typography.

The portal has four product pillars:

1. **Exam Prep** — reference books, chapter reader, section-based quizzes, progress, flashcards, glossary.
2. **API Reference** — 5,323 classes / 288 namespaces with method signatures & examples.
3. **Business Rules** — 827 real rules (Extender, Finance, Connector, Dashboard, Assemblies).
4. **Extension Guide** — VS Code extension + MCP server docs for AI-assisted development.

Plus a Portal landing page that ties the four together, a persistent sidebar, and a dark-mode variant.

## About the Design Files

**The files in this bundle are design references created in HTML — prototypes showing intended look and behavior, not production code to copy directly.**

Your task is to **recreate these HTML designs in the target codebase's existing environment** (React + whatever build/router/styling stack is already in use), following its established patterns. Keep the **visual design language** (colors, typography, spacing, component shapes, page layouts) but adapt the **code** to fit the codebase's conventions (component structure, state management, routing, data fetching, etc.).

Everything in the prototype uses hard-coded placeholder data — wire it to the real data sources in the target codebase.

## Fidelity

**High-fidelity (hifi).** Pixel-level color, type, spacing, and interaction decisions are final. Recreate pixel-perfectly using the codebase's existing component library where possible; where the existing library falls short, build new primitives that match the spec.

---

## Files in this bundle

| File | Purpose |
|---|---|
| `OneStream Portal.html` | The working prototype — open in a browser to see everything live. Sidebar + all pages. |
| `tokens.css` | **Design tokens.** Single source of truth: colors, fonts, type scale, spacing, radii, shadows, dark-mode overrides. **Port this first.** |
| `app.css` | Component-level CSS: `.nv-sidebar`, `.nv-btn`, `.nv-crumb`, `.nv-page` fade-in, etc. |
| `shell.js` | App shell: sidebar, layout wrapper, breadcrumb, `Glyph`/`BrandGlyph` SVG components, page router switch. |
| `portal.js` | Portal landing page (3 layout variants A/B/C exposed via Tweaks — ship the default). |
| `exam.js` | Exam-prep pages: index, chapter reader, quiz sections, quiz, progress, flashcards, glossary. |
| `reference.js` | API Reference + Business Rules + Extension Guide pages. |
| `fonts/Gilroy-*.ttf` | Gilroy font files (Light/Regular/Bold). Licensed separately — confirm Nova/OneStream has a license before shipping. |
| `assets/nova-logo.png` | The Nova square logo (coral glyphs on aqua). |
| `assets/bg-coral.jpg`, `bg-teal.jpg`, `bg-dark.png` | Background textures used on hero cards. |

**How to read the prototype:**
`OneStream Portal.html` inlines `tokens.css` and imports the other files as `<script type="text/babel">`. Every component is defined globally on `window` (e.g. `window.PortalPage`, `window.ExamIndexPage`) — a compromise for the Babel-standalone setup. In the real codebase, convert to ES modules / proper imports.

---

## Design Tokens

**Port `tokens.css` verbatim into the target stack** (CSS custom properties, Tailwind theme extension, styled-components ThemeProvider — whatever the codebase uses).

### Colors — Brand Core

| Token | Hex | Usage |
|---|---|---|
| `--nova-coral` | `#FF5948` | Primary. CTAs, brand accents, coral cards. |
| `--nova-coral-ink` | `#E84636` | Pressed/focused coral. |
| `--nova-coral-soft` | `#FFE8E4` | Coral tints, soft surfaces. |
| `--nova-teal` | `#1A8A7F` | Secondary. Confident deep teal for accents + dark chrome. |
| `--nova-teal-ink` | `#147168` | Pressed teal. |
| `--nova-teal-soft` | `#D6EDEA` | Teal tints. |
| `--nova-aqua` | `#A7E8E4` | Supporting. Soft background for logo, callouts. |
| `--nova-aqua-soft` | `#E7F7F6` | Even softer aqua. |
| `--nova-ink` | `#2E3A40` | Primary text, dark surfaces. |
| `--nova-ink-2` | `#4B5761` | Secondary text. |
| `--nova-ink-3` | `#7A8690` | Tertiary / muted. |
| `--nova-bone` | `#F4F2EE` | Off-white page background. |
| `--nova-paper` | `#FFFFFF` | Cards, surfaces. |
| `--nova-line` | `#E4E4E4` | Dividers. |
| `--nova-line-soft` | `#EFEDE8` | Very soft dividers, table header rows. |

### Semantic aliases

`--color-bg`, `--color-surface`, `--color-text`, `--color-text-2`, `--color-text-3`, `--color-primary`, `--color-accent`, `--color-line` — all map to nova-* tokens. **Components should reference semantic names**, not brand names, so dark mode (below) can swap them.

### Dark mode

Implemented via `html[data-theme="dark"]` (see `tokens.css`). Key overrides:
- `--color-bg` → `#1C2428` (deep ink)
- `--color-surface` → `#232C31`
- `--color-text` → `#F5E7D8` (cream, explicit — NOT `var(--nova-paper)` since that gets remapped)
- Sidebar uses `--nova-teal` background with cream text.

### Typography

- **Font family:** `Gilroy` (Light 300, Regular 400, Bold 700), fallback to system sans.
- **Monospace:** `ui-monospace, SFMono-Regular, Menlo, Consolas`.
- **Scale:**
  - `--fs-display: 88px` (hero)
  - `--fs-h1: 56px`
  - `--fs-h2: 40px`
  - `--fs-h3: 28px`
  - `--fs-h4: 20px`
  - `--fs-body-l: 18px`
  - `--fs-body: 16px`
  - `--fs-small: 13px`
  - `--fs-micro: 11px` (eyebrows, labels — uppercase, letter-spacing 0.14em)
- **Leading:** tight for display (1.02), normal for body (1.55).
- **Letter-spacing:** display -0.03em, h1 -0.03em, body 0, eyebrow 0.14em uppercase.

### Spacing / radii / motion

- Spacing scale: 4, 8, 12, 16, 20, 24, 32, 40, 48, 64, 96.
- Radii: 8 (inputs), 14 (cards), 20 (large cards), 40 (hero cards), 999 (pills).
- Motion: `cubic-bezier(0.2, 0.9, 0.2, 1)` for most transitions; 180ms for hover, 300ms for page fade-in.

---

## Screens / Views

### 1. App Shell (sidebar + content area)

**Layout:** Fixed 240px sidebar on the left (collapsible to 64px), content takes remaining width with a max-width of ~1200px centered within it.

**Sidebar:**
- Background: `var(--nova-paper)` (light) / `var(--nova-teal)` (dark).
- Top: Nova logo (square, 40×40, coral glyphs on aqua — use `assets/nova-logo.png`) + "OneStream" wordmark in Gilroy Bold 18px next to it.
- Nav items: Portal, Study Home, API Reference, Business Rules, Extension. Each with a small SVG glyph (see `Glyph` component in `shell.js`) + label. Active item has coral accent bar on the left (3px wide, full height) and slightly bolder weight.
- Bottom: theme toggle (sun/moon icon), settings.
- Hover state: background shifts to `--nova-line-soft`, text to `--color-text`.

**Main content area:**
- Padding: 48px top, 64px sides on desktop; 32px on tablet.
- Breadcrumb at top (`.nv-crumb`) — inline, 13px, with `/` separators, last item bold.
- Page fade-in: opacity 0 → 1 + 8px translateY over 300ms on mount.

### 2. Portal Landing (`PortalPage`)

The hub. Three layout variants (A/B/C) are exposed via Tweaks in the prototype — **ship Variant A as default** (the 4-card grid with brand glyphs).

- Hero: eyebrow "ONESTREAM DEVELOPER PORTAL" (micro, 11px, letter-spacing 0.14em, uppercase) → headline `fs-display` 88px, Gilroy Bold, `letter-spacing: -0.03em` → lede paragraph 18px, `--color-text-2`, max-width 680px.
- Primary grid: 4 cards in a 2×2 grid on desktop (1 column on mobile), gap 24px. Each card:
  - Full-height, border-radius 40px, padding 40px, cursor pointer.
  - Background: `--nova-coral` / `--nova-teal` / `--nova-ink` / `--nova-aqua` (one per pillar).
  - Large BrandGlyph (arch / circle / triangle / teardrop) at 180px, positioned absolute bottom-right at `-20px` offset with 16% opacity tint of card text color.
  - Eyebrow label ("REFERENCE" / "API" / "PRACTICE" / "TOOLING") at top.
  - Headline 36px, Gilroy Bold, `letter-spacing: -0.02em`.
  - Descriptor 14px.
  - CTA line at bottom: "Enter the reader →" etc., 14px, 600 weight.
- Stats strip below grid: 4 columns, showing `4 books`, `5,323 classes`, `827 rules`, `618 quiz questions`. Numbers in Gilroy Bold 48px, labels in `--fs-small` uppercase.

### 3. Exam Prep — Index (`ExamIndexPage`)

- Breadcrumb: Portal / Exam Prep
- Split hero: left column text (H1 56px "Deep reference. Practice with bite."), right column the Nova arch brand glyph.
- "Reference Books" section: 4 book cards in a grid.
  - Each card: title, chapters count, word count, short description, "Open" button.
  - Card BG `--color-surface`, border `1px solid --color-line`, radius 20, padding 28. Hover: translateY(-2px), shadow.
- "Practice" CTA card: coral BG, "618 weighted questions. 8 exam sections. ..." with "Start drills →" button.

### 4. Chapter Reader (`ChapterPage`)

- Breadcrumb: Portal / Exam Prep / [Book] / [Chapter]
- Two-column layout: left (700px max) = prose, right (sticky, 260px) = chapter TOC.
- Prose uses `--fs-body-l` 18px, line-height 1.7, `--color-text`. Headings scale h2 28px, h3 20px.
- "💡 EXAM TIP" callout: aqua BG, teal left border 4px, rounded 14, padding 20/24 — for actionable recall hints.

### 5. Exam Selector (`ExamSelectorPage`) — entry to Practice

**Route:** `/exam-prep/`. This is the landing for the Practice pillar — lists ALL available certification exams as cards. User picks one → lands on that exam's section list.

- Hero: pill badge "{N} Certification Exams" + H1 "Choose your **Certification**" (headline-colored accent on the noun) + subhead.
- 2-column grid of exam cards. Each card:
  - Icon (top-left) + Level badge (top-right) — Associate uses teal tint (`--nova-teal-soft` bg, `--nova-teal` text); Specialist uses coral tint (`--nova-coral-soft` bg, `--nova-coral` text).
  - Exam name (H3, Gilroy Bold) + short title underneath.
  - 2 stat tiles at bottom: `Sections` count and `Questions` count.
  - Hover-only "Start Studying →" affordance at the bottom.
- Bottom: single wide "Study Books & Reference Guides" card linking to the reference library.

### 6. Quiz Sections (`QuizSectionsPage`)

**Route:** `/exam-prep/{exam_id}/quiz`. The selected exam's section list. **This is per-exam, not a single flat quiz** — every exam has its own blueprint.

- H1: `{exam.name} Exam Quiz` + lede describing `{total_questions}` across `{sections.length}` sections, weighted per blueprint.
- Table with columns: `#`, `Section`, `Exam Weight` (%), `Questions`, `Progress` (answered/total), `Score` (% correct, color-coded — green ≥80, amber ≥50, red <50).
- Footer row: totals for all columns.
- Score cells use `.score-good`, `.score-ok`, `.score-bad` classes — map to `--nova-teal`, `#D18A2E` (amber), `--nova-coral`.
- Progress reads from `localStorage['quiz_progress_{exam_id}_{section_slug}']`.
- Below table: secondary action row — ghost buttons: `Exam Simulation`, `Review Failed`, `Study by Objectives`, `Section Summaries`, `Glossary`.

### 7. Quiz (`QuizPage`)

**Route:** `/exam-prep/{exam_id}/quiz/{section_slug}`.

- Breadcrumb: Home › Quiz › {Section Name}
- H1: `{section.name}` with weight in small: `({weight}% of exam)`.
- **Progress bar** (full-width, 6px): green fill showing % correct out of total section questions.
- **Filter chips row:**
  - Difficulty group: `All` / `Easy` / `Medium` / `Hard`
  - Status group: `All` / `Unanswered` / `Correct` / `Incorrect`
  - Chip styling: pill, `--color-line` border, 13px, padding 6/14. Active chip = coral BG, white text.
- **Navigation bar** (between filters and question):
  - Left: `Question {n} of {total}` counter.
  - Right cluster: `← Prev` button, `<select>` jumper (Q1…QN — colored by state: teal = correct, coral = incorrect, muted = unanswered), `Next →`, `Skip to unanswered` (ghost).
  - `<kbd>` hints next to prev/next (←/→).
- **Question card** (`<article>`): meta line (`Objective: X | Difficulty: Y`) 13px muted → question text 22px/1.4 → options.
- **Options** (4, rendered as buttons): full-width, radius 14, padding 18/22, `--color-surface` BG, `--color-line` border. Letter prefix bold (`A)` / `B)` / `C)` / `D)`). `<kbd>` hint at right edge. Selected+correct → `.correct` (teal border 2px, `--nova-teal-soft` BG). Selected+wrong → `.incorrect` (coral border 2px, `--nova-coral-soft` BG). After answering, all buttons disabled.
- **Feedback block** (revealed after answer, 180ms fade+slide):
  - Result line: `✓ Correct!` (teal) or `✗ Incorrect — correct answer: X)` (coral).
  - If wrong: "You selected: X) …" (muted) + "Correct: Y) …" (teal) stacked.
  - Explanation paragraph.
  - "Read more: {chapter title}" link to the related chapter, with book title in small.
- **Section summary** footer: `Answered: X / Y | Correct: Z (W%)`.
- **Keyboard shortcuts** (MUST implement, ignoring input/select/textarea):
  - `←` / `→` — prev / next
  - `A`/`1`, `B`/`2`, `C`/`3`, `D`/`4` — pick that option
  - `n` — skip to next unanswered (wraps)
- **Persistence:** `localStorage['quiz_progress_{exam_id}_{section_slug}']` shape: `{answered: {[questionIndex]: letter}}`.

### 8. Progress Dashboard (`ProgressPage`)

**Route:** `/exam-prep/{exam_id}/progress`. Per-exam dashboard — one per certification.

- H1: `{exam.name} Progress Dashboard`.
- **Overall summary card** (`.progress-summary`):
  - `{answered}` / `{total}` questions answered — `{pct}%` correct.
  - `<progress>` bar (completion %, full width).
  - **Pass estimate pill:** at least 20 questions answered → shows weighted score (weighted by each section's exam weight): `Estimated exam score: X% — PASS` (teal) or `X% — needs improvement (70% to pass)` (coral). Under 20 answered → muted "Answer at least 20 questions…".
- **Weak sections banner** (`.weak-sections`, shown only if any section <70%): `Focus areas: {name} ({pct}%), {name} ({pct}%), …`.
- **Per-section breakdown table** — columns: `Section` (link) / `Weight` / `Progress` (stacked bar) / `Answered` / `Correct` / `Score` / `Actions` (Reset button, ghost-secondary).
  - Progress bar cell uses a two-segment stacked bar: green (correct) + coral (incorrect) as % of total questions in that section. Unanswered = empty.
  - Score cell color-coded same as quiz_sections.
- **Exam Simulation History** (hidden until the user has taken ≥1 exam sim). Reads `localStorage['exam_history_{exam_id}']` — array of `{date, score, passed, timeElapsed}`.
  - Bar chart of last 20 attempts (bars colored teal/coral by pass/fail, height = score%). Horizontal dotted "70% pass" reference line.
  - Below chart: list table (date, score, PASS/FAIL, time elapsed) — most recent first.
  - `Clear History` ghost button.
- **Quick action row:** `Review Failed Questions`, `Study by Objectives`, `Section Summaries`, `Glossary`.
- **Export / Import / Reset All row** at bottom:
  - `Reset All Progress` (contrast/danger) — confirms.
  - `Export Progress` → downloads `onestream-progress-YYYY-MM-DD.json` (per-section answers + all `notes_*` keys).
  - `Import Progress` → file picker, merges back into localStorage.

### 9. Flashcards (`FlashcardsPage`)

Centered card, click to flip. Previous/Next arrows, progress counter.

### 10. Glossary (`GlossaryPage`)

Alphabetical term list, 2-column on desktop, each term: bold title + definition.

### 11. API Reference, Business Rules, Extension Guide

See `reference.js` — similar layout patterns: hero + filterable list + detail pane. Same token usage.

---

## Components (reusable primitives)

### `Glyph` (in `shell.js`)
Inline SVG icons (nav icons, arrows, chevrons). Props: `kind`, `size`, `color`. 1.5px stroke, `stroke-linecap: round`.

### `BrandGlyph` (in `shell.js`)
Large decorative shapes derived from Nova logo: `arch`, `circle`, `triangle`, `teardrop`. Props: `kind`, `size`, `color`, `style`.

### `.nv-btn`
- `.nv-btn--primary`: coral BG, white text, radius 999, height 44, padding 0 24, Gilroy Bold 14px.
- `.nv-btn--ghost`: transparent BG, `--color-line` border, `--color-text` text.
- Hover: slight y-translate + shadow on primary; BG fill on ghost.

### `.nv-card`
`--color-surface` BG, `1px solid --color-line`, radius 20, padding 28. Hover shadow: `0 8px 24px rgba(46, 58, 64, 0.08)`.

### `Crumb` (breadcrumb)
Takes `items: [{ label, onClick? }]`. Separator `/`, 13px, `--color-text-3`.

### `.nv-eyebrow`
`font-size: 11px, letter-spacing: 0.14em, text-transform: uppercase, font-weight: 700, color: var(--color-text-3)`.

---

## Interactions & Behavior

- **Page navigation:** currently a JS switch on `page` state in `shell.js`. Replace with real router (React Router / Next.js router / whatever the codebase uses).
- **Page transitions:** 300ms fade + 8px translateY on enter.
- **Theme toggle:** stored at `html[data-theme]`, persisted to `localStorage["nv-theme"]`.
- **Sidebar collapse:** `localStorage["nv-sidebar-collapsed"]`.
- **Quiz state:** tracks `answered`, `correct` per section — persist to backend (in prototype it's hardcoded).
- **Chapter scroll progress:** right-sidebar TOC should highlight active heading via IntersectionObserver.

## State Management

Minimal in the prototype (`useState` at app root). In production:
- Quiz answers → backend persistence (per-user progress).
- Read chapters → user progress tracking.
- Flashcard review state → spaced-repetition scheduling.
- Theme + sidebar state → localStorage is fine.

## Assets

- **Nova logo** (`assets/nova-logo.png`): Provided by user. Square, coral glyphs on aqua background. This is the official brand mark — don't redraw it.
- **Background textures** (`bg-coral.jpg`, `bg-teal.jpg`, `bg-dark.png`): subtle paper/grain textures used on hero cards. Overlay at low opacity.
- **Gilroy fonts:** licensed separately — confirm OneStream has a valid license before shipping.

## Route map (for developer reference)

Keep the URL structure the target codebase already uses. For reference, the original Flask routes the prototype mirrors:

| Route | Page component |
|---|---|
| `/` | `PortalPage` (landing) |
| `/exam-prep/` | `ExamSelectorPage` (list of all exams) |
| `/exam-prep/{exam_id}/quiz` | `QuizSectionsPage` |
| `/exam-prep/{exam_id}/quiz/{section_slug}` | `QuizPage` |
| `/exam-prep/{exam_id}/exam` | `ExamSimulationPage` (not yet in this bundle) |
| `/exam-prep/{exam_id}/review` | `ReviewPage` (not yet in this bundle) |
| `/exam-prep/{exam_id}/objectives` | `ObjectivesPage` |
| `/exam-prep/{exam_id}/summary` | `SummaryPage` |
| `/exam-prep/{exam_id}/glossary` | `GlossaryPage` |
| `/exam-prep/{exam_id}/flashcards` | `FlashcardsPage` |
| `/exam-prep/{exam_id}/progress` | `ProgressPage` |
| `/exam-prep/guide/{book_slug}` | `BookPage` |
| `/exam-prep/guide/{book_slug}/{chapter_slug}` | `ChapterPage` |
| `/docs/` | `APIIndexPage` |
| `/docs/rules/` | `RulesIndexPage` |
| `/docs/extension/` | `ExtensionPage` |

## Open questions for the developer

1. **Router choice:** does the target codebase use React Router, Next.js App Router, TanStack Router, or something else? Adapt the navigation accordingly.
2. **Icon library:** replace the inline `Glyph` SVGs with the codebase's existing icon library if one exists (Lucide, Phosphor, etc.) — or keep `Glyph` as a custom set if the codebase doesn't have one.
3. **Gilroy licensing:** confirm before shipping.
4. **Exam sim & Review pages:** the original repo has `exam.html` and `review.html` — not covered in this bundle. Apply the same token system (nav, typography, card shell) when implementing.
