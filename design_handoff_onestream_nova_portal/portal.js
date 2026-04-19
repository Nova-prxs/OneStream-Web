// Portal page — 3 variants selectable via tweaks

function PortalPage({ setPage, variant, setVariant }) {
  return (
    <div className="nv-page fade-in">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24 }}>
        <span className="nv-eyebrow">OS-201 · Platform Architect</span>
        <div style={{ display: "flex", gap: 4, background: "var(--nova-paper)", border: "1px solid var(--nova-line)", borderRadius: 999, padding: 4 }}>
          {["A", "B", "C"].map(v => (
            <button key={v} onClick={() => setVariant(v)}
              style={{
                padding: "6px 14px", borderRadius: 999, fontSize: 12, fontWeight: 600,
                background: variant === v ? "var(--nova-ink)" : "transparent",
                color: variant === v ? "#fff" : "var(--color-text-2)"
              }}>
              Variant {v}
            </button>
          ))}
        </div>
      </div>

      {variant === "A" && <PortalA setPage={setPage} />}
      {variant === "B" && <PortalB setPage={setPage} />}
      {variant === "C" && <PortalC setPage={setPage} />}
    </div>
  );
}

// ── Variant A: bone hero with oversized coral arch glyph
function PortalA({ setPage }) {
  const cards = [
    { id: "index", title: "Exam Prep", desc: "4 books · 60 chapters · 618 questions with explanations, flashcards and timed exam simulation.", color: "coral", glyph: "arch" },
    { id: "api", title: "API Reference", desc: "5,323 classes across 288 namespaces with full method signatures and live examples.", color: "teal", glyph: "circle" },
    { id: "rules", title: "Business Rules", desc: "827 real rules — Extender, Finance, Connector, Dashboard, Assemblies.", color: "ink", glyph: "triangle" },
    { id: "extension", title: "Extension Guide", desc: "VS Code extension + MCP server documentation for AI-assisted development.", color: "aqua", glyph: "teardrop" },
  ];
  return (
    <>
      {/* Hero */}
      <section style={{ position: "relative", padding: "40px 0 80px", overflow: "hidden" }}>
        <BrandGlyph kind="arch" size={520} color="var(--nova-coral)" style={{ position: "absolute", top: -40, right: -80, opacity: 0.08, zIndex: 0 }} />
        <div style={{ position: "relative", zIndex: 1, maxWidth: 820 }}>
          <p className="nv-eyebrow" style={{ marginBottom: 20 }}>
            <span style={{ display: "inline-block", width: 6, height: 6, background: "var(--nova-coral)", borderRadius: 999, marginRight: 8, verticalAlign: "middle" }}></span>
            CERTIFICATION PORTAL
          </p>
          <h1 style={{ fontSize: 96, lineHeight: 1.02, letterSpacing: "-0.03em", fontWeight: 700, margin: "0 0 24px" }}>
            Study.<br/>
            <span style={{ color: "var(--nova-coral)" }}>Build.</span> Certify.
          </h1>
          <p style={{ fontSize: 22, lineHeight: 1.5, color: "var(--color-text-2)", maxWidth: 620, margin: "0 0 36px" }}>
            Your centralized hub for OS-201 mastery and complete OneStream API documentation — no fluff, just what you need to ship.
          </p>
          <div style={{ display: "flex", gap: 12 }}>
            <button className="nv-btn nv-btn--primary" onClick={() => setPage("index")}>
              Start studying <Glyph kind="arrow" size={16} />
            </button>
            <button className="nv-btn nv-btn--ghost" onClick={() => setPage("api")}>
              Browse the API
            </button>
          </div>
        </div>
      </section>

      {/* Stats strip */}
      <section style={{
        background: "var(--nova-ink)", color: "var(--nova-paper)",
        borderRadius: "var(--r-lg)", padding: "32px 40px",
        display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 24,
        marginBottom: 48
      }}>
        {[
          { n: "5,323", l: "API Classes" },
          { n: "827", l: "Business Rules" },
          { n: "600k+", l: "Words of Reference" },
          { n: "618", l: "Quiz Questions" },
        ].map(s => (
          <div key={s.l}>
            <div style={{ fontSize: 48, fontWeight: 700, letterSpacing: "-0.03em", lineHeight: 1 }}>{s.n}</div>
            <div style={{ fontSize: 12, color: "#A7E8E4", letterSpacing: "0.14em", textTransform: "uppercase", marginTop: 8, fontWeight: 700 }}>{s.l}</div>
          </div>
        ))}
      </section>

      {/* Cards */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(2, 1fr)", gap: 20, marginBottom: 60 }}>
        {cards.map(c => (
          <PortalCard key={c.id} card={c} onClick={() => setPage(c.id)} />
        ))}
      </section>
    </>
  );
}

function PortalCard({ card, onClick }) {
  const colorMap = {
    coral: { bg: "var(--nova-coral)", fg: "#fff", glyphColor: "rgba(255,255,255,0.18)" },
    teal: { bg: "var(--nova-teal)", fg: "#fff", glyphColor: "rgba(255,255,255,0.18)" },
    ink: { bg: "var(--nova-ink)", fg: "#fff", glyphColor: "rgba(255,255,255,0.14)" },
    aqua: { bg: "var(--nova-aqua)", fg: "var(--nova-ink)", glyphColor: "rgba(46,58,64,0.14)" },
  };
  const c = colorMap[card.color];
  return (
    <div onClick={onClick} style={{
      position: "relative", overflow: "hidden",
      background: c.bg, color: c.fg,
      borderRadius: 24, padding: 40,
      minHeight: 240, cursor: "pointer",
      transition: "transform 220ms var(--ease-out)",
    }} onMouseEnter={e => e.currentTarget.style.transform = "translateY(-4px)"}
       onMouseLeave={e => e.currentTarget.style.transform = "translateY(0)"}>
      <BrandGlyph kind={card.glyph} size={220} color={c.glyphColor} style={{ position: "absolute", right: -30, bottom: -30 }} />
      <div style={{ position: "relative", zIndex: 1, display: "flex", flexDirection: "column", height: "100%" }}>
        <h3 style={{ fontSize: 32, fontWeight: 700, letterSpacing: "-0.02em", margin: "0 0 12px" }}>{card.title}</h3>
        <p style={{ fontSize: 15, lineHeight: 1.55, opacity: 0.88, margin: 0, maxWidth: 420 }}>{card.desc}</p>
        <div style={{ marginTop: "auto", paddingTop: 40, fontWeight: 600, fontSize: 14, display: "flex", alignItems: "center", gap: 8 }}>
          Explore <Glyph kind="arrow" size={16} color={c.fg} />
        </div>
      </div>
    </div>
  );
}

// ── Variant B: split layout, coral + bone, teardrop motif
function PortalB({ setPage }) {
  return (
    <>
      <section style={{
        display: "grid", gridTemplateColumns: "1.2fr 1fr", gap: 0,
        borderRadius: 40, overflow: "hidden", marginBottom: 48,
        border: "1px solid var(--nova-line)"
      }}>
        <div style={{ padding: "80px 56px", background: "var(--nova-paper)", position: "relative" }}>
          <p className="nv-eyebrow" style={{ marginBottom: 16 }}>OS-201 · PLATFORM ARCHITECT</p>
          <h1 style={{ fontSize: 72, lineHeight: 1.03, letterSpacing: "-0.03em", margin: "0 0 20px" }}>
            The handbook,<br/>the drills, and<br/>the <span style={{ color: "var(--nova-coral)" }}>reference</span>.
          </h1>
          <p style={{ fontSize: 18, color: "var(--color-text-2)", maxWidth: 480, marginBottom: 32 }}>
            One portal for OneStream practitioners — study, reference, and real business rules, in one place.
          </p>
          <div style={{ display: "flex", gap: 12 }}>
            <button className="nv-btn nv-btn--primary" onClick={() => setPage("index")}>Start</button>
            <button className="nv-btn nv-btn--ghost" onClick={() => setPage("api")}>Browse API</button>
          </div>
        </div>
        <div style={{ background: "var(--nova-coral)", position: "relative", overflow: "hidden", display: "grid", placeItems: "center" }}>
          <BrandGlyph kind="teardrop" size={360} color="rgba(255,255,255,0.9)" />
          <BrandGlyph kind="circle" size={80} color="var(--nova-aqua)" style={{ position: "absolute", top: 40, left: 40 }} />
          <BrandGlyph kind="triangle" size={60} color="var(--nova-ink)" style={{ position: "absolute", bottom: 40, right: 40 }} />
        </div>
      </section>

      <section style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 16, marginBottom: 48 }}>
        {[
          { id: "index", title: "Exam Prep", n: "618", l: "questions", glyph: "arch" },
          { id: "api", title: "API Reference", n: "5,323", l: "classes", glyph: "circle" },
          { id: "rules", title: "Business Rules", n: "827", l: "real rules", glyph: "triangle" },
          { id: "extension", title: "Extension", n: "12", l: "features", glyph: "teardrop" },
        ].map(s => (
          <div key={s.id} onClick={() => setPage(s.id)} className="nv-card" style={{ cursor: "pointer", padding: 24 }}>
            <BrandGlyph kind={s.glyph} size={36} color="var(--nova-coral)" />
            <div style={{ fontSize: 40, fontWeight: 700, letterSpacing: "-0.02em", marginTop: 32 }}>{s.n}</div>
            <div style={{ fontSize: 12, color: "var(--color-text-3)", textTransform: "uppercase", letterSpacing: "0.14em", fontWeight: 700 }}>{s.l}</div>
            <div style={{ marginTop: 20, fontSize: 15, fontWeight: 600 }}>{s.title} →</div>
          </div>
        ))}
      </section>
    </>
  );
}

// ── Variant C: teal-forward, editorial magazine layout
function PortalC({ setPage }) {
  return (
    <>
      <section style={{ padding: "40px 0 60px", position: "relative" }}>
        <div style={{ display: "grid", gridTemplateColumns: "auto 1fr", gap: 48, alignItems: "end", marginBottom: 40 }}>
          <BrandGlyph kind="arch" size={140} color="var(--nova-coral)" />
          <div style={{ borderBottom: "1px solid var(--nova-line)", paddingBottom: 20 }}>
            <p className="nv-eyebrow">ISSUE 01 · SPRING EDITION</p>
            <h1 style={{ fontSize: 120, lineHeight: 0.95, letterSpacing: "-0.04em", margin: "12px 0 0", fontWeight: 700 }}>
              OneStream,<br/>end to end.
            </h1>
          </div>
        </div>
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 40, marginTop: 40 }}>
          <p style={{ fontSize: 18, lineHeight: 1.55, margin: 0 }}>
            This is the portal for OS-201 candidates and working architects. Study the official guides, drill through 618 weighted questions, and cross-reference real business rules next to the API that defines them.
          </p>
          <div>
            <p className="nv-eyebrow" style={{ marginBottom: 8 }}>IN THIS EDITION</p>
            <ul style={{ margin: 0, paddingLeft: 0, listStyle: "none", fontSize: 15, lineHeight: 1.8 }}>
              <li>— 4 reference books</li>
              <li>— 8 exam sections</li>
              <li>— 827 real business rules</li>
              <li>— MCP server + VS Code</li>
            </ul>
          </div>
          <div style={{ display: "flex", flexDirection: "column", gap: 8, alignItems: "flex-start" }}>
            <button className="nv-btn nv-btn--primary" onClick={() => setPage("index")}>Start studying</button>
            <button className="nv-btn nv-btn--teal" onClick={() => setPage("api")}>Open API reference</button>
          </div>
        </div>
      </section>

      {/* Feature blocks */}
      <section style={{ display: "grid", gridTemplateColumns: "2fr 1fr", gap: 20, marginBottom: 20 }}>
        <div onClick={() => setPage("index")} style={{ background: "var(--nova-teal)", color: "#fff", borderRadius: 40, padding: 48, position: "relative", overflow: "hidden", cursor: "pointer", minHeight: 320 }}>
          <BrandGlyph kind="circle" size={260} color="rgba(167,232,228,0.25)" style={{ position: "absolute", right: -40, top: -40 }} />
          <p className="nv-eyebrow" style={{ color: "var(--nova-aqua)", marginBottom: 16 }}>FEATURED · STUDY TRACK</p>
          <h2 style={{ fontSize: 56, fontWeight: 700, letterSpacing: "-0.02em", margin: "0 0 16px", maxWidth: 560 }}>Everything for OS-201 in one reading track.</h2>
          <p style={{ fontSize: 17, opacity: 0.88, maxWidth: 480, marginBottom: 32 }}>Design Reference Guide, Finance Rules, Foundation Handbook, Workspaces & Assemblies — all cross-linked, all searchable.</p>
          <div style={{ fontSize: 15, fontWeight: 600, display: "flex", gap: 8 }}>Enter the reader <Glyph kind="arrow" size={16} /></div>
        </div>
        <div onClick={() => setPage("quiz-sections")} style={{ background: "var(--nova-coral)", color: "#fff", borderRadius: 40, padding: 40, position: "relative", overflow: "hidden", cursor: "pointer" }}>
          <BrandGlyph kind="triangle" size={180} color="rgba(255,255,255,0.16)" style={{ position: "absolute", right: -20, bottom: -20 }} />
          <p className="nv-eyebrow" style={{ color: "#fff", opacity: 0.85, marginBottom: 16 }}>PRACTICE</p>
          <h2 style={{ fontSize: 36, fontWeight: 700, letterSpacing: "-0.02em", margin: "0 0 12px" }}>618 weighted questions.</h2>
          <p style={{ fontSize: 14, opacity: 0.9 }}>8 exam sections. Track your score, skip to unanswered, review the ones you missed.</p>
          <div style={{ marginTop: 24, fontSize: 14, fontWeight: 600 }}>Start drills →</div>
        </div>
      </section>

      <section style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 20, marginBottom: 60 }}>
        {[
          { id: "api", title: "API Reference", sub: "5,323 classes · 288 namespaces", glyph: "circle", color: "#1A8A7F" },
          { id: "rules", title: "Business Rules", sub: "827 production-grade rules", glyph: "arch", color: "#2E3A40" },
          { id: "extension", title: "Extension Guide", sub: "VS Code + MCP server", glyph: "teardrop", color: "#FF5948" },
        ].map(s => (
          <div key={s.id} onClick={() => setPage(s.id)} className="nv-card" style={{ cursor: "pointer", padding: 32 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 24 }}>
              <BrandGlyph kind={s.glyph} size={44} color={s.color} />
              <Glyph kind="arrow" size={18} color="var(--color-text-3)" />
            </div>
            <div style={{ fontSize: 20, fontWeight: 700, letterSpacing: "-0.01em", marginBottom: 4 }}>{s.title}</div>
            <div style={{ fontSize: 13, color: "var(--color-text-2)" }}>{s.sub}</div>
          </div>
        ))}
      </section>
    </>
  );
}

window.PortalPage = PortalPage;
