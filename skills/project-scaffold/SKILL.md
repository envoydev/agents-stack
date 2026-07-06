---
name: project-scaffold
description: Build a new application or major module from scratch - greenfield scaffolding and orchestration. Routes a new project to the right architecture skill (dotnet-architecture, dotnet-project-structure) and scaffolding command (dotnet new, ng new via the angular-cli MCP), then in a stack-installed Claude Code project drives the build from the main session - greenfield-solution-designer designs, and each vertical slice runs the `domain-build` skill for its stack. Triggers on build from scratch, new project, greenfield, scaffold, start a new app.
---

# Project Scaffold - Greenfield Build Orchestration

Use this skill to build a new application or a major new module from scratch, before code exists. It routes the work to the right architecture skill and scaffolding command, then - in a stack-installed Claude Code project - drives the build itself, slice by slice, from the main session.

## Execution modes
Pick the mode once, before DESIGN, and hold it for the whole run.

- **DELEGATED** - the default whenever the current session can dispatch subagents (the Agent tool is available). The main session orchestrates: it dispatches greenfield-solution-designer, then runs the `domain-build` skill per slice; it never does their work itself.
- **INLINE** - the fallback when dispatch is unavailable: Cursor, a non-stack project, or a scaffold small enough that dispatch overhead outweighs the work. Do it all in-session, using brainstorming and writing-plans plus the architecture skills directly, instead of dispatching a designer.

Detection keys on dispatch capability, not file presence - a project can carry the agent files on disk with no Agent tool available, in which case it still runs INLINE.

## Steps
1. **DESIGN** - DELEGATED: dispatch greenfield-solution-designer to turn the spec into architecture options. INLINE: brainstorming and writing-plans in-session, to the same end. Either way, get the user's approval on the architecture and the stack before scaffolding anything - greenfield tech choices are the user's, never silently picked.
2. **SCAFFOLD** - once approved, run the named new-project command (dotnet new <template>; ng new via the angular-cli MCP; ionic start for Ionic), establish the structure from the chosen architecture skill, and wire the baseline: DI, config, a test project, and formatter/analyzer config for the stack.
3. **BUILD** - slice by slice: for each vertical slice, run the `domain-build` skill for that slice's stack (it fans out that stack's designer, implementer, and verifier in turn). Loop until the spec's first milestone is met.

## Per-stack scaffolding

| Stack | new-project command | Architecture + convention skills |
|---|---|---|
| Angular web | ng new (angular-cli MCP) | `angular-conventions` + `angular-styling` |
| Ionic/Capacitor mobile | ionic start + cap add | `ionic` + `mobile` |
| ASP.NET Core backend | dotnet new webapi/web | `dotnet-architecture` + `dotnet-web-backend` / `dotnet-minimal-api` |
| WPF desktop | dotnet new wpf | `dotnet-wpf` (strict MVVM) |
| SQL / data | first schema | `database-conventions` + `dotnet-migrate` |

## Rules
- Greenfield architecture and tech choices are the user's - present options, get approval, never scaffold before the design is approved.
- The main session is the only orchestrator - never instruct a subagent to dispatch another; the agents this skill dispatches (greenfield-solution-designer, then the domain seats via the `domain-build` skill) carry no Agent tool.
- Reuse the architecture skills rather than restating structure here - this skill routes, it does not re-derive.
