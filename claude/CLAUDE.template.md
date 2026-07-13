# CLAUDE.md (stack-neutral template)

<!-- Fill-in block - delete once done. Copy this file into a new project as CLAUDE.md, then:
replace every <placeholder> with what the project actually has (inspect first: .claude/skills/,
.mcp.json, claude plugin list); trim the ## Rules table to what the installer actually laid
down; delete any section that does not apply; add the project top per ## Per-project additions.
This file auto-injects every session and into subagents - keep it lean and route work by an
observable trigger (an artifact, a command, a checkpoint). The cross-project working conventions
are NOT here: they load from the always-on baseline rules in .claude/rules/ (installer-managed,
refreshed on update) - never restate them in this file. (HTML comment: stripped from injection,
so an unfilled template pays nothing for this block.) -->

## Rules

The always-on baseline set in `.claude/rules/` - one concern per file, all loaded every session;
this table maps where each behavior rule lives (the detail is in the rules, not here). Path-scoped
rules in the same directory attach on a matching file touch - their own `paths:` frontmatter says when.

| Baseline rule | What it governs |
|---|---|
| baseline-interaction | communication style, adversarial review of user proposals, planning/execution thresholds |
| baseline-quality-gates | code-quality bars and the done-claim verification gate |
| baseline-security | /security-review routing, PII/secret handling, the permissions.deny caveat |
| baseline-git | commits, branches, PRs, push discipline, the pre-commit checkpoint |
| baseline-navigation | symbol-lookup and code-reading discipline |
| baseline-project-capabilities (GENERATED - run /project-capabilities after install or a trim) | the usage policy plus this project's real skill / seat / MCP inventory |

Two more generated always-on rules land when their captures run: `baseline-project-architecture.md`
(/project-architecture-analyzer) and `baseline-project-related-context.md` (/project-related-context).

## Per-project additions

A project's `CLAUDE.md` is this base plus a project-specific top, in two groups (each section
lean; interleave as reads best - the project intro usually comes first):

**Project - what it is:**

1. **What this project is** - one paragraph: domain, shape (binary / service / library), persistence, surfaces.
2. **Architecture** - layers / modules, dependency rules, folder organization.
3. **Key patterns** - the non-obvious in-house patterns a newcomer would trip on.
4. **Operational notes** - runtime constraints and gotchas that shape code decisions.
5. **Cross-cutting checklists** - for each change that must move several files in lockstep, the full touch-point list.

**Stack - what it is built with:**

6. **Stack** - languages, frameworks, key libraries, test stack + coverage gate, the LSP plugin
   for the primary language(s). MCP routing is NOT hand-filled here - it lives in the generated
   `baseline-project-capabilities.md` (run `/project-capabilities`).
7. **Commands** - copy-pasteable build / test / run / migrate / publish, with any environment quirks.
8. **Code conventions** - the house-style skill for each file type (auto-attached by the path-scoped rules above).
9. **Testing approach** - per-layer strategy, what's excluded, the integration / regression net.
10. **Load by artifact** - a table mapping this repo's concrete files / types / constructs to the third-party skills it can't re-describe (house-style skills self-fire, so they're not in it).
