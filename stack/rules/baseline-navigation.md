---
description: House baseline - code navigation and reading. Always-on (no paths), installer-managed - update overwrites local edits.
---

# Navigation and code reading

- Read only what's needed; before editing, read the body end-to-end and any function it depends on.
- Locate symbols, callers, and resolved types with `serena` - inline, never delegated to `Explore` / `general-purpose`; reserve those for genuinely broad multi-file sweeps. An installed `LSP` plugin adds compiler-exact lookups and inline diagnostics for its language.
- Never fetch what is already in context: no repeat `find_symbol` for a symbol fetched this session, no fresh whole-file `Read` of the file under edit - re-check an edit at the edited range only.
- Ambiguous reference with multiple matches: list the matches, ask. Do not guess.
- Pasted code in chat is illustrative unless stated otherwise; confirm the target file before editing.
