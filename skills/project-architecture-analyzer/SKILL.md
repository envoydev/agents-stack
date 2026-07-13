---
name: project-architecture-analyzer
description: "The deliberate architecture capture: dispatch code-analyzer per module (the cheap read-only characterizer), reason over the returned digests IN-SESSION - declared vs enforced structure, the stack-keyed hazards, strengths and weaknesses - and write docs/architecture/ARCHITECTURE.md (lean core map + its references/ deep-dives) and docs/architecture/ASSESSMENT.md (10 reasoned strengths + 10 tiered weaknesses). The architecture judgment runs in the main session, and this skill's frontmatter pins its turn to opus/xhigh - a lighter session still analyzes on Opus; there is no dispatched architecture seat. Re-run to refresh: the same analysis, the docs reconciled in place, user-authored references/ files never touched. Manual, /-only. Triggers on 'capture the architecture' or 'refresh the architecture docs'. NOT for fixing the weaknesses it finds (architecture-quality-loop - it runs this capture as its first step), one module's characterization (@agent-code-analyzer), or code style (project-code-style-analyzer)."
disable-model-invocation: true
model: opus
effort: xhigh
---

# Project Architecture Analyzer - Capture the Architecture (Deliberate)

You are the architect seat for this run: you build the project's architecture picture by reasoning over cheap digests, and you record it as the two committed docs the whole stack orients from - `docs/architecture/ARCHITECTURE.md` (the neutral structure map, deep-dives under `docs/architecture/references/`) and `docs/architecture/ASSESSMENT.md` (the reasoned, tiered evaluation). The reading is delegated - code-analyzer characterizes one module per dispatch and returns a compact digest - but the judgment is NOT: you aggregate, reconcile, and evaluate in-session, then write the docs yourself. Architecture judgment is the expensive kind, so the frontmatter pins this skill's turn to `opus`/`xhigh` (the deleted architecture seat's old pin, preserved). The pin lasts the invoking turn only - a run that pauses for user input resumes on the session model - so a run you expect to interrupt still wants an Opus session.

This is capture only: it documents and evaluates, it fixes nothing and produces no implementation steps. Working the weaknesses is `architecture-quality-loop`, which runs this capture as its ANALYZE step and routes fixes by tier. The per-change fit verdict (extend / refactor first / isolate) is the domain solution-designers', reading the map this skill writes.

Read `references/doc-shapes.md` (the two docs' required shape) and `references/hazards.md` (the stack-keyed hazard catalog you hunt) before AGGREGATE - they are this skill's contract, not suggestions.

## Execution modes
DELEGATED vs INLINE - the shared policy `cross-stack-agents-flow` owns. Pick once, hold for the run:

- **DELEGATED** (dispatch available) - dispatch code-analyzer per module as below; reasoning and writing stay here.
- **INLINE** (no dispatch: Cursor) - characterize the modules yourself, serena-first and bounded (a module inventory pass, then located reads - never whole-file slurps), and continue at AGGREGATE identically.

## The run

### 1. ORIENT
Read `docs/architecture/ARCHITECTURE.md` and `docs/architecture/ASSESSMENT.md` if they exist - a claim to verify, not ground truth - and note which `docs/architecture/references/` files are yours versus user-authored. Read the config/manifest files for framework and package facts. Build the module inventory (`get_symbols_overview` / directory listing) - the list of areas to characterize. Scope it if the user did (one bounded context or module subtree on a large codebase); whole project otherwise.

### 2. GATHER - code-analyzer per module, in parallel
Dispatch code-analyzer per module/topic on the inventory - in a single message where the areas are independent. Each returns a compact digest (purpose, public surface, inbound/outbound dependencies, patterns, smells), every claim tied to a located symbol. You are the expensive seat: never read the codebase wholesale yourself - serena (`get_symbols_overview`, `find_symbol` / `find_referencing_symbols`) and `Read` are for light orientation and spot-verification of one edge only.

### 3. AGGREGATE + REASON - in-session
Load the vocabulary you reason with: the domain router (`dotnet`, `frontend`, or `mobile`) for the area's house conventions, `dotnet-architecture` for the architecture-style vocabulary the map names, `csharp-design-patterns` when judging pattern fit in .NET, `dotnet-architecture-tests` when judging whether a .NET boundary is guarded by a fitness test or only by convention, and the convention skill for a file type you must judge in depth. Assemble the digests into the structure - layers, dependency directions, patterns, boundaries - and reconcile against the existing docs: declared vs enforced part company exactly where coupling escapes the static graph (DI registrations, reflection and service location, string-keyed lookups, events and messaging, DTO/entity types reused across a boundary) - treat a name as a hypothesis, an edge as proven only from a usage. Hunt the `references/hazards.md` catalog for the stack in play. Reason out the strengths and weaknesses.

### 4. RE-GATHER on the gaps
Where a part is unclear, uncovered, or one digest conflicts with another, dispatch code-analyzer again on exactly that topic (or spot-verify one edge yourself with serena). **Hard cap: 3 gather rounds.** Still unsettled after 3: write what is established, mark what is uncertain and what would settle it - never guess to fill a section.

### 5. WRITE - the two docs, per references/doc-shapes.md
Write `docs/architecture/ARCHITECTURE.md` (lean core map, deep-dives spilled to `docs/architecture/references/<topic>.md` files linked from a short index) and `docs/architecture/ASSESSMENT.md` (10 strengths + 10 weaknesses, remediation and tier per weakness, summary) - clean, scannable Markdown per the `markdown-style` skill. Create `docs/architecture/` and `docs/architecture/references/` only when absent. The `references/` folder is shared with human-authored docs: rewrite only the topic files you author, link the user-authored ones from the index, and never delete a file. Re-run: reconcile in place - correct what drifted, add what is new, drop what is gone. Write ONLY under `docs/architecture/` - never source, never another doc.

### 6. REPORT
Confirm the files written (created vs refreshed, sections touched). Then lean: gather rounds used and whether the picture settled within the cap; the structure headline; the assessment's shape - strength/weakness counts, tier tally, the top few highest-leverage fixes `architecture-quality-loop` should take first; anything unverified and what would settle it. The docs are committed files - they ship with the repo. The map is what the domain solution-designers read to judge where a change fits, what `cross-stack-agents-flow` reads to pick a cross-domain run, and what cross-stack-contract-designer freezes a seam from. No re-paste of the doc bodies - point to the files.

## Don't game it
Record the structure that exists, not the one the names imply - in both docs, every claim traces to code you or a code-analyzer located, and anything unverified is marked unverified, never rounded up to certainty. When a digest looks too clean for the domain, dispatch again or spot-check one edge rather than trust it. In the assessment, an honest weakness beats a flattering omission, and a deliberate tradeoff is labelled a tradeoff, not a defect. Never pad to ten strengths or weaknesses when the codebase is too small to support them - say so instead.
