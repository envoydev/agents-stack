# Personal coding-agent skills & stacks

A collection of personal agent skills - reusable coding conventions and small utilities - plus
the **Claude Code stack installer** at the repo root (skills + MCP servers + hooks/rules/agents +
plugins). Skills install via a git-clone-and-copy step built into the installers. The **Cursor**
twin stack lives in its own repo, [`cursor-stack`](https://github.com/envoydev/cursor-stack) -
its installers clone THIS repo for the shared skills, so the skill/MCP baseline stays
single-sourced here.

## Installation

Install just the skills into the current project (no MCP servers, hooks, rules, or plugins):

```bash
mkdir -p .claude && curl -fsSL https://raw.githubusercontent.com/envoydev/claude-stack/main/scripts/claude-stack.sh -o .claude/claude-stack.sh
bash .claude/claude-stack.sh install --skills-only   # Claude Code -> .claude/skills/
```

For Cursor, use the installer from [`cursor-stack`](https://github.com/envoydev/cursor-stack) -
it clones this repo for the same skills.

Add `--scope global` to install into the account-wide `~/.claude/skills/`
instead of the project. The agent picks up the `SKILL.md` files automatically the next session.

**Want the whole stack** (skills + MCP servers + hooks/rules/agents + plugins), not just the
skills? Use the stack installer - see [The full stack](#the-full-stack) below.

## Bootstrapping a project with the setup plugin

Install the setup plugin once, then run it inside any project to install a curated subset of the stack:

```
claude plugin marketplace add envoydev/claude-stack
claude plugin install claude-stack@claude-stack
```

Then run `/claude-stack:setup` for a fresh install (in a project it decides the selection FROM the project; outside one it offers a global install seeded from the recommended set) or `/claude-stack:configure` to update an existing install (refresh as-is, add stacks/items, or drop them). Both detect the OS, check prerequisites, and let you review a dependency-complete selection before running the installer against just that subset; `/claude-stack` alone routes by state. The installer scripts and `stack-select.js` remain the source of truth; the plugin is the guided front end.

## Managing installed skills

```bash
bash .claude/claude-stack.sh update --skills-only   # refresh every skill to latest
```

Skills are plain directories under `.claude/skills/` - `ls` the directory to
see what's installed, delete a folder to drop a skill.

## The full stack

Two twin scripts install (or update) the **complete Claude Code stack into a project** -
every skill, plugin, MCP server, and hook from the curated inventory in `claude-stack.html`.
Built-in/system CLI skills are excluded (they ship with the CLI). The agent is the script you
run, so there is **no agent argument**.

> The subagent roster - the per-domain specialist team and the cross-cutting agents - is laid out
> visually at the top of [`docs/claude-stack.html`](docs/claude-stack.html), above the full stack inventory it now folds in.

| Script             | System        | Shell                                   |
| ------------------ | ------------- | --------------------------------------- |
| `claude-stack.sh`  | macOS / Linux | `bash`                                  |
| `claude-stack.ps1` | Windows       | PowerShell 5.1 (Desktop) or 7+ (`pwsh`) |

The `.sh`/`.ps1` twins take the **same arguments** and produce the **same result**;
everything below applies to both unless a row is marked otherwise.

> The scripts are the **single source of truth** for the stack. `SKILLS` and `MCPS` are shared
> with the Cursor stack (the `cursor-stack` repo clones this one for skills - mirror a baseline
> change there in the same sitting); `PLUGINS` are Claude-only. To trim or
> extend, comment/uncomment entries in the manifest blocks near the top, then re-run. After
> editing, run the repo lint (`npm run lint` from the repo root) - it verifies both manifests
> agree (and match the HTML).

---

## What gets installed

| Component | Count | Notes |
| --------- | ----- | ----- |
| **Skills**  | 65 | conventions + utilities (ticket writers, C#/.NET, Angular/TS, SQL, `project-task-flow` orchestration + routing (single-stack trios + cross-domain), `project-quality-loop` + `project-architecture-quality-loop` review loops) - git-clone + copy from `envoydev/claude-stack` (`claude-stack.sh install --skills-only`, or the claude-stack plugin) |
| **Plugins** | 7  | `superpowers`, `claude-md-management`, `csharp-lsp`, `typescript-lsp`, `security-guidance`, `claude-hud`, `ponytail` - `claude plugin install` (needs the `claude` CLI) |
| **MCP servers** | 8 | `angular-cli`, `serena`, `playwright`, `memory`, `context7`, plus `chrome-devtools` + `appium-mcp` (heavy - active; comment out where not needed) and `sentry` (error monitoring - hosted remote MCP, needs `SENTRY_ACCESS_TOKEN` in settings `env`; comment out without Sentry). `memory` is cross-project recall (the subagent handoff runs on serena) - comment it out in a standalone project → `<repo>/.mcp.json` |
| **Hooks** | 4 | `guard-protected-force-push` (blocks force-push to main/master/develop) + `guard-catastrophic-rm` (blocks recursive rm of /, ~, $HOME, or a bare *) + `guard-read-whole-file` (PreToolUse Read - blocks a whole-file Read of a >100-line source file, locate via serena first) → `.claude/hooks/` + wired into `.claude/settings.json`; plus `instrument-tool-usage` fetched UNWIRED (opt-in per-run stats - see the instrumentation section) |
| **Agents** | 33 | 4 resolvers - .NET (`dotnet-build-error-resolver`, `dotnet-test-failure-resolver`) + Angular (`ng-build-error-resolver`, `angular-test-resolver`): implement-phase build/test fix loops, serena-driven, iteration-capped, no reward-hacking, pinned sonnet/high - plus 4 cross-cutting agents (`ci-failure-diagnoser`, `issue-diagnoser`, `security-auditor`, `integration-reviewer` - the last the mandatory cross-domain final gate before commit, all read-only, pinned opus/xhigh - bar ci-failure-diagnoser at opus/high and integration-reviewer at sonnet/xhigh) - plus 21 per-domain seats, a 3-agent vertical repeated across 7 stacks (ASP.NET, Angular, WPF, console, mobile, data, DevOps - the three C# verticals split by surface: web/API, WPF desktop, and console the headless Generic-Host worker/bot/daemon/CLI): `<stack>-solution-designer` (opus/xhigh - decomposes into parallel tasks) → `<stack>-implementer` (sonnet/medium - builds one task, code + tests) → `<stack>-verifier` (sonnet/xhigh - gates the build vs plan + quality, loops back) - plus two read-only sonnet/low support seats: `evidence-gatherer` (the two diagnosers dispatch it to reproduce and pull logs) and `code-analyzer` (the `project-architecture-analyzer` capture fans it out to characterize modules - purpose, surface, deps, patterns, smells), each keeping the read volume off the opus seat; plus `code-style-analyzer` (sonnet/medium - the read-only per-language style characterizer the `project-code-style-analyzer` skill fans out in parallel, merging the reports into `docs/PROJECT-CODE-STYLE.md` + the inject-code-style hook's extension filter) and `related-project-analyzer` (sonnet/medium - the read-only sibling-repo characterizer the `project-related-context` skill fans out per related path/URL, merging the YAML entries into `docs/PROJECT-RELATED-CONTEXT.md`); every seat bar `evidence-gatherer`, `code-analyzer`, `code-style-analyzer` and `related-project-analyzer` also uses serena's per-project memory as the hand-off bus (read a note named `<feature>__<contract_version>__<seat>` at start, write one at hand-off); the committed architecture docs - a lean `docs/architecture/ARCHITECTURE.md` core map plus deep-dive files under `docs/architecture/references/`, the durable project-architecture map the designers build against - are owned by the `project-architecture-analyzer` skill, reasoning in the main session over `code-analyzer` digests → `.claude/agents/` |
| **Rules** | 15 | five always-on `baseline-*` rules (no `paths:` - the cross-project working conventions grouped by exclusion affinity: interaction (communication + proposal review + planning), quality-gates (code quality + definition of done), security, git + pre-commit, navigation; the skill/agent usage policy, the per-project capability inventory, MCP routing, related-projects and architecture awareness are per-project GENERATED rules - baseline-project-capabilities.md / baseline-project-related-context.md / baseline-project-architecture.md, written by their capture skills, never in this manifest; loaded every session and subagent like CLAUDE.md but refreshed on `update`, individually excludable) + ten path-scoped: markdown authoring, the two repair-loop routers, and seven single-job convention rules (typescript, angular, angular-styling, csharp, wpf, sql, devops - each glob-attaches ONE file family to its house-style skill, so a stack a project lacks is simply not installed) → `.claude/rules/` |
| **Project instructions** | 1 | `install` seeds `CLAUDE.md` from `CLAUDE.template.md` when the project has none (never clobbers a filled one). Per-project only - the working conventions live in the `house-baseline` rule; fill the template's `<placeholders>` (stack inventories, secret/config globs, related projects). Committed, not gitignored → `<repo>/CLAUDE.md` |

> **Two runtime notes.** `CLAUDE_CODE_SUBAGENT_MODEL` silently overrides every agent's frontmatter
> model pin - leave it unset. The generated `.claude/settings.json` pre-approves the project
> `.mcp.json` via an `enabledMcpjsonServers` allow-list naming exactly the registered servers (never
> a blanket `enableAllProjectMcpServers`); `chrome-devtools` and `appium-mcp` are in that list but
> fail at launch without their native deps (a browser; Xcode / Android SDK + Java) - which is why
> they are the comment-out pair.

### Install cadence - keep always vs install on occasion

Cost differs by artifact, so the keep-or-skip call does too:

- **Skills** - permanent by default: keyword-gated and ~free when idle, so install all and let them self-gate. Whole-domain sets (the Ionic/Capacitor `mobile` group, `dotnet-wpf`) are optional only if you never touch that domain.
- **MCPs** - real launch cost, so split: baseline `context7` / `serena` / `memory` / `playwright`; domain-gated `angular-cli` (Angular projects only); opt-in `chrome-devtools` and `appium-mcp` (heavy native deps - left commented unless needed). `memory` is cross-project recall (the subagent handoff runs on serena) - comment it out in a standalone project.
- **Plugins** - permanent (language-agnostic): `superpowers`, `claude-md-management`, `security-guidance`, `claude-hud`, `ponytail`. Language-gated: the `*-lsp` pair - install the ones matching the project's languages (`typescript-lsp` + `csharp-lsp` for an Angular + .NET shop). (Design-taste guidance for distinctive UI now lives in-house in the `frontend` skill, not a separate plugin.)

---

## Prerequisites

The script runs a **prerequisites check first and warns (never fails)** on anything missing -
install what you need for the work you actually do, then re-run.

| Tool | Needed for | Required? | Install (macOS/Linux) | Install (Windows) |
| ---- | ---------- | --------- | --------------------- | ----------------- |
| **node** ≥ 22.12 LTS | Claude Code, hooks, all `npx` MCPs | **Yes** | `brew install node` / nvm | `winget install OpenJS.NodeJS.LTS` |
| **npx** | `npx` MCPs (e.g. context7 local transport; ships with node) | **Yes** | (with node) | (with node) |
| **claude** CLI | plugins, MCP registration, hook wiring | **Yes** | `npm i -g @anthropic-ai/claude-code` | `winget install Anthropic.ClaudeCode` |
| **git** | project-scope path resolution (repo root) | **Yes** for project scope | `brew install git` | `winget install Git.Git` |
| **uvx** (uv) | `serena` + `memory` MCPs | for those MCPs | `curl -LsSf https://astral.sh/uv/install.sh \| sh` | `powershell -ExecutionPolicy Bypass -c "irm https://astral.sh/uv/install.ps1 \| iex"` |
| **python3** | `security-guidance` plugin hook; settings.json merge (**bash only** - PowerShell merges natively) | for that plugin | `brew install python` | `winget install Python.Python.3.12` (the Windows Store stub does **not** count) |
| **typescript-language-server** | `typescript-lsp` plugin (TS/JS work) | optional | `npm i -g typescript-language-server typescript` | same |
| **csharp-ls** | `csharp-lsp` plugin (C# work only) | optional | `dotnet tool install --global csharp-ls` | same (needs `~/.dotnet/tools` on PATH) |
| **curl** / `Invoke-WebRequest` | fetching hook files | for hooks | preinstalled | preinstalled |
| **brew** / **winget** | `github-cli` extra only | optional | Homebrew | winget |

> **Why node ≥ 22.12:** below that, some MCPs (notably `chrome-devtools`) refuse to start and
> die at launch with a generic JSON-RPC `-32000`.

> **Claude Code on Windows (winget):** `winget install Anthropic.ClaudeCode` is the native install -
> no Node needed for the CLI itself, but keep Node ≥ 22.12 anyway for the hooks and npx MCPs. Mind the
> package ID: `Anthropic.ClaudeCode` is the CLI; `Anthropic.Claude` is the separate desktop app. winget
> installs don't auto-update - refresh the CLI with `winget upgrade Anthropic.ClaudeCode` (this is the
> CLI binary, independent of the stack's own `update` action). npm still works on Windows too.

---

## Before you run - environment variables & keys

Set these in your shell **before** invoking the script (all optional except where a default is noted).

### `--scope` / `-Scope` - where the stack lands (default `project`)

| Value | Skills | Plugins / MCPs |
| ----- | ------ | -------------- |
| `project` (default) | project-scoped | `--scope project` (writes `<repo>/.mcp.json`) |
| `global` | `-g` (`~/.claude/skills`) | `--scope user` |

```bash
bash claude-stack.sh install --scope global          # macOS/Linux
pwsh claude-stack.ps1 install -Scope global          # Windows
```

The `SCOPE` env var is still honored as a **fallback** when the flag is absent
(`SCOPE=global bash claude-stack.sh install`); the flag wins when both are set.

### The `--space` flag / `CLAUDE_CONFIG_DIR` - which Claude account

Pass `--space <name>` (`-Space <name>` on Windows; any word) and the installer targets the
`~/.claude-<name>` account - it exports `CLAUDE_CONFIG_DIR` so the `claude` CLI installs
skills/plugins/MCPs there - and uses a separate `memory_<name>.db`:

```bash
bash claude-stack.sh install --space work        # -> ~/.claude-work account + memory_work.db
bash claude-stack.sh install --space clientx     # -> ~/.claude-clientx + memory_clientx.db
```

Without a space, the default `~/.claude` account is used. To target a specific account **manually**
(no space), set `CLAUDE_CONFIG_DIR` yourself - used only for path resolution, never exported:

```bash
export CLAUDE_CONFIG_DIR="$HOME/.claude-work"          # macOS/Linux
$env:CLAUDE_CONFIG_DIR = "$HOME\.claude-work"          # Windows
```

### `CONTEXT7_API_KEY` / `SENTRY_*` - the optional secrets

The `context7` MCP reads `CONTEXT7_API_KEY` **from the environment at launch**, so expose it to
the MCP process *without* writing it into the registration (`.mcp.json`). **Leave it unset in your
install shell** so the registration stays keyless, and provide the key one of these ways:

- **macOS / Linux (recommended).** Put it in `~/.claude/settings.json` under `"env"` (set once,
  user-global): `{ "env": { "CONTEXT7_API_KEY": "ctx7-..." } }`.
- **Windows (recommended).** Set a **persistent OS environment variable** - **Machine scope** so
  even an elevated PowerShell inherits it: `[Environment]::SetEnvironmentVariable('CONTEXT7_API_KEY','ctx7-...','Machine')`
  (or `setx … /M`). Open a **new** terminal afterward. (`settings.json` `env` works on Windows too.)
- **Legacy - bake it in (any OS).** Export `CONTEXT7_API_KEY` **before** running and it is added as
  `--api-key` to the registration; at project scope that writes it into `<repo>/.mcp.json` - **keep
  that file uncommitted** if you go this route.

The `sentry` MCP works the same way: it registers the hosted remote server at
`https://mcp.sentry.dev/mcp` with a literal `Authorization: Bearer ${SENTRY_ACCESS_TOKEN}` header
that Claude Code expands **at launch** from `settings.json` `"env"` - put the token there, and
comment the server out of the manifest where the project has no Sentry. No other API keys are
required by any component.

---

## How to run

The **action** (`install` | `update`) is the one **required** positional argument; everything else is
a **named flag**, optional with a default. Download the installer **into the project's `.claude/`**
and keep it there - the downloaded copy is the per-project manifest you trim and re-run for `update`.

macOS / Linux (`claude-stack.sh`):

```bash
cd /path/to/your/project        # run inside the target project

# 1) download the installer into the project's .claude/
mkdir -p .claude && curl -fsSL https://raw.githubusercontent.com/envoydev/claude-stack/main/scripts/claude-stack.sh -o .claude/claude-stack.sh

# 2) install (first time) / update (later)
bash .claude/claude-stack.sh install
bash .claude/claude-stack.sh update

# Named flags (any order): --space (account + memory DB), --scope, --context7, --github-cli, --keep-pins
bash .claude/claude-stack.sh install --space work                    # space 'work' -> ~/.claude-work account + memory_work.db
bash .claude/claude-stack.sh install --github-cli
bash .claude/claude-stack.sh install --space work --scope global --github-cli
bash .claude/claude-stack.sh install --context7 local                # local npx context7 (default: remote hosted server)
bash .claude/claude-stack.sh update --keep-pins                      # refresh, but keep local model/effort pin edits
```

Windows (`claude-stack.ps1`):

```powershell
Set-Location C:\path\to\your\project

# 1) download the installer into the project's .claude\
New-Item -ItemType Directory -Force .claude | Out-Null
Invoke-WebRequest -Uri https://raw.githubusercontent.com/envoydev/claude-stack/main/scripts/claude-stack.ps1 -OutFile .claude/claude-stack.ps1

# 2) install (first time) / update (later)
pwsh .claude/claude-stack.ps1 install
pwsh .claude/claude-stack.ps1 update

pwsh .claude/claude-stack.ps1 install -Space work                   # space 'work' -> ~/.claude-work + memory_work.db
pwsh .claude/claude-stack.ps1 install -GitHubCli                    # install gh (switch)
pwsh .claude/claude-stack.ps1 install -Scope global -Context7 local # global scope + local npx context7 (default: remote)
pwsh .claude/claude-stack.ps1 update -KeepPins                      # refresh, but keep local model/effort pin edits
```

> On Windows PowerShell 5.1 use `powershell` instead of `pwsh`. If scripts are blocked, run once:
> `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`.

### Arguments

The **action** is the one positional argument; everything else is a **named flag** (any order). The
old positional forms (`install work`, `install github-cli`) are gone - pass a value with its flag.

| Argument | Values | Meaning |
| -------- | ------ | ------- |
| **action** (positional) | `install` \| `update` (required) | `install` adds everything (idempotent - existing items skipped, MCP versions frozen). `update` brings everything to latest (clean re-add of skills, plugin/MCP refresh, re-fetch hook files). |
| **`--space`** \| **`-Space`** | any word | Selects the Claude account `~/.claude-<space>` (skills/plugins/MCPs install there) and a separate `memory_<space>.db`. Omit for `~/.claude` + shared `memory.db`. |
| **`--scope`** \| **`-Scope`** | `project` (default) \| `global` | `project` installs INTO this repo; `global` installs into the active account. Falls back to the `SCOPE` env var when the flag is absent. |
| **`--context7`** \| **`-Context7`** | `remote` (default) \| `local` | context7 transport: `remote` is the hosted HTTP server, `local` the npx stdio server. |
| **`--github-cli`** \| **`-GitHubCli`** | flag / switch | Install the GitHub CLI (`gh`) if missing. No auth during install; run `gh auth login` once before first GitHub platform use. |
| **`--keep-pins`** \| **`-KeepPins`** | flag / switch | Keep this project's **local `model:`/`effort:` frontmatter edits** on installed agents (`.claude/agents/`) and skills (`SKILL.md`) across the refresh - values are snapshotted before the fetch/reinstall and re-applied after (which otherwise resets them to upstream). Only keys present in both the old local file and the refreshed one are re-applied, and the local value always wins over an upstream pin change - drop the flag for one run to take upstream pins. |

### Claude-driven install - analyze, trim, confirm, run

For a new project, let Claude Code do the manifest trim instead of hand-editing. Give a session at
the project root this three-step brief:

1. **Download the installer into `.claude/`:**

   ```bash
   mkdir -p .claude && curl -fsSL https://raw.githubusercontent.com/envoydev/claude-stack/main/scripts/claude-stack.sh -o .claude/claude-stack.sh
   ```

   ```powershell
   New-Item -ItemType Directory -Force .claude | Out-Null
   Invoke-WebRequest -Uri https://raw.githubusercontent.com/envoydev/claude-stack/main/scripts/claude-stack.ps1 -OutFile .claude/claude-stack.ps1
   ```

2. **Analyze the project, then trim the script.** Detect the stacks actually present (languages,
   frameworks, desktop/mobile surfaces, CI files), open the downloaded script, and **comment
   out - never delete -** the manifest entries this project does not need: the per-stack `SKILLS`
   entries, the conditional `MCPS` (`angular-cli` outside Angular, `appium-mcp` / `chrome-devtools`
   without their native deps, `memory` in a standalone project), and the per-stack `AGENTS` +
   `CLAUDE_RULES` entries. Leave everything cross-stack alone.

3. **Stop for confirmation.** Show what was commented out and why, and wait for the go-ahead - run
   nothing before it. On confirmation, install from the project root:

   ```bash
   bash .claude/claude-stack.sh install
   ```

   ```powershell
   pwsh .claude/claude-stack.ps1 install
   ```

   (named flags as needed - see Arguments above), then follow the script's printed next steps.

The trimmed copy IS the per-project manifest: keep it (committed or local), and re-run it for
`update` so the trim survives - a fresh download restores the full manifest. If the project also
re-pinned any agent/skill `model:`/`effort:`, add `--keep-pins` to that `update` so the local pins
survive the refresh too.

### Before you start - the first-run captures

After `install`, fill the seeded `CLAUDE.md` (write the project top from its authoring-outline
comment, trim its rules table to what was laid down), then run the capture skills once
from a Claude Code session at the project root - all four are manual `/`-commands (they never
auto-fire), in this order:

1. **`/project-capabilities`** - inventories the actually-installed skills / agents / MCPs / plugins
   and generates `baseline-project-capabilities.md` (the usage policy + the project's real
   inventory, loaded every session). Re-run after every `update` or manifest trim.
2. **`/project-related-context <paths/URLs>`** - only when the repo has siblings (a
   backend/frontend pair, a consumed package): generates the sibling-awareness rule +
   `docs/PROJECT-RELATED-CONTEXT.md`.
3. **`/project-architecture-analyzer`** - the architecture capture:
   `docs/architecture/ARCHITECTURE.md` + `ASSESSMENT.md` + the always-on awareness rule. Every seat
   orients from this map instead of re-deriving the project.
4. **`/project-code-style-analyzer`** - `docs/PROJECT-CODE-STYLE.md` + the inject-code-style hook
   that surfaces it at edit time, filtered to the observed file types.

On a greenfield repo skip 3-4 (no code to characterize yet) - `/project-build-from-scratch` covers
the design instead; run those two captures once real code exists. All captures are
deliberate-refresh only: re-run them via their `/`-commands when the project shifts, never after
each change.

### Before you start - the MCP baseline

The MCPs land in `<repo>/.mcp.json`, pre-approved in `settings.json` (no per-launch trust prompt) -
**restart Claude Code once after `install`** so they load. What each one is for, so the first
session reaches for the right tool:

| MCP | Reach for it when |
| --- | ----------------- |
| **serena** | Locating any symbol or its references (`find_symbol` / `find_referencing_symbols`) - the shipped guard blocks whole-file `Read`s of large sources, so navigation goes through serena first. Self-activates from the project root on launch (`--project-from-cwd`); its `.serena/` state dir must be gitignored. **Installed? Index the project before the first session** (command below) so symbol/reference navigation is ready from the first query. |
| **context7** | Any design or upgrade against a library/framework API - current docs beat recalled knowledge. Optional `CONTEXT7_API_KEY` in settings `env` raises rate limits. |
| **memory** | Cross-project recall (one shared DB under `~/.memory-mcp`). Comment it out in a standalone project - the per-feature subagent hand-off runs on serena's local memory, not this. |
| **playwright** | Driving a real browser to verify web UI work. |
| **angular-cli** / **chrome-devtools** / **appium-mcp** / **sentry** | Conditional and/or heavy: Angular workspaces, browser/extension debug, native mobile E2E, Sentry error monitoring (hosted remote MCP; `SENTRY_ACCESS_TOKEN` in settings `env`). Trim them from the manifest where they do not apply - the heavy two die at launch without their native deps. |

**serena: index the project before you start.** With serena in the stack, run its indexer once from
the project root before the first working session - it builds the symbol/reference index up front,
so the first `find_symbol` / `find_referencing_symbols` answers instantly instead of cold-starting
the language server against an unindexed codebase (slow on large repos). `SERENA_HOME` must match
the registration (`.serena/home`) so the index lands in the project's own serena state:

```bash
SERENA_HOME=.serena/home uvx --from serena-agent serena project index
```

```powershell
$env:SERENA_HOME = '.serena/home'; uvx --from serena-agent serena project index
```

Re-run it only after a large code drop (a merge, a generated layer) - day-to-day edits keep the
index current on the fly.

The trim decisions are the Claude-driven install's step 2 above; `/project-capabilities` then bakes
the surviving inventory + routing into the generated awareness rule, so this table is only the
day-one orientation.

---

## `install` vs `update`

- **`install`** - additive, **idempotent**. Already-registered skills/plugins/MCPs are skipped;
  MCP runtime versions resolve to *latest at provision* then **freeze** until the next `update`.
  Hooks are fetched into `.claude/hooks/` and wired into `.claude/settings.json`.
- **`update`** - removes and re-adds skills (real copies, not symlinks), re-resolves/re-pins MCP
  versions, refreshes plugins, and re-fetches hook files. It does **not** rewrite the
  `settings.json` hook wiring (that happens only on `install`). The refresh resets every agent/skill
  file to upstream - if this project re-pinned a seat's `model:`/`effort:`, run with `--keep-pins`
  (`-KeepPins`) to carry those local values across.

**Claude-only steps fail soft** if the `claude` CLI is absent - skills still install.

### Optional: tool-usage instrumentation (opt-in, not installed by default)

The orchestrator can't see which skill or MCP a dispatched subagent actually loaded or called -
only that subagent's aggregate token/tool_use totals - so a real run's skill / MCP usage can only be
*assessed*, not measured. For an audit or benchmark run that needs the exact tally,
`hooks/instrument-tool-usage.js` is a **PreToolUse** hook that logs every `Skill` and `mcp__*`
call as one JSONL row. It is **not** in the installer's `HOOKS` set (zero cost by default) and is
inert unless enabled. To turn it on for a run:

1. The installer already fetched it to `.claude/hooks/instrument-tool-usage.js` (its `HOOKS` entry
   has an empty matcher: fetched, never wired - a wired `".*"` hook costs a node spawn per tool call).
2. Wire a PreToolUse hook to it in `.claude/settings.json` with matcher `".*"` (logs every tool - built-ins + Skill + MCP; use `"Skill|mcp__.*"` to scope to skills/MCP only).
3. Run with `STACK_INSTRUMENT=1` (optionally `STACK_INSTRUMENT_LOG=<path>`); rows land in
   `<project>/.claude/tool-usage.<session>.jsonl`. Unset the env (or remove the wiring) to stop.

Tallying the stats from the JSONL:

```bash
jq -r .tool .claude/tool-usage.*.jsonl | sort | uniq -c | sort -rn          # tool mix
jq -r 'select(.tool=="Skill") | .detail' .claude/tool-usage.*.jsonl | sort | uniq -c | sort -rn  # which skills actually fire
jq -r 'select(.tool|startswith("mcp__")) | .detail' .claude/tool-usage.*.jsonl | sort | uniq -c  # per-MCP volume
```

It only ever observes - it never blocks a call. Verify subagent-call coverage against a known run
before trusting a tally (whether PreToolUse propagates into dispatched subagents is build-dependent).

The hook can never see tokens - token accounting lives per API message in the transcript JSONL
Claude Code already writes. For the full consumption report (tokens by scope/model, exact
per-dispatch subagent cost, skill/MCP/tool call + result volume, context-growth spikes, optional
join against the hook ledger), run this repo's offline analyzer against a session transcript:

```bash
node <claude-stack>/scripts/analyze-usage.js ~/.claude/projects/<encoded-project-path>/<session-id>.jsonl
node <claude-stack>/scripts/analyze-usage.js ~/.claude/projects/<encoded-project-path>/            # per-session rollup
node <claude-stack>/scripts/analyze-usage.js <session>.jsonl --hook-log .claude/tool-usage.<sid>.jsonl
```

---

## Memory database

Claude Code and Cursor share **`~/.memory-mcp`** so both see the same DB:

- default: `memory.db` (`sqlite_vec` backend),
- with a **space** (e.g. `work`): `memory_<space>.db` - a separate per-space DB (same `sqlite_vec` backend, different path).

The path is resolved at install time, so the choice is baked into the registration.

---

## Troubleshooting

| Symptom | Cause / fix |
| ------- | ----------- |
| MCP dies at launch with `-32000` | Node too old (use ≥ 22.12 LTS); or a stale npm cache against a freshly pinned version - the script intentionally avoids `--prefer-offline`. |
| `serena` / `memory` MCP missing | `uvx` not installed - install uv (see prereqs). `memory` also needs numpy, which the script injects via `--with numpy`. |
| `security-guidance` hook fails | Python 3 missing (on Windows the Store stub doesn't count - install a real Python). |
| `csharp-lsp` won't start | `csharp-ls` not on PATH - install per the prereqs (only needed for C# work). |
| `csharp-ls` install fails: `DotnetToolSettings.xml was not found` | Your .NET SDK is older than the tool's latest (`csharp-ls` 0.24.0 targets .NET 10); that error is dotnet's misleading wording for 'the tool targets a framework you don't have', not a broken package. `dotnet --list-sdks` to check. Cross-platform fix: pin a version matching your SDK - .NET 9 -> `--version 0.20.0`, .NET 8 -> `--version 0.15.0`. Or install the .NET 10 SDK and retry (Windows: `winget install Microsoft.DotNet.SDK.10`; macOS/Linux: package manager or the `dotnet-install` script `--channel 10.0`) - side-by-side, `global.json` keeps projects on their own SDK. |
| "not in a git repo - skipping…" | Project scope needs a git repo. Run `git init`, or use `--scope global`. |
| Plugins/MCPs skipped | `claude` CLI not installed - install it (or use the Cursor stack, which never needs it). |
| context7 key ended up in `.mcp.json` | You exported `CONTEXT7_API_KEY` (legacy path) at project scope. Prefer the `settings.json` `env` approach; keep `.mcp.json` uncommitted. |

## Available skills

- **create-ticket** - Generate a well-structured bug, user story, epic, or technical
  task ticket in English from a raw description (Ukrainian or English), for any issue tracker. Detects the type and routes to a per-type template.
- **dev-log-convert** - Convert mixed Ukrainian/English daily work notes into a
  structured English work log. Triggers on `dev-log`.
- **explain-code-tutor** - Explain code, a bug, a concept, or an approach trade-off like a
  patient senior engineer: one fitted analogy, numbered steps with real project snippets, a marked break/insight/verdict, the real fix, and a one-line takeaway. Depth ELI5/intermediate/expert.
- **angular-conventions** - Angular 17+ conventions: signals, standalone
  components, OnPush, reactive forms, accessibility (TS baseline → typescript).
- **angular-material** - Angular Material + CDK conventions: selective imports,
  M3 theming, CDK primitives, and component test harnesses (the @angular/material library specifically, not generic MD3).
- **angular-styling** - Angular CSS/SCSS architecture (Material or not):
  ViewEncapsulation, `:host` / `:host-context`, the discouraged `::ng-deep` and its sanctioned ways out, design tokens as CSS custom properties, mobile-first + container queries, and accessibility-affecting styling (Material token work → angular-material).
- **angular-security** - Angular / web frontend security hardening: XSS and the DomSanitizer
  `bypassSecurityTrust*` escape hatches, CSP, CSRF via HttpClient XSRF, no secrets in the bundle, token storage, and SSR/TransferState leaks (server side → dotnet-security, native shell → mobile-security).
- **frontend** - Web frontend router: indexes Angular, TypeScript,
  distinctive production-grade UI (in-skill design-quality guidance), and Ionic/Capacitor (→ mobile).
- **mobile** - Ionic / Capacitor router/index over the Angular + TypeScript
  baselines; in-app navigation and page lifecycle owned by ionic.
- **ionic** - House Ionic / Capacitor conventions: Ionic UI (standalone +
  signals), navigation, lifecycle/permissions, and Capacitor plugin sourcing + wrapping.
- **capacitor-release** - House Ionic / Capacitor release pipeline: cap sync +
  native build, iOS/Android signing, store submission, OTA / live updates, version sync, Fastlane/Actions CI, and dSYM/sourcemap upload.
- **mobile-security** - Ionic / Capacitor mobile security hardening: Keychain/Keystore secret
  storage (never plaintext Preferences), deep-link input validation, least-privilege permissions, cleartext/WebView hardening, and data-at-rest protection (web layer → angular-security, signing → capacitor-release).
- **csharp** - C# style and runtime conventions: layout, naming, modern
  syntax, async, exceptions, logging, dependency injection.
- **csharp-design-patterns** - All 23 GoF design patterns with modern
  .NET 8+ idioms: selection table, anti-pattern checks, framework-native forms.
- **database-conventions** - Database conventions across Postgres,
  SQL Server/T-SQL, SQLite, and MongoDB: schema, migrations, indexes, query safety.
- **data-security** - SQL / data-layer security hardening: parameterized-only
  injection (no interpolated FromSqlRaw, sp_executesql for dynamic proc SQL), least-privilege DB accounts, row-level security + tenant isolation, connection-string secrets, encryption at rest/in transit, and audit-logging (app-layer EF → dotnet-security, primitives → dotnet-cryptography, migrations → dotnet-migrate).
- **dotnet** - Router that points to the focused .NET/C# specialist
  skill for the area you are working in.
- **dotnet-architecture** - Choose and hold a .NET architecture: the decision hub (topology,
  internal style, DDD-additive) with clean-architecture / ddd / vertical-slice / modular-monolith / microservices in references/. .NET 8 floor.
- **dotnet-architecture-tests** - Architecture fitness tests: encode layer /
  dependency / naming / isolation rules as build-failing tests with NetArchTest (default) or ArchUnitNET. .NET 8 floor.
- **dotnet-aspire** - .NET Aspire local orchestration: AppHost,
  ServiceDefaults, service discovery, dashboard. Orchestration-only; .NET 8 floor.
- **dotnet-authentication** - ASP.NET Core authentication/authorization:
  JWT/OIDC/Identity, policy-based authorization, secret storage. .NET 8 floor.
- **dotnet-code-quality** - Mechanically enforce the C# house style: CSharpier
  formatter ownership, SDK analyzers (AnalysisLevel / .editorconfig severity), TreatWarningsAsErrors + legacy batch promotion, Roslynator, the dotnet build CI gate. .NET 8 floor.
- **dotnet-console-apps** - The console app's interface surface on the generic host:
  CLI argument parsing (System.CommandLine 2.0 / Spectre.Console.Cli / Cocona) and bots / gateway consumers (Telegram / Discord / Slack / exchange) run in a BackgroundService. .NET 8 floor.
- **dotnet-cryptography** - System.Security.Cryptography: SHA-2, AES-GCM,
  RSA/ECDSA, PBKDF2/Argon2id, constant-time comparison, deprecated-algorithm matrix. .NET 8 floor.
- **dotnet-data-access** - EF Core + NHibernate data access: DbContext/session lifetime, change
  tracking, N+1 and loading, projection to read models, bounded results, bulk ops; per-ORM depth in references/.
- **dotnet-error-handling** - Result pattern + ProblemDetails (RFC 9457) +
  global exception handler (IExceptionHandler) + FluentValidation. .NET 8 floor.
- **dotnet-grpc** - gRPC: .proto/codegen, ASP.NET Core host, four streaming
  modes, JWT/mTLS, interceptors, health checks, gRPC-Web. .NET 8 floor.
- **dotnet-hosted-services** - Hosted-service / worker conventions:
  BackgroundService vs IHostedService, the ExecuteAsync exception trap, scoped scopes, PeriodicTimer, graceful shutdown, Channels; references/ add 24/7 hardening (I/O resilience, rate limiting, ClientWebSocket reconnect), scheduling + leader election, and deployment/signals. .NET 8 floor.
- **dotnet-messaging** - Event-driven messaging: Wolverine (MIT) /
  MassTransit, transactional outbox, choreography vs sagas, RabbitMQ / Azure Service Bus. .NET 8 floor.
- **dotnet-migrate** - Safe migration workflow for EF Core schema, .NET
  upgrades, and NuGet updates - rollback + verification at each step.
- **dotnet-minimal-api** - Minimal API endpoint mechanics: MapGroup,
  TypedResults, endpoint filters, parameter binding, metadata. .NET 8 floor.
- **dotnet-mvc-controllers** - Controller-based Web API mechanics:
  [ApiController], attribute routing, ActionResult<T>, the automatic-400 filter, action filters, thin controllers. .NET 8 floor.
- **dotnet-openapi** - OpenAPI document generation (Swashbuckle / built-in
  .NET 9+) and the Scalar docs UI. .NET 8 floor.
- **dotnet-realtime** - ASP.NET Core SignalR real-time push: strongly-typed
  Hub<TClient>, IHubContext, groups/presence, reconnection, JWT-over-query-string auth, and Redis / Azure SignalR scale-out. .NET 8 floor.
- **dotnet-security** - OWASP Top 10 (2021) mapped to .NET 8 mitigations;
  delegates auth and crypto to their dedicated skills. .NET 8 floor.
- **dotnet-source-generators** - Roslyn IIncrementalGenerator authoring +
  built-in generators (GeneratedRegex / LoggerMessage / System.Text.Json). .NET 8 floor.
- **dotnet-testing** - .NET testing conventions: AAA structure, per-layer
  strategy, coverage thresholds, and test-library routing.
- **dotnet-web-backend** - ASP.NET Core HTTP-service cross-cutting
  conventions: HttpClientFactory, Polly, API versioning, OpenAPI, observability.
- **dotnet-winforms** - WinForms conventions: logic out of code-behind,
  control / component / GDI disposal discipline, high-DPI, with per-version
  (4.8 vs .NET 8/9/10) mechanics and migration deltas in references.
- **dotnet-wpf** - WPF conventions: strict MVVM, binding modes, UI
  threading, list virtualization, localization.
- **postgres** - PostgreSQL engine specialist: index-type selection, JSONB / full-text, SARGable
  rewrites, the planner (EXPLAIN, pg_stat_statements, autovacuum), connection pooling.
- **sqlite** - SQLite engine specialist: WAL / single-writer concurrency, PRAGMAs, type affinity vs
  STRICT, limited ALTER TABLE, connection-per-thread, FTS5.
- **project-quality-loop** - Autonomous review-and-fix loop pipeline: run a `docs/loops/` folder of
  numbered prompts in order, looping each on a target to zero findings, deciding autonomously.
- **project-architecture-quality-loop** - Deliberate architecture analyze-assess-improve loop: the
  project-architecture-analyzer capture writes `docs/architecture/ARCHITECTURE.md` + a reasoned strengths/weaknesses `docs/architecture/ASSESSMENT.md`,
  then fixes the weaknesses by tier (small -> implementer, substantial -> designer-led build, structural ->
  flagged for approval) and reconciles the docs. Manual, `/`-only - the heavy counterpart to project-quality-loop.
- **project-architecture-analyzer** - Deliberate architecture capture: dispatches code-analyzer per
  module, reasons over the digests in the main session, and writes `docs/architecture/ARCHITECTURE.md` +
  `docs/architecture/ASSESSMENT.md` + the generated always-on awareness rule
  `baseline-project-architecture.md` (micro-summary + read-the-map trigger). Capture only - fixing the weaknesses is project-architecture-quality-loop.
  Manual, `/`-only.
- **project-version-upgrade** - Deliberate flow for any breaking version event (framework, runtime,
  or package major): plan in-session from the real breaking surface (context7 + usage digests),
  approval gate (auto mode only on an explicit ask), then staged execution - implementers edit,
  resolvers clear reds, a gate after every stage. Manual, `/`-only.
- **project-capabilities** - Deliberate capabilities capture: inventories the installed skills,
  seats, MCPs, and plugins, and generates the always-on awareness rule
  `baseline-project-capabilities.md` from the real inventory - so sessions are never steered at a
  capability the project doesn't carry. Re-run after install/update/trim. Manual, `/`-only.
- **project-code-style-analyzer** - Deliberate project code-style capture: fans out code-style-analyzer
  agents (one per detected language, parallel), merges their reports into `docs/PROJECT-CODE-STYLE.md`
  (the project's actual style - config-enforced rules + idioms a linter cannot encode), then generates
  the inject-code-style hook - extension filter from the observed extensions - and wires it into
  `settings.json`. The hook injects the doc once per session on the first code-file edit. Manual, `/`-only.
- **project-related-context** - Deliberate related-projects capture, args-driven (paths or git URLs,
  never a scan): fans out related-project-analyzer agents (one per sibling, parallel) and writes both
  tiers from their YAML entries (evidence-grounded or UNVERIFIED) - the always-on awareness rule
  `.claude/rules/baseline-project-related-context.md` (name/location/relation/seam) and
  `docs/PROJECT-RELATED-CONTEXT.md` (first_read + evidence, read on a seam touch).
  Re-run upserts entries per passed sibling in both files. Manual, `/`-only.
- **project-verify-plan** - Audit an implementation plan before writing code: a risk-coverage review that
  checks the plan names the traps its stack will hit (routing to the stack skill), matches scope,
  covers the edges, and stays minimal - the cheapest place to catch a design error, upstream of code review.
- **project-implementer** - Single-chat build step: execute a verified plan task by task - honor each
  card's contract, gate every task green (resolver agents absorb stubborn reds), flag scope beyond
  the plan, finish via `/code-review` + the done-gate. Completes the in-session
  design -> audit -> build -> review vertical.
- **project-solution-design** - Work out how a feature fits the existing code before building, in a single
  chat: read the committed architecture, judge where the change belongs (extend a seam, refactor
  first, or isolate a new boundary), load the stack skill for its traps, and decompose into an
  ordered, minimal plan. The in-context twin of the designer agent; feeds `project-verify-plan`.
- **project-failure-signatures** - Match a runtime crash to its signature and isolate the real cause: a
  lookup of the common local-runtime signatures (null-reference, DI resolution, async deadlock,
  disposed-lifecycle, config drift, boundary, database contention, HTTP-status) each mapped to where the cause lives -
  usually not the line that threw. Pairs with the systematic-debugging method.
- **project-ci-failure-signatures** - Triage a red CI pipeline or PR check in the current chat: match the failure to a
  signature (compile/restore, green-locally-red-on-runner, quality gate, signing, workflow drift,
  infra flake), make the code-vs-environment call, and route it. The single-chat twin of the
  `ci-failure-diagnoser` agent; the CI sibling of `project-failure-signatures`.
- **project-build-from-scratch** - Build a new application or major module from scratch: routes greenfield
  work to the right architecture skill and scaffolding command, then drives design -> scaffold ->
  slice-by-slice build over the agent pipeline.
- **project-task-flow** - Entry-point orchestrator for multi-agent engineering work: classify a
  feature or bug, pick the smallest safe execution mode, run a single stack's design-build-verify
  trio per its domain-trio protocol (the designer decomposes, implementers build in parallel, the
  verifier gates and loops back), and for cross-domain work freeze the shared contract, run the
  stack pipelines in parallel, then gate the assembled whole through the integration-reviewer
  before commit. Home of the shared subagent policies (contract change, structured output, model
  and token routing).
- **devops** - Containers, CI/CD, and safe deploys for the .NET/Angular house: multi-stage
  digest-pinned non-root Docker, GitHub Actions CI/CD (lockfile-hash caching, service-container
  tests, masked secrets, SHA-pinned actions, OIDC), and health-gated expand-contract deploys.
- **typescript** - Framework-agnostic TypeScript/JavaScript baseline:
  strict typing, type modeling, modules, async, error handling, JS with JSDoc.
- **markdown-style** - Markdown authoring and review: a syntax canon (valid,
  portable Markdown) plus an opinionated house style overlay, applied in a
  two-pass procedure. Form only, not prose (Vale) or spelling.
- **docs-as-code** - Docs-as-code authoring: Mermaid sequence and ER
  diagrams, architecture decision records (Nygard and MADR 4, with the
  operational discipline), and C4 context/container views - when to use each,
  per-type syntax and pitfalls, and when to graduate to DBML or Structurizr.
- **ilspy-decompile** - Decompile a compiled .NET assembly with ilspycmd to
  read its real implementation: framework internals, NuGet source, or
  behavior confirmation before a framework upgrade.
- **dotnet-project-setup** - Set up a new .NET solution's build spine: the
  src / tests / .config layout, .slnx, Directory.Build.props, global.json,
  central package management, and dotnet-tool pinning. A hub with references.
- **dotnet-performance** - Performance-aware .NET design: allocation and type
  layout (struct vs class, Span, ValueTask, frozen collections) plus
  serialization-format choice (System.Text.Json source-gen, Protobuf,
  MessagePack). A hub with references.
- **dotnet-diagnostics** - Measure and diagnose a live .NET process: BenchmarkDotNet
  microbenchmarks and crash / hang / OOM dump capture (dotnet-dump, dotnet-gcdump,
  containers) with a first-look SOS pass. A hub with references.
- **nx** - Nx monorepo project-graph layer: navigate via nx show projects / nx graph /
  nx affected instead of dumping config, scope build / test / lint to affected projects,
  scaffold with nx generate, enforce module boundaries with tags. Draws the serena-vs-Nx
  routing line; teaches the CLI over the (minimal-mode) Nx MCP.

## Working in a single chat

The same build/diagnose flow runs two ways: dispatch the **subagents** (isolated contexts,
parallel fan-out) or load the **skills that reproduce each seat inside your current chat**
(visible, checkpointed, at your model). The single-chat form is the cheaper path for
small-to-medium single-stack work - you see and correct every step instead of reading a
dispatched agent's report.

| Agent seat | Load this in a single chat |
|---|---|
| `<stack>-solution-designer` | `project-solution-design` |
| verifier (before build) | `project-verify-plan` |
| `<stack>-implementer` | just code - conventions auto-load on file touch |
| verifier (after build) | `/code-review` + `verification-before-completion` |
| `issue-diagnoser` | `systematic-debugging` + `project-failure-signatures` |
| `ci-failure-diagnoser` | `project-ci-failure-signatures` |

The trio loop is `project-solution-design` -> `project-verify-plan` -> build under the auto-loaded conventions
-> `/code-review`, with a checkpoint after each step.

The inverse path runs the same flow as a dispatched team of 33 model-pinned subagents - the
`project-task-flow` orchestration skill owns that ladder (the roster is laid out at the top of
[docs/claude-stack.html](docs/claude-stack.html)). Pick by size: stay in chat for small
single-stack work, dispatch the team for large, parallel, cross-domain, or log-heavy work.

## Repository layout

```text
skills/                 # the personal skills - one <skill-name>/SKILL.md per skill (some carry a references/ subfolder)
scripts/claude-stack.{sh,ps1}  # the stack installer twins; docs/claude-stack.html is the agent-roster viz + full inventory
templates/CLAUDE.template.md   # the stack-neutral per-project skeleton you copy into a project and fill in
agents/ hooks/ rules/   # the 33 subagents, the guard hooks, and the 15 rules the installers fetch
setup-plugin/           # the claude-stack plugin (setup = fresh install, configure = update; /claude-stack routes)
scripts/lint-skills.js  # repo lint (keeps skills / manifests / HTML in sync)
```

`skills/` is the flat layout the installers copy directly from (one `<skill-name>/SKILL.md`
per skill) - including the Cursor installers in [`cursor-stack`](https://github.com/envoydev/cursor-stack),
which clone this repo.

## Maintenance

After adding, renaming, or removing a skill, plugin, or MCP, run the repo
lint. The installer scripts (`claude-stack.{sh,ps1}`) are the single
source of truth for everything in
use; the lint verifies every skill directory is registered in the manifests
and this README, that each `SKILL.md` frontmatter loads as valid YAML (using
js-yaml, so a malformed block fails here instead of reaching a project
uncaught), that cross-skill references
resolve everywhere they appear (`SKILL.md` files, the Claude subagents, the base
template, and the Claude rules - a renamed skill is caught in all of them),
that both installers agree (`SKILLS` + `MCPS` + `PLUGINS` identical across both
shells, `SKILLS` in the SAME ORDER), that the on-disk subagents match
the set the installers fetch, that every README headline count (skills /
plugins / MCPs / hooks / agents / rules) matches the installer, and that the stack
inventory HTML matches the manifests in both directions. A change to the shared
`SKILLS`/`MCPS` baseline is a two-repo commit - mirror the manifest lists in
`cursor-stack` in the same sitting:

```bash
npm install              # one-time: the lint needs js-yaml
node scripts/lint-skills.js   # or: npm run lint
```

## License

[MIT](LICENSE) © 2026 envoydev
