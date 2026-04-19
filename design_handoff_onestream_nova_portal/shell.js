// Sidebar, TopBar, Pomodoro, Breadcrumb — shared chrome
// Note: use React.useState/useEffect/useRef directly to avoid const redeclare across babel scripts

function NovaMark({ size = 32 }) {
  // Real Nova brand mark rendered via CSS mask on the container.
  // (background = coral, glyphs = aqua). This component renders nothing.
  return null;
}

function Glyph({ kind, size = 20, color = "currentColor" }) {
  const paths = {
    home: "M3 11l9-8 9 8v10a2 2 0 01-2 2h-4v-6h-6v6H5a2 2 0 01-2-2V11z",
    books: "M4 4h5a3 3 0 013 3v14a2 2 0 00-2-2H4V4zm16 0h-5a3 3 0 00-3 3v14a2 2 0 012-2h6V4z",
    quiz: "M12 2a10 10 0 100 20 10 10 0 000-20zm0 15v.01M12 7a3 3 0 013 3c0 2-3 2.5-3 4",
    exam: "M9 11l3 3 8-8M3 12a9 9 0 1015 6.7",
    code: "M8 5l-5 7 5 7m8-14l5 7-5 7m-4-14l-4 14",
    rules: "M3 5h18M3 12h18M3 19h18",
    flashcards: "M3 6h12v12H3zM9 4h12v12",
    progress: "M3 17l4-6 4 4 6-9 4 5",
    glossary: "M4 4h12a4 4 0 014 4v12a2 2 0 00-2-2H4V4zm4 6h8M8 14h5",
    highlights: "M4 20l2-5 10-10 3 3-10 10-5 2zM13 5l3 3",
    objectives: "M12 2l2 7h7l-6 4 2 7-5-4-5 4 2-7-6-4h7z",
    summary: "M6 4h12M6 12h12M6 20h8",
    puzzle: "M10 3a2 2 0 114 0v2h5v5a2 2 0 110 4h-5v5a2 2 0 11-4 0H5a2 2 0 002-2v-3h-2a2 2 0 110-4h2V8a2 2 0 00-2-2h5z",
    search: "M10 17a7 7 0 100-14 7 7 0 000 14zm11 4l-6-6",
    moon: "M21 13a9 9 0 01-11.3-11A9 9 0 1021 13z",
    sun: "M12 4v2m0 12v2m8-8h-2M6 12H4m13.5-5.5L16 8m-8 8l-1.5 1.5m11 0L16 16m-8-8L6.5 6.5M12 8a4 4 0 100 8 4 4 0 000-8z",
    menu: "M3 6h18M3 12h18M3 18h18",
    timer: "M12 8v4l3 2m-3 6a8 8 0 100-16 8 8 0 000 16zM9 2h6",
    bell: "M6 8a6 6 0 0112 0v5l2 2H4l2-2V8zm3 10a3 3 0 006 0",
    ext: "M12 3l9 4v10l-9 4-9-4V7l9-4zm0 0v18m9-14l-9 4-9-4",
    arrow: "M5 12h14m-6-6l6 6-6 6",
    back: "M19 12H5m6 6l-6-6 6-6",
    chapter: "M7 4h10a2 2 0 012 2v14l-7-3-7 3V6a2 2 0 012-2z",
  };
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <path d={paths[kind] || paths.home} />
    </svg>
  );
}

// Big decorative brand glyphs (arch, circle, triangle, teardrop) — Nova signature
function BrandGlyph({ kind, size = 160, color = "var(--nova-coral)", style = {} }) {
  const els = {
    arch: <path d={`M${size*0.1} ${size} V${size*0.4} a${size*0.4} ${size*0.4} 0 0 1 ${size*0.8} 0 V${size}`} stroke={color} strokeWidth={size*0.18} fill="none" strokeLinecap="butt" />,
    circle: <circle cx={size/2} cy={size/2} r={size*0.45} fill={color} />,
    triangle: <polygon points={`${size*0.05},${size*0.95} ${size*0.95},${size*0.95} ${size/2},${size*0.1}`} fill={color} />,
    teardrop: <path d={`M${size*0.5} ${size*0.05} a${size*0.45} ${size*0.45} 0 1 0 ${size*0.01} 0 L${size*0.95} ${size*0.5} L${size*0.95} ${size*0.95} L${size*0.5} ${size*0.95} Z`} fill={color} />,
  };
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} style={style}>
      {els[kind]}
    </svg>
  );
}

function Sidebar({ page, setPage }) {
  const items = [
    { group: "OVERVIEW", items: [
      { id: "portal", label: "Portal", glyph: "home" },
    ]},
    { group: "EXAM PREP", items: [
      { id: "index", label: "Study Home", glyph: "books" },
      { id: "chapter", label: "Reader", glyph: "chapter" },
      { id: "quiz-sections", label: "Quiz", glyph: "quiz" },
      { id: "quiz", label: "Practice", glyph: "exam", badge: "618" },
      { id: "progress", label: "Progress", glyph: "progress" },
      { id: "flashcards", label: "Flashcards", glyph: "flashcards" },
      { id: "glossary", label: "Glossary", glyph: "glossary" },
    ]},
    { group: "REFERENCE", items: [
      { id: "api", label: "API Reference", glyph: "code", badge: "5.3k" },
      { id: "rules", label: "Business Rules", glyph: "rules" },
      { id: "extension", label: "Extension Guide", glyph: "ext" },
    ]},
  ];
  return (
    <aside className="nv-sidebar">
      <div className="nv-brand" onClick={() => setPage("portal")} style={{ cursor: "pointer" }}>
        <div className="nv-brand-mark" style={{ "--nv-logo-mask": `url("${window.__novaLogoSrc}")` }}>
          <NovaMark size={22} />
        </div>
        <div className="nv-brand-name">
          OneStream
          <small>Nova Portal</small>
        </div>
      </div>
      {items.map(group => (
        <div className="nv-nav-group" key={group.group}>
          <div className="nv-nav-label">{group.group}</div>
          {group.items.map(it => (
            <div key={it.id}
              className={`nv-nav-item ${page === it.id ? "active" : ""}`}
              onClick={() => setPage(it.id)}>
              <Glyph kind={it.glyph} size={18} />
              <span>{it.label}</span>
              {it.badge && <span className="badge">{it.badge}</span>}
            </div>
          ))}
        </div>
      ))}
      <div className="nv-sidebar-footer">
        <div style={{ fontSize: 11, color: "var(--color-text-3)", padding: "0 12px" }}>
          OS-201 Certification<br/>
          v2.0 · Nova Design System
        </div>
      </div>
    </aside>
  );
}

function TopBar({ theme, toggleTheme }) {
  return (
    <div className="nv-topbar">
      <div className="nv-search">
        <span className="nv-search-icon"><Glyph kind="search" size={16} /></span>
        <input type="search" placeholder="Search chapters, rules, APIs..." />
        <kbd>⌘K</kbd>
      </div>
      <div className="nv-topbar-actions">
        <button className="nv-icon-btn" title="Notifications">
          <Glyph kind="bell" size={18} />
        </button>
        <button className="nv-icon-btn" onClick={toggleTheme} title="Toggle theme">
          <Glyph kind={theme === "dark" ? "sun" : "moon"} size={18} />
        </button>
        <div style={{ width: 1, height: 24, background: "var(--nova-line)", margin: "0 4px" }}/>
        <div style={{
          width: 36, height: 36, borderRadius: 999,
          background: "var(--nova-teal)", color: "#fff",
          display: "grid", placeItems: "center",
          fontWeight: 700, fontSize: 13
        }}>MP</div>
      </div>
    </div>
  );
}

function Pomodoro() {
  const [open, setOpen] = React.useState(false);
  const [running, setRunning] = React.useState(false);
  const [mode, setMode] = React.useState("study"); // study | break
  const [seconds, setSeconds] = React.useState(25 * 60);
  const intv = React.useRef(null);

  React.useEffect(() => {
    if (running) {
      intv.current = setInterval(() => {
        setSeconds(s => {
          if (s <= 1) {
            clearInterval(intv.current);
            setRunning(false);
            setMode(m => m === "study" ? "break" : "study");
            return (mode === "study" ? 5 : 25) * 60;
          }
          return s - 1;
        });
      }, 1000);
    }
    return () => clearInterval(intv.current);
  }, [running, mode]);

  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  const time = `${m}:${String(s).padStart(2, "0")}`;

  return (
    <div className="nv-pomo">
      {open && (
        <div className="nv-pomo-panel open fade-in">
          <div className={`nv-pomo-time ${mode === "break" ? "break" : ""}`}>{time}</div>
          <div className="nv-pomo-label">{mode === "study" ? "Focus Session" : "Break"}</div>
          <div className="nv-pomo-btns">
            <button className="start" onClick={() => setRunning(r => !r)}>
              {running ? "Pause" : "Start"}
            </button>
            <button className="reset" onClick={() => {
              setRunning(false);
              setMode("study");
              setSeconds(25 * 60);
            }}>Reset</button>
          </div>
          <div style={{ marginTop: 16, fontSize: 11, textAlign: "center", color: "var(--color-text-3)", letterSpacing: "0.08em", textTransform: "uppercase" }}>
            Sessions today: <strong style={{ color: "var(--nova-coral)" }}>3</strong> · 75 min
          </div>
        </div>
      )}
      <button className={`nv-pomo-fab ${running ? "running" : ""}`} onClick={() => setOpen(o => !o)}>
        <Glyph kind="timer" size={22} color="#fff" />
      </button>
    </div>
  );
}

function Crumb({ items }) {
  return (
    <nav className="nv-crumb">
      {items.map((it, i) => (
        <React.Fragment key={i}>
          {i > 0 && <span className="sep">/</span>}
          {it.onClick ? <a onClick={it.onClick} style={{ cursor: "pointer" }}>{it.label}</a> :
           <span className={i === items.length - 1 ? "current" : ""}>{it.label}</span>}
        </React.Fragment>
      ))}
    </nav>
  );
}

window.NovaMark = NovaMark;
window.Glyph = Glyph;
window.BrandGlyph = BrandGlyph;
window.Sidebar = Sidebar;
window.TopBar = TopBar;
window.Pomodoro = Pomodoro;
window.Crumb = Crumb;
