## First prompt (implemented)

OBJECTIVE
Analyze the skills defined in claude-stack.html and recommend whether to adopt
them as-is or extract only the parts relevant to our flow into project-owned
skills. Then draft the owned PostgreSQL and .NET skills.

CONTEXT
- claude-stack.html contains third-party skill definitions. Analyze skills only,
  ignore plugins.
- Assume this content may be over-engineered or redundant for our use case.
- Goal: skills that adapt to OUR current agent flow, not skills we bend our flow
  around.
- Core domains we care about: PostgreSQL and .NET.

TASKS
1. Parse claude-stack.html and list every skill (skip plugins). For each report:
   name, purpose, approximate token size, and any external dependencies.
2. Classify each skill's content as: (a) directly useful, (b) partially useful,
   (c) redundant / over-engineered / irrelevant to our flow.
3. Per skill, compare adopting wholesale vs. extracting only the useful parts.
   Give a recommendation: adopt / extract / drop, with rationale.
4. Evaluate merging and moving skills into our ownership. Identify which source
   skills overlap or are fragmented and should be consolidated into fewer owned
   skills, and which single skills should be split. The point is that once a skill
   lives with us, we can reshape it to fit our flow exactly - merge related ones,
   move parts between them, or split by responsibility. Recommend the target
   structure and how each source skill maps into it.
5. Build the project-owned skills from the extracted useful content. At minimum:
   - a PostgreSQL skill
   - a .NET skill
   Design them single-responsibility so they stay cheap to modify, merge, or split
   later, with no runtime dependency on remote skills.
6. Estimate token savings of the owned/trimmed/consolidated versions vs. the
   originals.
7. Specify where each agent should reference each skill.
8. Note what upstream value we lose by forking (fixes/updates from the source
   skills) and propose a lightweight strategy to keep the owned skills current.

CONSTRAINTS
- Owned skills only. No runtime dependency on remote/third-party skills, since a
  remote skill available today can be deleted tomorrow and access lost.
- Optimize for low token footprint: split or trim wherever it does not reduce
  utility.
- Keep each skill single-purpose so merging and splitting stay cheap.

DELIVERABLE
- Table: skill -> classification -> recommendation -> rationale.
- Target structure map: source skills -> owned skills (merge/move/split).
- Drafts of the owned PostgreSQL and .NET skills.
- Token savings estimate.
- Agent-to-skill reference map.


## Second prompt (partially implemented)

OBJECTIVE
Investigate every skill in claude-stack.html (skills sourced from GitHub) and
identify which ones are worth moving in-house into our repo alongside our agents.
Produce a ranked shortlist of candidates to own, with a migration plan for each.

CONTEXT
- claude-stack.html references skills pulled from GitHub. Analyze skills only,
  ignore plugins.
- We want as many relevant skills as practical living on our side, in our repo,
  so we control their content, adapt them to our flow, and strip out what we do
  not need.
- Owning a skill removes the dependency on a remote source that can change or
  disappear, and lets us merge, move, split, and trim freely.
- This is a discovery pass across ALL skills, not a fixed shortlist. Postgres and
  .NET are already being migrated separately - flag them but do not re-do them.

TASKS
1. Enumerate every skill in claude-stack.html. For each report: name, source
   (GitHub repo/path), purpose, approximate token size, and external
   dependencies.
2. Score each skill for migration on:
   - relevance to our stack and agent flow (.NET, Angular, WPF, PostgreSQL, etc.)
   - frequency our agents would actually invoke it
   - how much of the content is useful vs. redundant/over-engineered for us
   - remote-availability risk (how exposed we are if the source changes/vanishes)
   - migration effort (size, dependencies, coupling to other skills)
3. Produce a ranked shortlist: which skills to move in-house now, which later,
   which to skip. Give a clear recommendation per skill with rationale.
4. For each recommended skill, define how it should land in our repo:
   - keep as-is, trim, merge with another skill, or split by responsibility
   - what content to remove as irrelevant to us
   - target single-responsibility shape so it stays cheap to modify later
5. Map source skills -> owned skills after any merge/move/split decisions.
6. Estimate total token savings from the trimmed/consolidated in-house set vs.
   pulling the originals.
7. Specify where each agent should reference each migrated skill.
8. Note what upstream value we lose by forking each skill and propose a
   lightweight way to keep the owned copies current.

CONSTRAINTS
- Target: owned skills in our repo, no runtime dependency on remote/GitHub skills.
- Prioritize by value-to-effort - do not recommend migrating skills we will rarely
  use just because they exist.
- Optimize for low token footprint: trim, split, or merge wherever it does not
  reduce utility.
- Keep each owned skill single-purpose so merging and splitting stay cheap.

DELIVERABLE
- Full skill inventory table: skill -> source -> size -> dependencies.
- Scoring table across the criteria in task 2.
- Ranked migration shortlist: now / later / skip, with rationale.
- Target structure map: source skills -> owned skills (as-is/trim/merge/split).
- Per-skill migration notes (what to keep, what to remove, target shape).
- Token savings estimate.
- Agent-to-skill reference map.


## Summary of finished job from previous agent

HANDOFF: third-party skill forking migration

Mission

Fork useful third-party Claude/Cursor skills (from claude-stack.html's remote inventory) into project-owned skills in this repo (envoydev/agents-stack), so there's no runtime dependency on remote GitHub skills (a remote skill can vanish). Trim
aggressively, keep each skill single-purpose, drop what's off-stack. The stack targets .NET (ASP.NET Core, WPF), Angular 17+, Ionic/Capacitor, PostgreSQL + SQLite, EF Core + NHibernate, DevOps (Docker + GitHub Actions).

A full discovery pass across all 26 remote skills was done (ranked NOW/LATER/SKIP with per-skill rationale). The NOW + SKIP tiers are now executed and lint-green. Only the LATER tier remains.

Current state (verify first)

cd <repo-root> && npm run lint
# EXPECT: lint-skills: clean (49 skills, 58 active manifest entries, 8 plugins, 7 MCPs; 4 manifests + HTML in sync).
Not committed — working tree only. Owned skills install from GitHub main via npx skills add, so nothing reaches consuming projects until committed + pushed (do this on a branch per the repo git rule; the user hasn't asked yet).

DONE (all lint-green)

New owned skills created:
- skills/markdown-style/ (SKILL.md + references/syntax-canon.md, style-overlay.md) — from josiahsiegel doc-master. Was a dangling dep (claude/rules/markdown-docs.md + cursor/AGENTS.template.md routed to it by name). Flipped manifest
josiahsiegel→envoydev, moved to personal block.
- skills/ilspy-decompile/ — standalone, kept allowed-tools: Bash(dnx:*). From aaronontheweb.
- skills/dotnet-project-setup/ (SKILL.md + references/central-package-management.md, local-tools.md) — folded aaronontheweb's project-structure + package-management + local-tools.
- skills/dotnet-performance/ (SKILL.md + references/type-design.md, serialization.md) — folded aaronontheweb's type-design-performance + serialization (Akka cut).
- skills/dotnet-testing/references/ — aspire-integration-testing.md + snapshot-testing.md (dotnet-testing gained metadata.sources).

Fold-ins into existing skills:
- dotnet-web-backend — added "Typed options and startup validation" section (from microsoft-extensions-configuration).
- ionic — added "The Angular zone boundary" nucleus (NgZone.run() listener rule + Android back-button) from capacitor-angular. NOTE: platform-detection / deep-links / listener-lifecycle already existed in ionic — not re-stated.
- devops — docker hardening deltas (seccomp/AppArmor via --security-opt, --pids-limit, BuildKit # syntax= pin + cosign/SBOM caveat, network internal:true/localhost-bind/icc:false), harvested from docker-security-guide.
- dotnet-code-quality — "Reward-hacking shortcuts to reject" 6-row checklist (harvested from slopwatch; no external CLI).

Remote skills removed from all 4 manifests + both HTMLs: api-design's siblings project-structure, package-management, dotnet-local-tools, type-design-performance, serialization, aspire-integration-testing, snapshot-testing,
microsoft-extensions-configuration (folded); docker-platform-guide, docker-security-guide, git-master, r3-reactive-extensions, dotnet-slopwatch, capacitor-plugins, capacitor-angular (dropped/folded). josiahsiegel repo fully gone;
capawesome-team down to ionic-angular only.

All backticked references to removed names were repointed — dotnet/mobile routers, dotnet-migrate, csharp, dotnet-aspire, dotnet-realtime, dotnet-messaging, both CLAUDE.template.md/AGENTS.template.md, and agents (greenfield-solution-designer,
dotnet-build-error-resolver ×2, framework-upgrade-planner, dotnet-test-failure-resolver ×2). Decorative HTML routing-diagram chips updated too.

DEFERRED (decision, not an oversight)

api-design was scored SKIP (OSS/library binary-compat content, off app-stack) but is actively wired into the contract-design flow (dotnet-web-backend:60, cross-stack-contract-designer, aspnet-solution-designer,
dotnet-architecture/references/microservices.md) with no owned replacement. Left as a remote entry to avoid stranding refs. To resolve: either author a small owned contract-versioning home (the ~15-20% useful slice: API-approval-testing via
PublicApiGenerator+Verify, deprecation, no-silent-breaking-changes) and repoint, or keep it remote.

LEFT — LATER tier (still remote manifest entries; 9 remain = 8 LATER + api-design)

Each has a planned home. When forking, follow the process rules below.

┌─────────────────────────────────────────────┬───────────────────────────────────────┬───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│            Remote skill (source)            │             Planned home              │                                                                         Notes                                                                         │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ microbenchmarking (dotnet/skills)           │ new dotnet-diagnostics hub            │ pair with dump-collect; trim NativeAOT + deep-tuning refs (~46K ref tree upstream)                                                                    │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ dump-collect (dotnet/skills)                │ new dotnet-diagnostics hub            │ drop NativeAOT branch/nativeaot-dumps.md; keep coreclr + container refs                                                                               │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ OpenTelemetry-NET-Instrumentation           │ dotnet-web-backend →                  │ distill 23K→~4.5K; DROP sdk-resources-and-logs-reference.md (dupes Aspire wiring); keep only manual-span/metric authoring nucleus (guards, SpanKind   │
│ (aaronontheweb)                             │ dotnet-observability ref              │ table, instrument-selection tree, cardinality, TagList)                                                                                               │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ testcontainers-integration-tests            │ dotnet-testing ref                    │ Postgres+Respawn slice ONLY; needs rewrite to modern module-builder API (upstream uses pre-3.0 generic builder). Respawn already defined in           │
│ (aaronontheweb)                             │                                       │ references/aspire-integration-testing.md — don't duplicate                                                                                            │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ crap-analysis (aaronontheweb)               │ dotnet-code-quality ref               │ keep coverage.runsettings + ReportGenerator Risk-Hotspots; drop Azure Pipelines + Blazor excludes + theory tables                                     │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ csharp-concurrency-patterns (aaronontheweb) │ dotnet-hosted-services                │ gutted nucleus only — strip ALL Akka/Rx (its ref file is ~80% Akka); Channels core already in dotnet-hosted-services                                  │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ dependency-injection-patterns               │ csharp                                │ only the Add* composition + keyed/factory nucleus; rest dupes csharp + dotnet-hosted-services                                                         │
│ (aaronontheweb)                             │                                       │                                                                                                                                                       │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ ionic-angular (capawesome)                  │ ionic                                 │ only Ionic-lifecycle + NavController/tab-routing nucleus; drop the legacy-NgModule half (~20% useful); rewrite Angular-17-standalone-only             │
├─────────────────────────────────────────────┼───────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ api-design (aaronontheweb)                  │ DEFERRED (see above)                  │                                                                                                                                                       │
└─────────────────────────────────────────────┴───────────────────────────────────────┴───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

Process rules (the repo enforces these — read before editing)

Upstream sources: were cloned to a session scratchpad (now gone). Re-clone to read them:
mkdir -p /tmp/skill-src && cd /tmp/skill-src
for r in aaronontheweb/dotnet-skills dotnet/skills capawesome-team/skills; do git clone --depth 1 "https://github.com/$r.git" "$(echo $r|tr / _)"; done
# skill dirs: aaronontheweb_dotnet-skills/skills/<name>/, dotnet_skills/plugins/dotnet-diag/skills/<name>/, capawesome-team_skills/skills/ionic-angular/

4-way parity + lint (scripts/lint-skills.js, run npm run lint): SKILLS manifest must be identical and SAME ORDER across claude/claude-stack.{sh,ps1} + cursor/cursor-stack.{sh,ps1}. Fix the SOURCE (installers/skills), never generated
.mcp.json/.cursor/ output.

Manifest entry format: .sh uses double quotes "repo|skill", .ps1 single quotes 'repo|skill', both with a trailing # comment. Owned = envoydev/agents-stack|<skill> (personal block); remote = <owner>/<repo>|<skill>.

Backtick lint rule (critical): regex /`([A-Za-z][A-Za-z0-9]*(?:-[A-Za-z0-9]+)+)`/ scans SKILL.md and references/*.md and agents/rules/templates. Every backticked token that starts with a letter and has an internal hyphen must resolve to a
known skill (local dir OR active/commented manifest entry) or be in NON_SKILL_TOKENS. SAFE (not matched): leading-dash flags (--pids-limit), tokens with //./space (references/x.md, Directory.Build.props), single words (NgZone), underscores
(pids_limit). So: never backtick a hyphenated multi-word concept that isn't a skill — write it as plain prose.

HTML inventories (claude/claude-stack.html, cursor/cursor-stack.html): const personal = { rows: [ [name, area, description], ... ] } (3-tuple, source implicit) vs const repository = [ [name, area, sourceUrl, description], ... ] (4-tuple). Lint
checks: every local skill dir ↔ a personal row (both HTMLs); every active third-party manifest skill ↔ a repository row.

Counts (lint-enforced): active manifest size must equal the ps1 every skill (N) comment (both ps1 files) and the README Skills count (claude/README.md | **Skills** | N | cell + cursor/README.md **Skills** (N)).

README (README.md root): every local skill dir needs an "Available skills" bullet.

Two operational gotchas hit this session:
1. zsh does NOT word-split unquoted vars — pass file lists explicitly to perl/tools, not via $VAR.
2. Subagent-drafted content arrives HTML-escaped in task notifications (&lt;, &gt;, =&gt;). When integrating, either have the subagent Write files directly (it has real chars) or convert entities before writing — never paste the escaped form
into files.

Patterns:
- New top-level skill: create skills/<name>/SKILL.md (+references/), add envoydev manifest entry in the personal block (all 4, same position), add HTML personal row (both), add README bullet, add metadata.sources provenance to the SKILL.md.
- Fold-in: add content to an existing skill (as a section or references/x.md), repoint all backticked refs to it, REMOVE the source's remote manifest entry (all 4) + HTML repository row (both). No README/HTML-personal add (no new dir).
- Drop: remove remote manifest entry (all 4) + HTML repository row (both); repoint/reword every backticked ref first (else lint fails).

House voice: single hyphens (no em/en dashes), single quotes in prose, direct/lean, recommend one option with a reason. Public repo — no private names/absolute personal paths.

Recommended next step for the incoming agent

Start with the dotnet-diagnostics hub (microbenchmarking + dump-collect) — it's the cleanest LATER item (two dotnet/skills sources, clear trims). Then OTel distillation. Verify npm run lint stays clean after each item.