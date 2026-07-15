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

module.exports = { computeClosure };
