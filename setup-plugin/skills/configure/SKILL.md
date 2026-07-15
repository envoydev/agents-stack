---
name: configure
description: "UPDATE an existing claude-stack install - inventory what is actually installed, let the user adjust the selection (refresh as-is, add stacks/items, or drop items), close it against the dependency graph with a prerequisite check, and run the installer's update action against that subset. Same selection machinery as the sibling setup skill, applied to an install that already exists. Trigger by invoking /claude-stack:configure or 'update the claude stack here'. NOT for a first install - that is the sibling setup skill (install from scratch); routes there when nothing is installed yet."
disable-model-invocation: true
---

# Configure the Claude stack - update an existing install

You are refreshing or adjusting a claude-stack install that already exists. Same discipline as
`setup`: drive it interactively, always show the resolved selection and the prerequisite report
before running, never run past an unmet blocker. `stack-select.js` does the deterministic work;
you orchestrate. The one difference from `setup`: the baseline selection is what is INSTALLED,
not the recommendations - and the action is `update`, not `install`.

Everything is fetched from `https://raw.githubusercontent.com/envoydev/claude-stack/main`. Use a
temp working dir (e.g. `mktemp -d`) for the fetched tools; never write them into the project.

## 1. Preconditions - find the install
- Project mode: cwd is a project root with a populated `.claude/` (skills/agents/rules dirs, or
  `.mcp.json`). Global mode: no project here, but the account (`~/.claude`, or `~/.claude-<space>`)
  carries installed skills. Nothing installed in either place -> stop and route to the sibling
  `setup` skill; there is nothing to configure yet.
- OS: on `darwin`/`linux` use the sh installer; on Windows the ps1 (via `pwsh`).

## 2. Inventory the installed set
Build the CURRENT selection from disk - never from memory or assumption:
- skills: the directory names under `.claude/skills/` (or the account's `skills/`).
- agents: `.claude/agents/*.md`; rules: `.claude/rules/*.md` (exclude the GENERATED
  `baseline-project-*.md` awareness rules - they are written by capture skills, never installed).
- mcps: the server names in `<repo>/.mcp.json`; plugins: `claude plugin list` (fail-soft without
  the CLI).
Show the inventory grouped by category, with counts.

## 3. Ask what to change
One question: **refresh as-is** (default - update everything currently installed), **add**
(multi-pick: a whole stack seeded from the sibling `setup` skill's
`references/recommendations.json`, or named individual items), or **drop** (pick installed items
to remove). Also ask: keep local model/effort pins? (`--keep-pins`, default yes for a configure
run - an existing install often carries deliberate pin edits).

## 4. Fetch the tools
Into the temp dir, download the right installer (`scripts/claude-stack.sh` or
`scripts/claude-stack.ps1`), plus `scripts/stack-select.js`, `scripts/stack-graph.json`, and
`templates/CLAUDE.template.md` (for step 9).

## 5. Build the selection and close it
- Selection = installed set, plus the adds, minus the drops; write it to `raw.json`.
- Run: `node stack-select.js --selection raw.json --graph stack-graph.json --emit selection.txt --check`
- A drop that something kept still depends on comes back as a `required:` line - show the reason
  and let the user keep it or also drop the dependents.

## 6. Review, prerequisite gate
Same contract as `setup`: show the closed selection grouped by category, closure adds marked with
their reasons; list blockers with fixes and never run past one; warnings are listed and passed.

## 7. Run the update
- Unix: `bash claude-stack.sh update --scope <scope> --selection selection.txt [--space <name>] [--keep-pins]`
- Windows: `pwsh -File claude-stack.ps1 update -Scope <scope> -Selection selection.txt [-Space <name>] [-KeepPins]`
- Scope/space mirror how the install was laid down (project install -> `project`; account
  install -> `global`, with the space that owns it) - ask only when it is genuinely ambiguous.

## 8. Apply the drops
`update --selection` refreshes the selected set - it does NOT uninstall what was dropped. Remove
dropped items explicitly, show each command before running it: delete the skill directory /
agent file / rule file; `claude mcp remove <name>` for an MCP; `claude plugin uninstall <name>`
for a plugin. Then re-run `/project-capabilities` (when installed) so the generated awareness
rule reflects the new inventory.

## 9. Reconcile the project's CLAUDE.md with the template (project mode)
Reconcile the project's CLAUDE.md against the fetched `templates/CLAUDE.template.md`: add the
sections the template gained since the install, update the selection-tied parts - the rules
table and any capability mentions - for what this run added or dropped, and fill any
still-empty `<placeholders>` from what the inventory established. Reconcile ADDITIVELY: never
overwrite the project's own prose, and show the changes before writing. Skip in global mode
(no project file to reconcile).

## 10. Post-check
Report what changed per category (refreshed / added / dropped), the CLAUDE.md reconcile result,
anything deferred, and remind that a restart picks up MCP registration changes.

## Do not
- Do not fall back to a full re-install - this is the update path; a from-scratch install is the
  sibling `setup` skill. Do not skip the review or the prerequisite gate. Do not write fetched
  tools into the project tree. Do not commit anything on the user's behalf.
