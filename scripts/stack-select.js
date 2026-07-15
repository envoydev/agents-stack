#!/usr/bin/env node
// The deterministic brain of the setup skill: expand a raw skills/agents/rules
// selection into a dependency-complete install set (computeClosure), and check
// a curated prerequisite map against a detected environment (evaluatePrereqs).
// Reads the committed stack-graph.json (Component A); emits an installer
// selection file for claude-stack.sh --selection (Component B).
'use strict';
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

// Expand raw = { skills?, agents?, rules? } into the dependency-complete set.
// Edges (skills never pull skills): rule -> skill/agent, agent -> skill/agent
// (to fixpoint), then every kept skill/agent/rule -> its mcps/plugins.
function computeClosure(graph, raw)
{
    const skills = new Set(raw.skills || []);
    const agents = new Set(raw.agents || []);
    const rules = new Set(raw.rules || []);
    const reasons = {};
    const note = (name, why) => { if (!reasons[name]) reasons[name] = why; };

    for (const r of rules)
    {
        const node = graph.rules[r];
        if (!node) continue;
        for (const s of node.skills) if (!skills.has(s)) { skills.add(s); note(s, `required by rule ${r}`); }
        for (const a of node.agents) if (!agents.has(a)) { agents.add(a); note(a, `required by rule ${r}`); }
    }

    const queue = [...agents];
    while (queue.length)
    {
        const a = queue.shift();
        const node = graph.agents[a];
        if (!node) continue;
        for (const s of node.skills) if (!skills.has(s)) { skills.add(s); note(s, `required by agent ${a}`); }
        for (const a2 of node.agents) if (!agents.has(a2)) { agents.add(a2); note(a2, `required by agent ${a}`); queue.push(a2); }
    }

    const mcps = new Set();
    const plugins = new Set();
    const pull = (node, why) =>
    {
        if (!node) return;
        for (const m of node.mcps) if (!mcps.has(m)) { mcps.add(m); note(m, why); }
        for (const p of node.plugins) if (!plugins.has(p)) { plugins.add(p); note(p, why); }
    };
    for (const s of skills) pull(graph.skills[s], `required by skill ${s}`);
    for (const a of agents) pull(graph.agents[a], `required by agent ${a}`);
    for (const r of rules) pull(graph.rules[r], `required by rule ${r}`);

    const sort = set => [...set].sort();
    return { skills: sort(skills), agents: sort(agents), rules: sort(rules), mcps: sort(mcps), plugins: sort(plugins), reasons };
}

// Phase 1 - always required, a miss is a hard blocker.
const HARD_PREREQS = [
    { bin: 'node', need: 'Node.js', how: 'install Node.js (https://nodejs.org)' },
    { bin: 'git', need: 'git', how: 'install git' },
    { bin: 'claude', need: 'Claude Code CLI', how: 'npm install -g @anthropic-ai/claude-code' },
    { bin: 'uvx', need: 'uv (uvx)', how: 'install uv (https://docs.astral.sh/uv/)' },
];

// Phase 2 - only checked when 'when' matches the closed selection / options.
// severity: 'blocker' (a kept item will not work) or 'warning' (soft/optional).
const SCOPED_PREREQS = [
    { when: { mcp: 'sentry' }, env: 'SENTRY_ACCESS_TOKEN', severity: 'blocker', need: 'Sentry token', how: 'export SENTRY_ACCESS_TOKEN=...' },
    { when: { plugin: 'csharp-lsp' }, bin: 'csharp-ls', severity: 'blocker', need: 'csharp-ls tool', how: 'dotnet tool install -g csharp-ls' },
    { when: { skillPrefix: 'dotnet' }, bin: 'dotnet', severity: 'blocker', need: '.NET SDK', how: 'install the .NET SDK (https://dotnet.microsoft.com)' },
    { when: { skillPrefix: 'csharp' }, bin: 'dotnet', severity: 'blocker', need: '.NET SDK', how: 'install the .NET SDK (https://dotnet.microsoft.com)' },
    { when: { mcp: 'chrome-devtools' }, bin: 'chrome', severity: 'warning', need: 'Chrome / Chromium', how: 'install Google Chrome or Chromium' },
    { when: { mcp: 'appium-mcp' }, bin: 'appium', severity: 'warning', need: 'Appium + native SDKs', how: 'install Appium and the Xcode / Android SDK / Java toolchain' },
    { when: { option: 'context7Local' }, env: 'CONTEXT7_API_KEY', severity: 'warning', need: 'context7 API key', how: 'export CONTEXT7_API_KEY=... (or use --context7 remote)' },
    { when: { option: 'githubCli' }, bin: 'brew', severity: 'warning', need: 'Homebrew', how: 'install Homebrew to auto-install the GitHub CLI (macOS)' },
];

function evaluatePrereqs(selection, env, options)
{
    options = options || {};
    const bins = env.bins || {};
    const envs = env.envs || {};
    const skills = new Set(selection.skills || []);
    const mcps = new Set(selection.mcps || []);
    const plugins = new Set(selection.plugins || []);

    const matches = when =>
    {
        if (when.mcp) return mcps.has(when.mcp);
        if (when.plugin) return plugins.has(when.plugin);
        if (when.skillPrefix) return [...skills].some(s => s.startsWith(when.skillPrefix));
        if (when.option) return !!options[when.option];
        return false;
    };

    const blockers = [];
    const warnings = [];
    const seen = new Set();
    const add = (bucket, p) =>
    {
        const key = p.need;
        if (seen.has(key)) return;
        seen.add(key);
        bucket.push({ need: p.need, how: p.how, ...(p.because ? { because: p.because } : {}) });
    };

    for (const p of HARD_PREREQS)
    {
        if (!bins[p.bin]) add(blockers, p);
    }

    for (const p of SCOPED_PREREQS)
    {
        if (!matches(p.when)) continue;
        const present = p.env ? envs[p.env] : bins[p.bin];
        if (present) continue;
        add(p.severity === 'blocker' ? blockers : warnings, p);
    }

    return { blockers, warnings, ok: blockers.length === 0 };
}

module.exports = { computeClosure, evaluatePrereqs, HARD_PREREQS, SCOPED_PREREQS };
