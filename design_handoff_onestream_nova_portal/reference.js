// API Reference, Business Rules, Extension pages

// ── API Reference page
function ApiPage({ setPage }) {
  const [selected, setSelected] = React.useState("OneStream.Finance.Api.BusinessRule");
  const namespaces = [
    { ns: "OneStream.Finance", count: 842, open: true, classes: [
      { n: "Api.BusinessRule", t: "Class", selected: true },
      { n: "Api.DataBuffer", t: "Class" },
      { n: "Api.DataCell", t: "Class" },
      { n: "Api.Pov", t: "Class" },
      { n: "Api.FinanceFunctions", t: "Class" },
    ]},
    { ns: "OneStream.Data", count: 420 },
    { ns: "OneStream.Dashboard", count: 618 },
    { ns: "OneStream.Assemblies", count: 312 },
    { ns: "OneStream.Workspaces", count: 180 },
    { ns: "OneStream.Admin", count: 278 },
    { ns: "OneStream.Security", count: 124 },
  ];

  return (
    <div className="nv-page fade-in" style={{ maxWidth: 1400 }}>
      <Crumb items={[
        { label: "Portal", onClick: () => setPage("portal") },
        { label: "API Reference" },
        { label: "OneStream.Finance" },
        { label: "BusinessRule" }
      ]} />

      <div style={{ display: "grid", gridTemplateColumns: "260px 1fr 280px", gap: 32 }}>
        {/* LEFT: namespace tree */}
        <aside style={{ position: "sticky", top: 80, alignSelf: "start", height: "calc(100vh - 120px)", overflowY: "auto" }}>
          <p className="nv-eyebrow" style={{ marginBottom: 12 }}>NAMESPACES · 288</p>
          <nav>
            {namespaces.map(ns => (
              <div key={ns.ns} style={{ marginBottom: 4 }}>
                <div style={{
                  display: "flex", alignItems: "center", gap: 6,
                  padding: "8px 10px", borderRadius: 8,
                  fontSize: 13, fontWeight: 600,
                  background: ns.open ? "var(--nova-line-soft)" : "transparent",
                  cursor: "pointer"
                }}>
                  <span style={{ color: "var(--color-text-3)", fontSize: 10, transform: ns.open ? "rotate(90deg)" : "none" }}>▶</span>
                  <span style={{ flex: 1, fontFamily: "var(--font-mono)" }}>{ns.ns}</span>
                  <span style={{ fontSize: 10, color: "var(--color-text-3)", fontWeight: 700 }}>{ns.count}</span>
                </div>
                {ns.open && ns.classes && (
                  <div style={{ borderLeft: "1px solid var(--nova-line)", marginLeft: 18, paddingLeft: 8 }}>
                    {ns.classes.map(c => (
                      <div key={c.n} style={{
                        padding: "6px 10px", fontSize: 12.5,
                        fontFamily: "var(--font-mono)",
                        borderRadius: 6,
                        background: c.selected ? "var(--nova-coral)" : "transparent",
                        color: c.selected ? "#fff" : "var(--color-text-2)",
                        fontWeight: c.selected ? 600 : 400,
                        cursor: "pointer",
                        display: "flex", alignItems: "center", gap: 6
                      }}>
                        <span style={{
                          width: 16, height: 16, borderRadius: 4,
                          background: c.selected ? "rgba(255,255,255,0.2)" : "var(--nova-teal)",
                          color: "#fff", fontSize: 10, fontWeight: 700,
                          display: "grid", placeItems: "center", flexShrink: 0
                        }}>C</span>
                        {c.n}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </nav>
        </aside>

        {/* CENTER */}
        <main style={{ minWidth: 0 }}>
          <div style={{ display: "flex", alignItems: "baseline", gap: 12, marginBottom: 8 }}>
            <span style={{
              background: "var(--nova-teal)", color: "#fff",
              fontSize: 11, fontWeight: 700, padding: "4px 10px",
              borderRadius: 999
            }}>CLASS</span>
            <span style={{ fontFamily: "var(--font-mono)", fontSize: 13, color: "var(--color-text-3)" }}>OneStream.Finance.Api</span>
          </div>
          <h1 style={{ fontSize: 48, fontWeight: 700, letterSpacing: "-0.02em", margin: "0 0 16px", fontFamily: "var(--font-mono)" }}>BusinessRule</h1>
          <p style={{ fontSize: 17, lineHeight: 1.55, color: "var(--color-text-2)", margin: "0 0 32px", maxWidth: 640 }}>
            Base class for all Finance business rules. Provides access to the POV, data buffer, and finance engine helpers. All custom Finance rules should inherit from this class.
          </p>

          {/* Tabs */}
          <div style={{ display: "flex", gap: 2, borderBottom: "1px solid var(--nova-line)", marginBottom: 24 }}>
            {["Overview", "Methods (24)", "Properties (8)", "Events (3)", "Examples"].map((t, i) => (
              <button key={t} style={{
                padding: "10px 18px",
                fontSize: 13, fontWeight: 600,
                color: i === 1 ? "var(--nova-coral)" : "var(--color-text-2)",
                borderBottom: i === 1 ? "2px solid var(--nova-coral)" : "2px solid transparent",
                marginBottom: -1
              }}>{t}</button>
            ))}
          </div>

          {/* Methods list */}
          <div style={{ display: "flex", flexDirection: "column", gap: 14 }}>
            {[
              { rt: "DataBuffer", n: "Calculate", params: "string formula, Scenario scn", d: "Evaluates a formula against the current POV and returns a DataBuffer with the result. Use inside cell-loop contexts for cross-account arithmetic." },
              { rt: "void", n: "SetDataCell", params: "DataCell cell, decimal amount", d: "Write a value to a data cell. Respects consolidation, translation, and calculation order." },
              { rt: "Pov", n: "GetPovForFormulas", params: "", d: "Returns the POV the current formula is being evaluated against." },
              { rt: "bool", n: "IsBaseEntity", params: "Member entity", d: "True if the supplied entity has no children in the consolidation hierarchy." },
            ].map(m => (
              <div key={m.n} className="nv-card" style={{ padding: 20 }}>
                <div style={{ fontFamily: "var(--font-mono)", fontSize: 14, marginBottom: 8 }}>
                  <span style={{ color: "var(--nova-teal)" }}>{m.rt}</span>
                  {" "}
                  <strong style={{ color: "var(--nova-coral)" }}>{m.n}</strong>
                  <span style={{ color: "var(--color-text-3)" }}>({m.params})</span>
                </div>
                <p style={{ fontSize: 14, color: "var(--color-text-2)", margin: 0, lineHeight: 1.55 }}>{m.d}</p>
              </div>
            ))}
          </div>

          {/* Example */}
          <h3 style={{ fontSize: 24, fontWeight: 700, letterSpacing: "-0.01em", margin: "40px 0 16px" }}>Example</h3>
          <pre style={{
            background: "var(--nova-ink)", color: "#E7F7F6",
            padding: 24, borderRadius: 14,
            fontFamily: "var(--font-mono)", fontSize: 13, lineHeight: 1.6,
            overflow: "auto", margin: 0
          }}>
{`// Calculate NetSales from GrossSales minus Returns
Function Main(si As SessionInfo, globals, api, args)
    Dim result As DataBuffer = api.Data.Calculate("A#GrossSales - A#Returns")
    api.Data.SetDataBuffer(result)
End Function`}
          </pre>
        </main>

        {/* RIGHT: on-this-page */}
        <aside style={{ position: "sticky", top: 80, alignSelf: "start", height: "fit-content" }}>
          <p className="nv-eyebrow" style={{ marginBottom: 12 }}>ON THIS PAGE</p>
          <nav style={{ fontSize: 13, lineHeight: 1.9 }}>
            {["Overview", "Inheritance", "Methods", "Example", "Related rules"].map((t, i) => (
              <a key={t} style={{ display: "block", color: i === 2 ? "var(--nova-coral)" : "var(--color-text-2)", fontWeight: i === 2 ? 700 : 500 }}>{t}</a>
            ))}
          </nav>
          <div style={{ marginTop: 32, padding: 16, background: "var(--nova-aqua)", borderRadius: 12, color: "var(--nova-ink)" }}>
            <p className="nv-eyebrow" style={{ color: "var(--nova-teal-ink)", marginBottom: 8 }}>RELATED</p>
            <p style={{ fontSize: 13, lineHeight: 1.5, margin: "0 0 12px" }}>This class is referenced by <strong>47 business rules</strong> in the library.</p>
            <button onClick={() => setPage("rules")} style={{ fontSize: 12, fontWeight: 600, color: "var(--nova-teal-ink)" }}>See all rules →</button>
          </div>
        </aside>
      </div>
    </div>
  );
}

// ── Business Rules page
function RulesPage({ setPage }) {
  const rules = [
    { n: "Consolidation_GoodwillCalc", cat: "Finance", complexity: "Complex", loc: 342, desc: "Calculates goodwill for M&A scenarios during consolidation." },
    { n: "Extender_FXRateLoader", cat: "Extender", complexity: "Medium", loc: 128, desc: "Pulls FX rates from external source and loads into Rate scenario." },
    { n: "Connector_SAPHana", cat: "Connector", complexity: "Complex", loc: 584, desc: "Incremental data pull from SAP Hana into Stage." },
    { n: "Dashboard_CubeViewExport", cat: "Dashboard", complexity: "Simple", loc: 62, desc: "Exports cube view to Excel with custom styling." },
    { n: "Finance_AllocationEngine", cat: "Finance", complexity: "Complex", loc: 412, desc: "Drives cross-dimensional allocation patterns." },
    { n: "Assembly_WorkflowOrchestrator", cat: "Assembly", complexity: "Medium", loc: 218, desc: "Coordinates multi-entity close workflow tasks." },
  ];
  const cats = [
    { name: "All Rules", c: 827, active: true },
    { name: "Finance", c: 312 },
    { name: "Extender", c: 218 },
    { name: "Connector", c: 142 },
    { name: "Dashboard", c: 98 },
    { name: "Assembly", c: 57 },
  ];
  const catColor = {
    Finance: "var(--nova-coral)",
    Extender: "var(--nova-teal)",
    Connector: "var(--nova-ink)",
    Dashboard: "var(--nova-coral)",
    Assembly: "var(--nova-teal)",
  };
  return (
    <div className="nv-page fade-in" style={{ maxWidth: 1300 }}>
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Business Rules" }]} />

      {/* Hero */}
      <section style={{ position: "relative", marginBottom: 40, padding: "32px 0" }}>
        <div style={{ display: "grid", gridTemplateColumns: "1fr auto", alignItems: "center", gap: 40 }}>
          <div>
            <p className="nv-eyebrow" style={{ marginBottom: 16 }}>PRODUCTION-GRADE LIBRARY</p>
            <h1 style={{ fontSize: 64, letterSpacing: "-0.03em", fontWeight: 700, lineHeight: 1.02, margin: "0 0 16px" }}>
              <span style={{ color: "var(--nova-coral)" }}>827</span> real<br/>business rules.
            </h1>
            <p style={{ fontSize: 18, color: "var(--color-text-2)", maxWidth: 580, margin: 0 }}>
              Every rule here is pulled from real OneStream implementations. Read them, copy them, adapt them — with full cross-references to the API they call.
            </p>
          </div>
          <BrandGlyph kind="triangle" size={180} color="var(--nova-ink)" />
        </div>
      </section>

      <div style={{ display: "grid", gridTemplateColumns: "220px 1fr", gap: 32 }}>
        {/* Cat sidebar */}
        <aside>
          <p className="nv-eyebrow" style={{ marginBottom: 12 }}>CATEGORY</p>
          {cats.map(c => (
            <div key={c.name} style={{
              display: "flex", justifyContent: "space-between", alignItems: "center",
              padding: "10px 14px", borderRadius: 10, fontSize: 14,
              background: c.active ? "var(--nova-ink)" : "transparent",
              color: c.active ? "#fff" : "var(--color-text-2)",
              fontWeight: c.active ? 700 : 500,
              marginBottom: 2, cursor: "pointer"
            }}>
              {c.name}
              <span style={{
                fontSize: 11,
                background: c.active ? "rgba(255,255,255,0.15)" : "var(--nova-line-soft)",
                color: c.active ? "#fff" : "var(--color-text-3)",
                padding: "2px 8px", borderRadius: 999, fontWeight: 700
              }}>{c.c}</span>
            </div>
          ))}

          <p className="nv-eyebrow" style={{ marginBottom: 12, marginTop: 32 }}>COMPLEXITY</p>
          {["Simple", "Medium", "Complex"].map(c => (
            <label key={c} style={{ display: "flex", gap: 10, padding: "6px 14px", fontSize: 14, color: "var(--color-text-2)" }}>
              <input type="checkbox" defaultChecked={c !== "Complex"} style={{ accentColor: "var(--nova-coral)" }}/>
              {c}
            </label>
          ))}
        </aside>

        {/* Rules list */}
        <div>
          <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 16, alignItems: "center" }}>
            <p style={{ fontSize: 14, color: "var(--color-text-2)", margin: 0 }}>Showing <strong style={{ color: "var(--color-text)" }}>6 of 827</strong> rules</p>
            <select style={{ padding: "8px 12px", borderRadius: 10, border: "1px solid var(--nova-line)", fontSize: 13, background: "var(--nova-paper)" }}>
              <option>Most referenced</option>
              <option>A → Z</option>
              <option>Recently added</option>
            </select>
          </div>

          <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
            {rules.map(r => (
              <div key={r.n} className="nv-card" style={{ padding: 24, display: "grid", gridTemplateColumns: "1fr auto", gap: 20, alignItems: "center" }}>
                <div style={{ minWidth: 0 }}>
                  <div style={{ display: "flex", gap: 8, marginBottom: 8, alignItems: "center" }}>
                    <span style={{
                      fontSize: 10, fontWeight: 700,
                      background: catColor[r.cat] || "var(--nova-ink)", color: "#fff",
                      padding: "3px 10px", borderRadius: 999, letterSpacing: "0.04em"
                    }}>{r.cat.toUpperCase()}</span>
                    <span style={{
                      fontSize: 10, fontWeight: 700, color: "var(--color-text-3)",
                      border: "1px solid var(--nova-line)", padding: "3px 8px", borderRadius: 999
                    }}>{r.complexity}</span>
                  </div>
                  <h3 style={{ fontFamily: "var(--font-mono)", fontSize: 17, fontWeight: 700, margin: "0 0 6px", letterSpacing: "-0.005em" }}>{r.n}</h3>
                  <p style={{ fontSize: 14, color: "var(--color-text-2)", lineHeight: 1.5, margin: 0 }}>{r.desc}</p>
                </div>
                <div style={{ textAlign: "right", flexShrink: 0 }}>
                  <div style={{ fontSize: 28, fontWeight: 700, letterSpacing: "-0.02em", fontFamily: "var(--font-mono)" }}>{r.loc}</div>
                  <div style={{ fontSize: 11, color: "var(--color-text-3)", textTransform: "uppercase", letterSpacing: "0.14em", fontWeight: 700 }}>Lines</div>
                  <button className="nv-btn nv-btn--ghost" style={{ marginTop: 12, height: 34, padding: "0 14px", fontSize: 12 }}>View →</button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

// ── Extension Guide page
function ExtensionPage({ setPage }) {
  return (
    <div className="nv-page fade-in" style={{ maxWidth: 1200 }}>
      <Crumb items={[{ label: "Portal", onClick: () => setPage("portal") }, { label: "Extension Guide" }]} />

      {/* Hero */}
      <section style={{
        background: "var(--nova-ink)", color: "#fff",
        borderRadius: 32, padding: 56,
        position: "relative", overflow: "hidden",
        marginBottom: 40
      }}>
        <BrandGlyph kind="teardrop" size={360} color="rgba(255,89,72,0.28)" style={{ position: "absolute", right: -60, top: -60 }} />
        <BrandGlyph kind="circle" size={80} color="var(--nova-aqua)" style={{ position: "absolute", right: 100, bottom: 40, opacity: 0.4 }}/>
        <div style={{ position: "relative", zIndex: 1, maxWidth: 640 }}>
          <p className="nv-eyebrow" style={{ color: "var(--nova-aqua)", marginBottom: 16 }}>VS CODE + MCP SERVER</p>
          <h1 style={{ fontSize: 64, letterSpacing: "-0.03em", fontWeight: 700, lineHeight: 1.02, margin: "0 0 20px" }}>
            AI-assisted<br/>OneStream<br/>development.
          </h1>
          <p style={{ fontSize: 18, color: "rgba(255,255,255,0.8)", margin: "0 0 32px", lineHeight: 1.5 }}>
            Write, debug, and deploy business rules directly from VS Code — with a Model Context Protocol server that feeds Claude and Copilot the full OneStream API.
          </p>
          <div style={{ display: "flex", gap: 12 }}>
            <button className="nv-btn nv-btn--primary">Install extension →</button>
            <button className="nv-btn" style={{ border: "1.5px solid rgba(255,255,255,0.3)", color: "#fff" }}>View on GitHub</button>
          </div>
        </div>
      </section>

      {/* Features grid */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 16, marginBottom: 40 }}>
        {[
          { t: "Live API Completion", d: "Full IntelliSense for 5,323 OneStream API classes inside .vb and .cs files.", g: "arch", c: "var(--nova-coral)" },
          { t: "Rule Deployment", d: "Push business rules to dev / QA / prod from the VS Code command palette.", g: "circle", c: "var(--nova-teal)" },
          { t: "MCP Server", d: "Exposes the OneStream metadata layer to Claude, Cursor, and Copilot.", g: "triangle", c: "var(--nova-ink)" },
          { t: "Cube View Preview", d: "Render cube views in a VS Code panel without leaving your editor.", g: "teardrop", c: "var(--nova-coral)" },
          { t: "Debug Console", d: "Attach to a running app server and step through rule execution.", g: "circle", c: "var(--nova-teal)" },
          { t: "Snippet Library", d: "All 827 business rules available as parameterized VS Code snippets.", g: "arch", c: "var(--nova-ink)" },
        ].map(f => (
          <div key={f.t} className="nv-card" style={{ padding: 28 }}>
            <BrandGlyph kind={f.g} size={40} color={f.c} />
            <h3 style={{ fontSize: 18, fontWeight: 700, margin: "24px 0 8px", letterSpacing: "-0.01em" }}>{f.t}</h3>
            <p style={{ fontSize: 14, color: "var(--color-text-2)", lineHeight: 1.55, margin: 0 }}>{f.d}</p>
          </div>
        ))}
      </section>

      {/* Install steps */}
      <section style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 32, marginBottom: 40 }}>
        <div>
          <p className="nv-eyebrow" style={{ marginBottom: 16 }}>GETTING STARTED</p>
          <h2 style={{ fontSize: 40, fontWeight: 700, letterSpacing: "-0.02em", margin: "0 0 24px" }}>Three steps to your first rule.</h2>
          {[
            { n: "01", t: "Install from Marketplace", d: "Search for 'OneStream' in the VS Code extensions panel." },
            { n: "02", t: "Connect your app server", d: "Authenticate via the command palette: OneStream: Connect." },
            { n: "03", t: "Start writing", d: "Open any .vb file and watch IntelliSense light up." },
          ].map(s => (
            <div key={s.n} style={{ display: "flex", gap: 20, padding: "20px 0", borderTop: "1px solid var(--nova-line)" }}>
              <div style={{ fontSize: 32, fontWeight: 700, color: "var(--nova-coral)", fontFamily: "var(--font-mono)", letterSpacing: "-0.02em" }}>{s.n}</div>
              <div>
                <h4 style={{ fontSize: 17, fontWeight: 700, margin: "0 0 4px" }}>{s.t}</h4>
                <p style={{ fontSize: 14, color: "var(--color-text-2)", margin: 0 }}>{s.d}</p>
              </div>
            </div>
          ))}
        </div>

        <div style={{
          background: "var(--nova-ink)",
          borderRadius: 20, overflow: "hidden",
          fontFamily: "var(--font-mono)", fontSize: 12.5,
          border: "1px solid var(--nova-line)",
        }}>
          <div style={{ background: "#1E2628", color: "#A7E8E4", padding: "10px 16px", fontSize: 11, display: "flex", gap: 8, alignItems: "center" }}>
            <span style={{ display: "flex", gap: 6 }}>
              <span style={{ width: 10, height: 10, borderRadius: 999, background: "#FF5948" }}/>
              <span style={{ width: 10, height: 10, borderRadius: 999, background: "#FFD57E" }}/>
              <span style={{ width: 10, height: 10, borderRadius: 999, background: "#A7E8E4" }}/>
            </span>
            <span style={{ marginLeft: 12, color: "#8A949B" }}>ConsolidationRule.vb · OneStream VS Code</span>
          </div>
          <pre style={{ color: "#E7F7F6", padding: 24, margin: 0, lineHeight: 1.7 }}>
{`1  ' Auto-completion from OneStream MCP
2  Function Main(si As SessionInfo, api)
3      Dim entity = api.Pov.Entity
4      If api.Entity.IsBaseEntity(entity) Then
5          api.Data.Calculate("A#Revenue * 1.05")
6      End If
7  End Function`}
          </pre>
        </div>
      </section>

      {/* MCP callout */}
      <section style={{
        background: "var(--nova-coral)", color: "#fff",
        borderRadius: 32, padding: 48,
        display: "grid", gridTemplateColumns: "1fr auto", gap: 40,
        alignItems: "center", position: "relative", overflow: "hidden"
      }}>
        <BrandGlyph kind="arch" size={260} color="rgba(255,255,255,0.18)" style={{ position: "absolute", right: -30, bottom: -30 }}/>
        <div style={{ position: "relative", zIndex: 1 }}>
          <p className="nv-eyebrow" style={{ color: "rgba(255,255,255,0.85)", marginBottom: 12 }}>MCP SERVER</p>
          <h2 style={{ fontSize: 40, letterSpacing: "-0.02em", fontWeight: 700, margin: "0 0 12px" }}>Give Claude the full API context.</h2>
          <p style={{ fontSize: 16, opacity: 0.9, margin: 0, maxWidth: 520, lineHeight: 1.55 }}>Run our MCP server alongside any AI assistant and it'll query live metadata, search business rules, and validate calls — no hallucinated method names.</p>
        </div>
        <button className="nv-btn" style={{ background: "#fff", color: "var(--nova-coral)", fontWeight: 700 }}>Read the docs →</button>
      </section>
    </div>
  );
}

window.ApiPage = ApiPage;
window.RulesPage = RulesPage;
window.ExtensionPage = ExtensionPage;
