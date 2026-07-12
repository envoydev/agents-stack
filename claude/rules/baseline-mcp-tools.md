---
description: House baseline - MCP server and tool routing. Always-on (no paths), installer-managed - update overwrites local edits.
---

# MCP servers and tools

| Server | Use for |
|---|---|
| `serena` | default symbol navigator + symbol-level editor - `find_symbol` / `find_referencing_symbols` before any whole-file Read; self-activates on launch (never call `activate_project`). Also holds the per-project memory (`.serena/memories/`) the agent flows use for hand-off notes. |
| `context7` | up-to-date docs for any API you don't own. Before writing or changing hand-written code against a third-party package, vendor SDK, or version-sensitive framework surface: resolve + query first; never answer library-API questions from recall. Generated code doesn't count. |
| `memory` | *cross-project* recall only - search when this project's context is thin; store a significant cross-project outcome at task end (decision / gotcha / architecture, + project & date). Per-project hand-off lives in serena, not here. |
| `playwright` | drive a browser for visual checks / large HTML reports - don't text-read them |
| framework CLI (`angular-cli` in the Angular baseline) | the framework CLI's own docs / commands - registered only in matching projects |
| Issue-tracker connector | the project's tracker read-write; ticket skills write the content, the connector files it - always confirm before filing |

Routing applies only to servers actually registered in the project's `.mcp.json` - a standalone
project drops `memory`; `chrome-devtools` and `appium-mcp` are registered only for browser /
mobile targets. LSP plugins (`csharp-lsp`, `typescript-lsp`) feed the `LSP` tool for the project's
language(s). Project-added MCPs and plugins are named in the project's `CLAUDE.md`.
