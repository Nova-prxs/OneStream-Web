// Exam prep pages: index/home, chapter reader, quiz sections, quiz, progress, flashcards, glossary
const books = [
  { slug: "design-reference-guide", title: "Design Reference Guide", chapters: 24, words: 312000, desc: "Comprehensive reference covering the full OneStream platform: dimensions, cubes, workflow, dashboards, data integration." },
  { slug: "finance-rules", title: "Finance Rules Handbook", chapters: 10, words: 155000, desc: "Financial logic engine, calculations, translations, and consolidation rules." },
  { slug: "foundation-handbook", title: "Foundation Handbook", chapters: 14, words: 185000, desc: "Core platform concepts: architecture, security, metadata, business rules, and administration." },
  { slug: "workspaces-assemblies", title: "Workspaces & Assemblies", chapters: 12, words: 75000, desc: "Workspaces, assemblies, solution management, and deployment patterns." },
];

function ExamIndexPage({ setPage }) {
  return (
    <div className="nv-page fade-in">
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Exam Prep" }]} />

      <div style={{ display: "grid", gridTemplateColumns: "1.4fr 1fr", gap: 40, marginBottom: 48, alignItems: "center" }}>
        <div>
          <p className="nv-eyebrow" style={{ marginBottom: 16 }}>OS-201 · YOUR TRACK</p>
          <h1 style={{ fontSize: 64, lineHeight: 1.02, letterSpacing: "-0.03em", margin: "0 0 16px" }}>
            Welcome back,<br/>Maria.
          </h1>
          <p style={{ fontSize: 18, color: "var(--color-text-2)", maxWidth: 520 }}>
            You're 42% through the syllabus. Pick up where you left off, or jump into a drill.
          </p>
        </div>
        <div style={{ background: "var(--nova-ink)", color: "#fff", borderRadius: 24, padding: 32, position: "relative", overflow: "hidden" }}>
          <BrandGlyph kind="arch" size={180} color="rgba(255,89,72,0.22)" style={{ position: "absolute", right: -20, top: -20 }} />
          <p className="nv-eyebrow" style={{ color: "var(--nova-aqua)" }}>CONTINUE READING</p>
          <h3 style={{ fontSize: 22, fontWeight: 700, margin: "12px 0 8px" }}>Chapter 08 — Cubes</h3>
          <p style={{ fontSize: 13, opacity: 0.7, marginBottom: 20 }}>Design Reference Guide · 48 min remaining</p>
          <div style={{ height: 6, background: "rgba(255,255,255,0.1)", borderRadius: 999, overflow: "hidden", marginBottom: 16 }}>
            <div style={{ width: "62%", height: "100%", background: "var(--nova-coral)" }}/>
          </div>
          <button className="nv-btn nv-btn--primary" onClick={() => setPage("chapter")} style={{ height: 38 }}>Resume →</button>
        </div>
      </div>

      <section style={{ display: "grid", gridTemplateColumns: "repeat(5, 1fr)", gap: 16, marginBottom: 48 }}>
        {[
          { n: "4", l: "Books", c: "var(--nova-coral)" },
          { n: "60", l: "Chapters", c: "var(--nova-teal)" },
          { n: "618", l: "Questions" },
          { n: "247", l: "Answered" },
          { n: "81%", l: "Avg Score", c: "var(--nova-teal)" },
        ].map((s, i) => (
          <div key={i} className="nv-card" style={{ padding: 24 }}>
            <div style={{ fontSize: 40, fontWeight: 700, letterSpacing: "-0.02em", color: s.c || "var(--color-text)" }}>{s.n}</div>
            <div style={{ fontSize: 11, color: "var(--color-text-3)", textTransform: "uppercase", letterSpacing: "0.14em", fontWeight: 700, marginTop: 4 }}>{s.l}</div>
          </div>
        ))}
      </section>

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: 20 }}>
        <h2 style={{ fontSize: 32, fontWeight: 700, letterSpacing: "-0.02em", margin: 0 }}>Reference Books</h2>
        <button className="nv-btn nv-btn--ghost" onClick={() => setPage("quiz-sections")} style={{ height: 40, padding: "0 16px", fontSize: 13 }}>Jump to quiz</button>
      </div>

      <section style={{ display: "grid", gridTemplateColumns: "repeat(2, 1fr)", gap: 20 }}>
        {books.map((b, i) => {
          const glyphs = ["arch", "circle", "triangle", "teardrop"];
          const colors = ["var(--nova-coral)", "var(--nova-teal)", "var(--nova-ink)", "var(--nova-coral)"];
          return (
            <div key={b.slug} className="nv-card" style={{ padding: 32, cursor: "pointer" }} onClick={() => setPage("chapter")}>
              <div style={{ display: "flex", gap: 20 }}>
                <BrandGlyph kind={glyphs[i]} size={56} color={colors[i]} />
                <div style={{ flex: 1 }}>
                  <p className="nv-eyebrow" style={{ marginBottom: 4 }}>BOOK {String(i+1).padStart(2, "0")}</p>
                  <h3 style={{ fontSize: 22, fontWeight: 700, margin: "0 0 8px", letterSpacing: "-0.01em" }}>{b.title}</h3>
                  <p style={{ fontSize: 14, color: "var(--color-text-2)", lineHeight: 1.55, margin: "0 0 16px" }}>{b.desc}</p>
                  <div style={{ display: "flex", gap: 16, fontSize: 12, color: "var(--color-text-3)" }}>
                    <span><strong style={{ color: "var(--color-text)" }}>{b.chapters}</strong> chapters</span>
                    <span><strong style={{ color: "var(--color-text)" }}>{(b.words/1000).toFixed(0)}k</strong> words</span>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </section>
    </div>
  );
}

// ── Chapter reader
function ChapterPage({ setPage }) {
  const [noteOpen, setNoteOpen] = React.useState(false);
  const [notes, setNotes] = React.useState("Cubes are the core dimensional model — when defining a cube, the Time dimension is always required.\n\nKey point: only ONE of each dimension type per cube.");

  return (
    <div className="nv-page fade-in" style={{ maxWidth: 1200 }}>
      <Crumb items={[
        { label: "Portal", onClick: () => setPage("portal") },
        { label: "Design Reference", onClick: () => setPage("index") },
        { label: "Chapter 08 — Cubes" }
      ]} />

      <div style={{ display: "grid", gridTemplateColumns: "240px 1fr 280px", gap: 40 }}>
        {/* TOC */}
        <aside style={{ position: "sticky", top: 80, alignSelf: "start", height: "fit-content" }}>
          <p className="nv-eyebrow" style={{ marginBottom: 12 }}>IN THIS CHAPTER</p>
          <nav style={{ borderLeft: "2px solid var(--nova-line)", paddingLeft: 16 }}>
            {[
              { t: "Introduction", active: false },
              { t: "Dimensionality", active: true },
              { t: "Cube types", active: false },
              { t: "Time dimension", active: false },
              { t: "Scenario setup", active: false },
              { t: "Consolidation rules", active: false },
              { t: "Custom dimensions", active: false },
            ].map((it, i) => (
              <a key={i} style={{
                display: "block", padding: "6px 0",
                fontSize: 13,
                color: it.active ? "var(--nova-coral)" : "var(--color-text-2)",
                fontWeight: it.active ? 700 : 500,
                marginLeft: it.active ? -18 : 0,
                borderLeft: it.active ? "2px solid var(--nova-coral)" : "none",
                paddingLeft: it.active ? 16 : 0,
                cursor: "pointer"
              }}>{it.t}</a>
            ))}
          </nav>
        </aside>

        {/* Main content */}
        <article style={{ fontSize: 17, lineHeight: 1.7, color: "var(--color-text)" }}>
          <p className="nv-eyebrow" style={{ marginBottom: 12 }}>CHAPTER 08 · DESIGN REFERENCE GUIDE</p>
          <h1 style={{ fontSize: 56, lineHeight: 1.05, letterSpacing: "-0.03em", margin: "0 0 32px" }}>Cubes</h1>
          <p style={{ fontSize: 21, lineHeight: 1.5, color: "var(--color-text-2)", marginBottom: 32 }}>
            Cubes are the multidimensional containers that store financial data in OneStream. Each cube combines required and optional dimensions into a logical model your organization reports against.
          </p>

          <h2 style={{ fontSize: 32, fontWeight: 700, letterSpacing: "-0.02em", marginTop: 48, marginBottom: 16 }}>Dimensionality</h2>
          <p>A cube always contains the <strong>eight required dimensions</strong>: Scenario, Time, Entity, Consolidation, Parent Currency, Cube, View, and Account. These form the backbone of every financial record stored in the platform — you cannot remove them, but you can extend them.</p>
          <p>Beyond the required set, cubes support up to <strong>four User-Defined (UD)</strong> dimensions and up to <strong>four Flow</strong> dimensions, which give you space to model the specifics of your business.</p>

          <div style={{ background: "var(--nova-aqua)", borderLeft: "4px solid var(--nova-teal)", padding: "20px 24px", borderRadius: 14, margin: "32px 0", color: "var(--nova-ink)" }}>
            <p className="nv-eyebrow" style={{ color: "var(--nova-teal-ink)", marginBottom: 8 }}>💡 EXAM TIP</p>
            <p style={{ margin: 0, fontSize: 16 }}>Only ONE of each required dimension type per cube. When the exam asks "how many Time dimensions can a cube have?" — the answer is always one.</p>
          </div>

          <h2 style={{ fontSize: 32, fontWeight: 700, letterSpacing: "-0.02em", marginTop: 48, marginBottom: 16 }}>Cube types</h2>
          <p>OneStream provides three cube types: <strong>Standard</strong> cubes for financial consolidation, <strong>Planning</strong> cubes for budgeting and forecasting, and <strong>BI Viewer</strong> cubes for analytics blends. Each has subtle differences in how it caches and aggregates data.</p>

          {/* Mini-quiz */}
          <section style={{ marginTop: 64, padding: "40px 0", borderTop: "2px solid var(--nova-line)" }}>
            <p className="nv-eyebrow" style={{ marginBottom: 8 }}>TEST YOUR UNDERSTANDING</p>
            <h2 style={{ fontSize: 28, fontWeight: 700, letterSpacing: "-0.01em", margin: "0 0 24px" }}>3 questions for this chapter</h2>

            <div className="nv-card" style={{ padding: 28 }}>
              <div style={{ display: "flex", gap: 8, marginBottom: 12, alignItems: "center" }}>
                <span style={{ fontSize: 11, fontWeight: 700, color: "var(--nova-teal)", background: "var(--nova-teal-soft)", padding: "3px 10px", borderRadius: 999 }}>MEDIUM</span>
                <span style={{ fontSize: 11, color: "var(--color-text-3)" }}>Question 1 of 3</span>
              </div>
              <p style={{ fontSize: 17, fontWeight: 600, margin: "0 0 20px" }}>How many Time dimensions can a single cube contain?</p>
              <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                {[
                  { l: "A", t: "Zero — Time is scenario-specific", s: "default" },
                  { l: "B", t: "Exactly one", s: "correct" },
                  { l: "C", t: "Up to four (matching UD dimensions)", s: "wrong" },
                  { l: "D", t: "Unlimited", s: "default" },
                ].map(o => (
                  <button key={o.l} style={{
                    textAlign: "left", padding: "14px 18px",
                    border: `1.5px solid ${o.s === "correct" ? "var(--nova-teal)" : o.s === "wrong" ? "var(--nova-coral)" : "var(--nova-line)"}`,
                    background: o.s === "correct" ? "var(--nova-teal-soft)" : o.s === "wrong" ? "var(--nova-coral-soft)" : "var(--nova-paper)",
                    borderRadius: 12, fontSize: 14, fontWeight: 500,
                    display: "flex", gap: 12
                  }}>
                    <strong style={{ color: o.s === "correct" ? "var(--nova-teal-ink)" : o.s === "wrong" ? "var(--nova-coral-ink)" : "var(--color-text-3)" }}>{o.l})</strong>
                    {o.t}
                    {o.s === "correct" && <span style={{ marginLeft: "auto", color: "var(--nova-teal)" }}>✓</span>}
                    {o.s === "wrong" && <span style={{ marginLeft: "auto", color: "var(--nova-coral)" }}>✗</span>}
                  </button>
                ))}
              </div>
              <div style={{ marginTop: 20, padding: 16, background: "var(--nova-line-soft)", borderRadius: 10, fontSize: 14, lineHeight: 1.5 }}>
                <strong>Correct — B.</strong> Every cube has exactly one Time dimension, which is fundamental to how OneStream periodizes and aggregates data. The Time dimension is mandatory and singular.
              </div>
            </div>
          </section>

          {/* Prev/next */}
          <nav style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16, marginTop: 48, paddingTop: 32, borderTop: "1px solid var(--nova-line)" }}>
            <a className="nv-card" style={{ padding: 20, cursor: "pointer" }}>
              <div style={{ fontSize: 11, color: "var(--color-text-3)", textTransform: "uppercase", letterSpacing: "0.14em", fontWeight: 700 }}>← Previous</div>
              <div style={{ fontSize: 16, fontWeight: 600, marginTop: 6 }}>Chapter 07 — Implementing Security</div>
            </a>
            <a className="nv-card" style={{ padding: 20, cursor: "pointer", textAlign: "right" }}>
              <div style={{ fontSize: 11, color: "var(--color-text-3)", textTransform: "uppercase", letterSpacing: "0.14em", fontWeight: 700 }}>Next →</div>
              <div style={{ fontSize: 16, fontWeight: 600, marginTop: 6 }}>Chapter 09 — Workflow</div>
            </a>
          </nav>
        </article>

        {/* Right rail: Notes */}
        <aside style={{ position: "sticky", top: 80, alignSelf: "start", height: "fit-content" }}>
          <div className="nv-card" style={{ padding: 20 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
              <p className="nv-eyebrow" style={{ margin: 0 }}>MY NOTES</p>
              <span style={{ fontSize: 10, color: "var(--nova-teal)", fontWeight: 600 }}>● Saved</span>
            </div>
            <textarea value={notes} onChange={e => setNotes(e.target.value)}
              style={{
                width: "100%", minHeight: 180,
                border: "1px solid var(--nova-line)", borderRadius: 10,
                padding: 12, fontSize: 13, lineHeight: 1.5,
                background: "var(--nova-line-soft)",
                resize: "vertical", outline: "none"
              }}/>
            <button className="nv-btn nv-btn--ghost" style={{ width: "100%", justifyContent: "center", fontSize: 12, height: 36, marginTop: 12 }}>
              Export to Markdown
            </button>
          </div>
          <div style={{ fontSize: 11, color: "var(--color-text-3)", padding: "16px 4px", lineHeight: 1.6 }}>
            <strong style={{ color: "var(--color-text-2)" }}>💡 Tip:</strong> Highlight text with your cursor — it will be saved automatically to your highlights panel.
          </div>
        </aside>
      </div>
    </div>
  );
}

// ── Quiz Sections
function QuizSectionsPage({ setPage }) {
  const sections = [
    { n: 1, slug: "cube", name: "Cube Design", weight: 18, total: 95, answered: 63, correct: 54 },
    { n: 2, slug: "workflow", name: "Workflow", weight: 14, total: 78, answered: 40, correct: 32 },
    { n: 3, slug: "data", name: "Data Collection", weight: 12, total: 68, answered: 28, correct: 22 },
    { n: 4, slug: "presentation", name: "Presentation", weight: 15, total: 88, answered: 50, correct: 41 },
    { n: 5, slug: "tools", name: "Tools & Admin", weight: 10, total: 62, answered: 18, correct: 14 },
    { n: 6, slug: "security", name: "Security", weight: 11, total: 68, answered: 30, correct: 28 },
    { n: 7, slug: "admin", name: "Administration", weight: 8, total: 52, answered: 10, correct: 8 },
    { n: 8, slug: "rules", name: "Business Rules", weight: 12, total: 107, answered: 8, correct: 6 },
  ];
  return (
    <div className="nv-page fade-in">
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Exam Prep", onClick: () => setPage("index") }, { label: "Quiz" }]} />

      <div style={{ display: "grid", gridTemplateColumns: "1fr auto", gap: 40, alignItems: "end", marginBottom: 48 }}>
        <div>
          <p className="nv-eyebrow" style={{ marginBottom: 12 }}>618 WEIGHTED QUESTIONS · 8 SECTIONS</p>
          <h1 style={{ fontSize: 56, lineHeight: 1.02, letterSpacing: "-0.03em", margin: 0 }}>Practice, by section.</h1>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="nv-btn nv-btn--ghost" style={{ height: 44, padding: "0 18px" }}>Review failed</button>
          <button className="nv-btn nv-btn--primary" onClick={() => setPage("quiz")}>Start exam simulation →</button>
        </div>
      </div>

      <div className="nv-card" style={{ padding: 0, overflow: "hidden" }}>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr style={{ background: "var(--nova-line-soft)" }}>
              {["#", "Section", "Exam weight", "Progress", "Score", ""].map((h, i) => (
                <th key={i} style={{ textAlign: i === 5 ? "right" : "left", padding: "16px 20px", fontSize: 11, textTransform: "uppercase", letterSpacing: "0.14em", fontWeight: 700, color: "var(--color-text-3)" }}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {sections.map((s, i) => {
              const pct = s.answered > 0 ? Math.round(s.correct/s.answered*100) : 0;
              const progPct = Math.round(s.answered/s.total*100);
              return (
                <tr key={s.slug} style={{ borderTop: "1px solid var(--nova-line)", cursor: "pointer" }}
                    onClick={() => setPage("quiz")}
                    onMouseEnter={e => e.currentTarget.style.background = "var(--nova-line-soft)"}
                    onMouseLeave={e => e.currentTarget.style.background = "transparent"}>
                  <td style={{ padding: "20px", fontSize: 13, color: "var(--color-text-3)", fontFamily: "var(--font-mono)" }}>{String(s.n).padStart(2,"0")}</td>
                  <td style={{ padding: "20px" }}>
                    <div style={{ fontSize: 16, fontWeight: 600 }}>{s.name}</div>
                    <div style={{ fontSize: 12, color: "var(--color-text-3)", marginTop: 2 }}>{s.total} questions</div>
                  </td>
                  <td style={{ padding: "20px", fontSize: 14 }}>
                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                      <div style={{ width: 60, height: 4, background: "var(--nova-line)", borderRadius: 999, overflow: "hidden" }}>
                        <div style={{ width: `${s.weight*5}%`, height: "100%", background: "var(--nova-ink)" }}/>
                      </div>
                      <span style={{ fontWeight: 600 }}>{s.weight}%</span>
                    </div>
                  </td>
                  <td style={{ padding: "20px", fontSize: 14 }}>
                    <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                      <div style={{ width: 100, height: 6, background: "var(--nova-line)", borderRadius: 999, overflow: "hidden" }}>
                        <div style={{ width: `${progPct}%`, height: "100%", background: "var(--nova-coral)" }}/>
                      </div>
                      <span style={{ fontVariantNumeric: "tabular-nums", fontWeight: 600 }}>{s.answered}/{s.total}</span>
                    </div>
                  </td>
                  <td style={{ padding: "20px" }}>
                    {s.answered > 0 ? (
                      <span style={{
                        fontSize: 14, fontWeight: 700,
                        color: pct >= 80 ? "var(--nova-teal)" : pct >= 50 ? "var(--color-text)" : "var(--nova-coral)"
                      }}>{pct}%</span>
                    ) : <span style={{ color: "var(--color-text-3)" }}>—</span>}
                  </td>
                  <td style={{ padding: "20px", textAlign: "right" }}>
                    <Glyph kind="arrow" size={16} color="var(--color-text-3)" />
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ── Quiz (single question)
function QuizPage({ setPage }) {
  const [selected, setSelected] = React.useState(null);
  const [answered, setAnswered] = React.useState(false);
  const correct = "B";

  const opts = [
    { l: "A", t: "Use XFSetCellAmount to directly write to the target cell without any buffering" },
    { l: "B", t: "Use api.Data.Calculate() and route the result through the Finance Engine's Data Buffer" },
    { l: "C", t: "Manually query SQL through XFSqlHelper and insert directly into tblDataRecord" },
    { l: "D", t: "Use a Dashboard Extender rule with api.Pov.GetPovForFormulas()" },
  ];

  return (
    <div className="nv-page fade-in" style={{ maxWidth: 960 }}>
      <Crumb items={[
        { label: "Portal", onClick: () => setPage("portal") },
        { label: "Exam Prep", onClick: () => setPage("index") },
        { label: "Quiz", onClick: () => setPage("quiz-sections") },
        { label: "Business Rules" }
      ]} />

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 20 }}>
        <div>
          <h1 style={{ fontSize: 32, fontWeight: 700, letterSpacing: "-0.01em", margin: 0 }}>Business Rules <span style={{ color: "var(--color-text-3)", fontWeight: 500, fontSize: 18 }}>12% of exam</span></h1>
        </div>
        <div style={{ fontSize: 14, color: "var(--color-text-2)" }}>Question <strong>34</strong> of <strong>107</strong></div>
      </div>

      {/* Progress bar */}
      <div style={{ height: 6, background: "var(--nova-line)", borderRadius: 999, overflow: "hidden", marginBottom: 24, display: "flex" }}>
        <div style={{ width: "28%", background: "var(--nova-teal)" }}/>
        <div style={{ width: "4%", background: "var(--nova-coral)" }}/>
      </div>

      {/* Filter chips */}
      <div style={{ display: "flex", gap: 8, marginBottom: 24, alignItems: "center", flexWrap: "wrap" }}>
        <span style={{ fontSize: 12, color: "var(--color-text-3)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.08em", marginRight: 4 }}>Difficulty:</span>
        {["All", "Easy", "Medium", "Hard"].map((f, i) => (
          <button key={f} style={{
            padding: "6px 14px", borderRadius: 999,
            border: `1px solid ${i === 0 ? "var(--nova-ink)" : "var(--nova-line)"}`,
            background: i === 0 ? "var(--nova-ink)" : "transparent",
            color: i === 0 ? "#fff" : "var(--color-text-2)",
            fontSize: 12, fontWeight: 600
          }}>{f}</button>
        ))}
        <span style={{ width: 1, height: 20, background: "var(--nova-line)", margin: "0 8px" }}/>
        <span style={{ fontSize: 12, color: "var(--color-text-3)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.08em", marginRight: 4 }}>Status:</span>
        {["All", "Unanswered", "Correct", "Incorrect"].map((f, i) => (
          <button key={f} style={{
            padding: "6px 14px", borderRadius: 999,
            border: "1px solid var(--nova-line)",
            background: "transparent",
            color: "var(--color-text-2)",
            fontSize: 12, fontWeight: 600
          }}>{f}</button>
        ))}
      </div>

      {/* Question card */}
      <div className="nv-card" style={{ padding: 40 }}>
        <div style={{ display: "flex", gap: 8, marginBottom: 20, alignItems: "center" }}>
          <span style={{ fontSize: 11, fontWeight: 700, color: "var(--nova-coral)", background: "var(--nova-coral-soft)", padding: "4px 12px", borderRadius: 999 }}>HARD</span>
          <span style={{ fontSize: 12, color: "var(--color-text-3)" }}>Objective 8.3 · Finance Engine API</span>
        </div>
        <p style={{ fontSize: 22, fontWeight: 600, lineHeight: 1.45, margin: "0 0 32px", letterSpacing: "-0.005em" }}>
          When writing a Finance business rule that calculates a member formula based on another account's data, which approach correctly routes through the Data Buffer cell loop?
        </p>

        <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
          {opts.map(o => {
            let state = "default";
            if (answered) {
              if (o.l === correct) state = "correct";
              else if (o.l === selected) state = "wrong";
            } else if (o.l === selected) state = "selected";

            return (
              <button key={o.l} onClick={() => { if (!answered) { setSelected(o.l); setAnswered(true); }}}
                style={{
                  textAlign: "left", padding: "18px 22px",
                  border: `1.5px solid ${
                    state === "correct" ? "var(--nova-teal)" :
                    state === "wrong" ? "var(--nova-coral)" :
                    state === "selected" ? "var(--nova-ink)" : "var(--nova-line)"
                  }`,
                  background:
                    state === "correct" ? "var(--nova-teal-soft)" :
                    state === "wrong" ? "var(--nova-coral-soft)" :
                    "var(--nova-paper)",
                  borderRadius: 14, fontSize: 15, fontWeight: 500, lineHeight: 1.5,
                  display: "flex", gap: 14, alignItems: "flex-start",
                  transition: "all 140ms"
                }}>
                <strong style={{
                  flexShrink: 0,
                  width: 28, height: 28, borderRadius: 999,
                  background: state === "correct" ? "var(--nova-teal)" : state === "wrong" ? "var(--nova-coral)" : "var(--nova-line-soft)",
                  color: state === "correct" || state === "wrong" ? "#fff" : "var(--color-text-2)",
                  display: "grid", placeItems: "center", fontSize: 13
                }}>{o.l}</strong>
                <span style={{ flex: 1 }}>{o.t}</span>
                {state === "correct" && <span style={{ color: "var(--nova-teal)" }}>✓</span>}
                {state === "wrong" && <span style={{ color: "var(--nova-coral)" }}>✗</span>}
              </button>
            );
          })}
        </div>

        {answered && (
          <div style={{ marginTop: 24, padding: 24, background: "var(--nova-line-soft)", borderRadius: 14, borderLeft: "4px solid var(--nova-teal)" }}>
            <p className="nv-eyebrow" style={{ color: "var(--nova-teal-ink)", marginBottom: 8 }}>CORRECT — B</p>
            <p style={{ fontSize: 15, lineHeight: 1.6, margin: "0 0 12px" }}>
              The Finance Engine's <strong>Data Buffer cell loop</strong> is the officially supported path for cross-account calculations. Using <code style={{ background: "var(--nova-paper)", padding: "2px 6px", borderRadius: 4, fontSize: 13 }}>api.Data.Calculate()</code> ensures the engine respects scenario, POV, and consolidation rules.
            </p>
            <a style={{ fontSize: 13, color: "var(--nova-coral)", fontWeight: 600 }}>
              📖 Read more: Finance Rules Handbook · Ch. 03 — API Data Calculate →
            </a>
          </div>
        )}
      </div>

      {/* Nav */}
      <div style={{ display: "flex", justifyContent: "space-between", marginTop: 24 }}>
        <button className="nv-btn nv-btn--ghost">← Previous</button>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="nv-btn nv-btn--ghost">Skip to unanswered</button>
          <button className="nv-btn nv-btn--primary" onClick={() => { setSelected(null); setAnswered(false); }}>Next question →</button>
        </div>
      </div>
    </div>
  );
}

// ── Progress page
function ProgressPage({ setPage }) {
  return (
    <div className="nv-page fade-in">
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Exam Prep", onClick: () => setPage("index") }, { label: "Progress" }]} />
      <h1 style={{ fontSize: 56, letterSpacing: "-0.03em", fontWeight: 700, margin: "0 0 40px" }}>Progress</h1>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 16, marginBottom: 32 }}>
        <div className="nv-card" style={{ padding: 28, background: "var(--nova-coral)", color: "#fff", border: "none" }}>
          <p className="nv-eyebrow" style={{ color: "rgba(255,255,255,0.75)" }}>ANSWERED</p>
          <div style={{ fontSize: 72, fontWeight: 700, letterSpacing: "-0.03em", lineHeight: 1 }}>247</div>
          <div style={{ fontSize: 14, opacity: 0.8, marginTop: 4 }}>of 618 questions (40%)</div>
        </div>
        <div className="nv-card" style={{ padding: 28 }}>
          <p className="nv-eyebrow">AVG SCORE</p>
          <div style={{ fontSize: 72, fontWeight: 700, letterSpacing: "-0.03em", lineHeight: 1, color: "var(--nova-teal)" }}>81%</div>
          <div style={{ fontSize: 14, color: "var(--color-text-2)", marginTop: 4 }}>Passing threshold is 70%</div>
        </div>
        <div className="nv-card" style={{ padding: 28 }}>
          <p className="nv-eyebrow">STUDY STREAK</p>
          <div style={{ fontSize: 72, fontWeight: 700, letterSpacing: "-0.03em", lineHeight: 1 }}>12 <span style={{ fontSize: 32, color: "var(--color-text-3)" }}>days</span></div>
          <div style={{ fontSize: 14, color: "var(--color-text-2)", marginTop: 4 }}>3 Pomodoro sessions today</div>
        </div>
      </div>

      <div className="nv-card" style={{ padding: 32 }}>
        <h3 style={{ fontSize: 20, fontWeight: 700, margin: "0 0 20px" }}>Per-section accuracy</h3>
        {[
          { s: "Cube Design", p: 57, a: 85 },
          { s: "Workflow", p: 51, a: 80 },
          { s: "Data Collection", p: 41, a: 79 },
          { s: "Presentation", p: 57, a: 82 },
          { s: "Tools & Admin", p: 29, a: 78 },
          { s: "Security", p: 44, a: 93 },
          { s: "Administration", p: 19, a: 80 },
          { s: "Business Rules", p: 7, a: 75 },
        ].map(r => (
          <div key={r.s} style={{ display: "grid", gridTemplateColumns: "180px 1fr 60px", gap: 16, alignItems: "center", padding: "14px 0", borderTop: "1px solid var(--nova-line)" }}>
            <div style={{ fontSize: 14, fontWeight: 600 }}>{r.s}</div>
            <div style={{ display: "flex", gap: 2, height: 8, background: "var(--nova-line)", borderRadius: 999, overflow: "hidden" }}>
              <div style={{ width: `${r.p}%`, background: "var(--nova-coral)" }}/>
            </div>
            <div style={{ fontSize: 14, fontWeight: 700, textAlign: "right", color: r.a >= 80 ? "var(--nova-teal)" : "var(--color-text)" }}>{r.a}%</div>
          </div>
        ))}
      </div>
    </div>
  );
}

// ── Flashcards page
function FlashcardsPage({ setPage }) {
  const [flipped, setFlipped] = React.useState(false);
  return (
    <div className="nv-page fade-in" style={{ maxWidth: 900 }}>
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Exam Prep", onClick: () => setPage("index") }, { label: "Flashcards" }]} />
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 32 }}>
        <h1 style={{ fontSize: 48, fontWeight: 700, letterSpacing: "-0.03em", margin: 0 }}>Flashcards</h1>
        <div style={{ fontSize: 14, color: "var(--color-text-2)" }}>Card <strong>7</strong> of <strong>120</strong> · Cube Design</div>
      </div>

      <div onClick={() => setFlipped(f => !f)} style={{
        background: flipped ? "var(--nova-teal)" : "var(--nova-coral)",
        color: "#fff", borderRadius: 32, padding: 80,
        minHeight: 400, display: "grid", placeItems: "center",
        textAlign: "center", cursor: "pointer",
        position: "relative", overflow: "hidden",
        transition: "background 360ms var(--ease-out)"
      }}>
        <BrandGlyph kind={flipped ? "circle" : "arch"} size={300} color="rgba(255,255,255,0.10)" style={{ position: "absolute", right: -40, bottom: -40 }}/>
        <div style={{ position: "relative", zIndex: 1 }}>
          <p className="nv-eyebrow" style={{ color: "rgba(255,255,255,0.7)", marginBottom: 16 }}>{flipped ? "ANSWER" : "QUESTION"}</p>
          <p style={{ fontSize: 36, fontWeight: 600, lineHeight: 1.3, letterSpacing: "-0.015em", margin: 0, maxWidth: 640 }}>
            {flipped
              ? "One — every cube has exactly one Time dimension. It is mandatory and singular."
              : "How many Time dimensions can a single cube contain?"}
          </p>
          <p style={{ fontSize: 13, opacity: 0.6, marginTop: 32 }}>Click to {flipped ? "flip back" : "reveal"}</p>
        </div>
      </div>

      <div style={{ display: "flex", justifyContent: "space-between", marginTop: 24, gap: 12 }}>
        <button className="nv-btn nv-btn--ghost">← Previous</button>
        <div style={{ display: "flex", gap: 8 }}>
          <button style={{ padding: "10px 20px", borderRadius: 999, background: "var(--nova-coral-soft)", color: "var(--nova-coral-ink)", fontWeight: 600, fontSize: 14 }}>Hard</button>
          <button style={{ padding: "10px 20px", borderRadius: 999, background: "var(--nova-line-soft)", fontWeight: 600, fontSize: 14 }}>Medium</button>
          <button style={{ padding: "10px 20px", borderRadius: 999, background: "var(--nova-teal-soft)", color: "var(--nova-teal-ink)", fontWeight: 600, fontSize: 14 }}>Easy</button>
        </div>
        <button className="nv-btn nv-btn--primary">Next →</button>
      </div>
    </div>
  );
}

// ── Glossary
function GlossaryPage({ setPage }) {
  const terms = [
    { t: "Assembly", d: "A container for related business rules, dashboards, and database tables within a workspace.", ch: "Workspaces · Ch. 05" },
    { t: "BI Blend", d: "OneStream's analytic engine that joins relational stage data with cube data for reporting.", ch: "Foundation · Ch. 12" },
    { t: "Cube View", d: "A configurable matrix of dimensions rendered as a grid — rows, columns, and POV.", ch: "Design · Ch. 12" },
    { t: "Data Buffer", d: "An in-memory collection of data cells used inside a Finance rule cell loop.", ch: "Finance · Ch. 04" },
    { t: "Dimension", d: "A hierarchical axis of a cube — Entity, Account, Time, Scenario, etc.", ch: "Design · Ch. 02" },
    { t: "Extender", d: "A business rule type for custom logic not tied to the Finance Engine.", ch: "Foundation · Ch. 08" },
    { t: "Finance Engine", d: "The core calculation engine that processes consolidation, translation, and custom rules.", ch: "Finance · Ch. 01" },
    { t: "POV", d: "Point of View — the currently selected dimension members defining a user's context.", ch: "Design · Ch. 05" },
    { t: "Workflow", d: "The process orchestration model: Import → Validate → Load → Confirm → Certify.", ch: "Design · Ch. 09" },
  ];
  return (
    <div className="nv-page fade-in">
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Exam Prep", onClick: () => setPage("index") }, { label: "Glossary" }]} />
      <h1 style={{ fontSize: 56, letterSpacing: "-0.03em", fontWeight: 700, margin: "0 0 12px" }}>Glossary</h1>
      <p style={{ fontSize: 18, color: "var(--color-text-2)", margin: "0 0 32px", maxWidth: 600 }}>Core OneStream terms, auto-extracted from chapter headings. {terms.length} terms.</p>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(2, 1fr)", gap: 16 }}>
        {terms.map(term => (
          <div key={term.t} className="nv-card" style={{ padding: 24 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "baseline", marginBottom: 8 }}>
              <h3 style={{ fontSize: 20, fontWeight: 700, margin: 0, letterSpacing: "-0.01em" }}>{term.t}</h3>
              <span style={{ fontSize: 11, color: "var(--nova-coral)", fontWeight: 600 }}>{term.ch}</span>
            </div>
            <p style={{ fontSize: 14, color: "var(--color-text-2)", lineHeight: 1.55, margin: 0 }}>{term.d}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

window.ExamIndexPage = ExamIndexPage;
window.ChapterPage = ChapterPage;
window.QuizSectionsPage = QuizSectionsPage;
window.QuizPage = QuizPage;
window.ProgressPage = ProgressPage;
window.FlashcardsPage = FlashcardsPage;
window.GlossaryPage = GlossaryPage;
