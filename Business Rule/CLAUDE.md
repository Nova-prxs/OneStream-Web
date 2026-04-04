<!-- GSD:project-start source:PROJECT.md -->
## Project

**OneStream AI Training Dataset**

A comprehensive dataset generation project that analyzes existing OneStream business rules (780+ VB.NET files across Extender, Assemblies, Connector, DashboardStringFunction, and Finance categories) and generates new rules using the OneStream MCP to create a 10,000+ example fine-tuning dataset. The dataset will enable an open-source LLM to become a specialist in writing OneStream business rules using native APIs.

**Core Value:** The dataset must produce examples that, when used for fine-tuning, result in a model that generates valid, compilable OneStream business rules.

### Constraints

- **Data source**: All new rules must be generated using valid OneStream APIs as documented via the MCP
- **Quality**: Every example must have a natural language prompt, complete functional code, and metadata tags
- **Scale**: Minimum 10,000 examples to be effective for fine-tuning
- **Format**: Must support both JSONL and chat multi-turn export formats
- **Language**: Rules are in VB.NET; prompts can be in English or Spanish
<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->
## Technology Stack

## Recommended Stack
### Core Language and Runtime
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Python | 3.11+ | All pipeline scripts | Widest ecosystem support for data/ML tooling; 3.11 has significant performance improvements over 3.10; 3.12+ has minor compatibility issues with some ML libs as of early 2026. Avoid 3.13. |
### Dataset Format and I/O
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `jsonlines` | 4.0.0 | Read/write JSONL files | Thin, stable wrapper over stdlib json; handles encoding edge cases cleanly; production-stable since 2023. Use over raw `json` + file iteration to avoid common newline/encoding bugs. |
| Python stdlib `json` | built-in | Inline JSON operations within scripts | No dependency needed for simple dict-to-string serialization. |
| Python stdlib `pathlib` | built-in | File discovery across 780+ VB.NET files | `Path.rglob("*.vb")` handles recursive traversal of Extender/, Assemblies/, Connector/ etc. cleanly without external dependencies. |
### Dataset Schema Validation
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `pydantic` | 2.12.5 | Schema enforcement for dataset examples | Defines and validates the shape of each training example (prompt, code, metadata) at generation time, not at export time. Catches malformed examples before they pollute the dataset. v2 is 5-17x faster than v1 due to Rust core — critical when validating 10,000+ records. |
### Data Processing and Manipulation
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `pandas` | 2.2.x | Tabular analysis of API coverage gaps, statistics, deduplication joins | At 10,000–50,000 examples, pandas is more than sufficient — the performance difference vs Polars only materializes at 1M+ rows. The ecosystem integration (to_json, groupby, value_counts) directly maps to coverage analysis tasks (which rule types are underrepresented?). Polars is faster but adds syntax learning overhead for no benefit at this scale. |
### Progress and Logging
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `tqdm` | 4.67.x | Progress bars for file ingestion, generation loops | Standard for Python data pipelines; adds one line per loop, works in Jupyter and CLI. Critical UX when processing 780 files or generating 10,000 examples — you need visibility into how far along a run is. |
| Python stdlib `logging` | built-in | Structured logs for pipeline stages | No dependency needed; use `logging.getLogger(__name__)` pattern per module rather than print statements. |
### VB.NET Code Analysis
#### Primary Approach: Regex + stdlib (Recommended for this project)
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Python stdlib `re` | built-in | Extract function signatures, Sub/Function names, API call patterns, imports | The 780 existing rules have consistent structure — OneStream business rules follow predictable patterns (`BRApi.`, parameter declarations, `Sub Main`). Regex is sufficient to extract what matters for cataloging and metadata. No dependency, no compilation step, no tool installation. |
# Extract BRApi calls
# Extract Sub/Function names
# Extract parameter types
# Detect rule type from file path
# e.g., Path("Extender/SomeName.vb").parts[0] -> "Extender"
#### Secondary Approach: Tree-sitter (Use if regex proves insufficient)
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `tree-sitter` | 0.25.x (py-tree-sitter) | Full AST parsing of VB.NET | Only needed if regex proves insufficient — e.g., if you need to distinguish nested function calls or parse complex inheritance. A community VB.NET grammar exists at `CodeAnt-AI/tree-sitter-vb-dotnet` but its maturity and coverage are **LOW confidence** — verify before relying on it. |
#### Do NOT Use: Roslyn / pythonnet
- `pythonnet` (CLR interop from Python) — adds .NET runtime dependency, complex setup, slow startup
- A subprocess calling a compiled .NET tool — adds build complexity
### Dataset Construction (Chat Format)
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `datasets` (HuggingFace) | 4.8.4 | Final dataset packaging, HF Hub upload, format conversion | The standard for distributing fine-tuning datasets. `datasets.Dataset.from_list()` converts Python dicts directly. Supports push_to_hub for sharing the final dataset. Required by downstream fine-tuning toolchains (TRL, LLaMA-Factory, Unsloth). |
### Deduplication
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Python stdlib `hashlib` | built-in | Exact deduplication via SHA-256 hash of `(prompt, completion)` tuples | Free, fast, no dependency. Exact deduplication is the first and most important pass. |
| `datasketch` | 1.9.0 | Near-duplicate detection via MinHash LSH | Use for detecting cases where generated variations are too similar to existing examples. Operates on token n-grams. Prevents the model from memorizing near-identical examples. Only needed in the generation phase when producing 10,000+ examples. |
## Export Format Specifications
### Format 1: JSONL Prompt/Completion
- Prompt ends with `\n\n###\n` (stop sequence separator — prevents the model from confusing prompt and completion)
- Completion ends with `\n\n###` (stop sequence — tells the model when to stop)
- No comments inside the VB.NET code (project decision)
### Format 2: Chat Multi-Turn (Messages Array)
- System message is consistent across ALL examples (defines the model's persona and constraints)
- One user turn per example (single-turn instruction format — simpler to generate, sufficient for this use case)
- Multi-turn (conversation chains) are optional and harder to generate consistently — defer unless the dataset needs conversational debugging examples
- Use `"messages"` key, not `"conversations"` — the former is the HuggingFace/TRL standard; the latter is the older ShareGPT format and requires extra normalization steps in LLaMA-Factory
### Metadata Sidecar (JSONL)
## Alternatives Considered
| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| DataFrame | pandas | polars | Polars performance advantage is irrelevant at <100k rows; pandas ecosystem compatibility is superior for this workload |
| VB.NET parsing | regex (re) | Roslyn via pythonnet | Roslyn requires .NET runtime + complex interop; overkill for pattern extraction from a known, consistent codebase |
| VB.NET parsing | regex (re) | tree-sitter vb-dotnet | Community grammar with LOW confidence coverage of OneStream-specific constructs |
| Dataset packaging | HuggingFace datasets | plain JSONL files only | datasets library enables HF Hub publishing and downstream fine-tuning tool compatibility with minimal extra work |
| Chat format | messages array | ShareGPT conversations | ShareGPT requires normalization; messages array is the current HuggingFace/TRL standard directly |
| Dedup | hashlib + datasketch | semhash (embedding-based) | Embedding-based dedup is slower and requires a model; for code, token-level MinHash is sufficient and interpretable |
| Schema validation | pydantic v2 | marshmallow / cerberus | Pydantic v2 is 5-17x faster, uses native Python type hints, and is the current ecosystem standard |
## Complete Installation
# Create environment
# Core pipeline
# Deduplication
# Optional: tree-sitter (only if regex proves insufficient)
# pip install tree-sitter==0.25.2
# pip install git+https://github.com/CodeAnt-AI/tree-sitter-vb-dotnet
## OneStream MCP Integration Notes
- Reading existing rules via the MCP's file/workspace access tools
- Browsing OneStream API documentation
- Generating new rules using MCP tool calls
## What NOT to Use
| Technology | Why Not |
|------------|---------|
| LangChain | Unnecessary abstraction for a batch processing pipeline; adds significant dependency weight and version fragility for no benefit here |
| Spark / Dask | Distributed computing for a single-machine pipeline generating ~10k records is over-engineering |
| SQLite / PostgreSQL | A dataset of 10k JSONL records does not need a database; flat files with pandas for analysis is simpler and more portable |
| Poetry / conda | Use venv + pip for reproducibility; Poetry adds complexity without benefit for a single-developer data pipeline |
| Jupyter notebooks for the pipeline | Use notebooks for exploration/analysis only; production pipeline scripts should be `.py` files for repeatability and version control |
| tree-sitter VB.NET grammar | LOW confidence in grammar completeness; start with regex, escalate only if needed |
## Sources
- HuggingFace datasets PyPI page — version 4.8.4: https://pypi.org/project/datasets/
- Pydantic PyPI page — version 2.12.5: https://pypi.org/project/pydantic/
- jsonlines PyPI page — version 4.0.0: https://pypi.org/project/jsonlines/
- HuggingFace 2025 fine-tuning guide (Phil Schmid): https://www.philschmid.de/fine-tune-llms-in-2025
- HuggingFace chat templates documentation: https://huggingface.co/docs/transformers/chat_templating
- HuggingFace Code LLM fine-tuning cookbook: https://huggingface.co/learn/cookbook/fine_tuning_code_llm_on_single_gpu
- tree-sitter VB.NET discussion: https://github.com/tree-sitter/tree-sitter/discussions/4530
- datasketch PyPI (v1.9.0): https://pypi.org/project/datasketch/
- BigCode near-deduplication reference: https://github.com/bigcode-project/bigcode-dataset/blob/main/near_deduplication/minhash_deduplication.py
- Unsloth chat templates guide: https://unsloth.ai/docs/basics/chat-templates
- LLaMA-Factory multi-format dataset guide: https://github.com/hiyouga/LlamaFactory
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
