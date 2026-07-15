'use strict';
const test = require('node:test');
const assert = require('node:assert');
const path = require('node:path');
const { computeClosure } = require('./stack-select.js');
const graph = require('./stack-graph.json');

test('an agent pulls its declared skills and plugins', () => {
    const c = computeClosure(graph, { agents: ['aspnet-implementer'] });
    for (const s of ['csharp', 'dotnet-web-backend', 'dotnet-testing'])
    {
        assert.ok(c.skills.includes(s), `expected skill ${s} pulled by aspnet-implementer`);
    }
    assert.ok(c.plugins.includes('ponytail'), 'aspnet-implementer pulls the ponytail plugin');
    assert.match(c.reasons['csharp'], /aspnet-implementer/);
});

test('a rule pulls its skills', () => {
    const c = computeClosure(graph, { rules: ['csharp-conventions'] });
    assert.ok(c.skills.includes('csharp'));
    assert.match(c.reasons['csharp'], /csharp-conventions/);
});

test('a kept skill makes its mcps required', () => {
    const c = computeClosure(graph, { skills: ['project-capabilities'] });
    for (const m of ['serena', 'context7', 'sentry', 'memory', 'playwright', 'angular-cli', 'chrome-devtools', 'appium-mcp'])
    {
        assert.ok(c.mcps.includes(m), `expected mcp ${m} required by project-capabilities`);
    }
});

test('user-chosen items carry no reason; only closure-added ones do', () => {
    const c = computeClosure(graph, { skills: ['csharp'] });
    assert.ok(c.skills.includes('csharp'));
    assert.strictEqual(c.reasons['csharp'], undefined, 'a directly chosen item is not a closure add');
});

test('empty selection yields empty closure', () => {
    const c = computeClosure(graph, {});
    assert.deepStrictEqual(c, { skills: [], agents: [], rules: [], mcps: [], plugins: [], reasons: {} });
});

const { evaluatePrereqs } = require('./stack-select.js');

const fullEnv = { bins: { node: true, npx: true, git: true, claude: true, uvx: true, dotnet: true, 'csharp-ls': true }, envs: { SENTRY_ACCESS_TOKEN: true, CONTEXT7_API_KEY: true } };
const emptyEnv = { bins: {}, envs: {} };

test('phase-1 hard prereqs are blockers when the binary is absent', () => {
    const r = evaluatePrereqs({ skills: [], mcps: [], plugins: [] }, emptyEnv, {});
    const needs = r.blockers.map(b => b.need).join(' ');
    for (const label of ['Node.js', 'git', 'Claude Code CLI', 'uv (uvx)'])
    {
        assert.ok(needs.includes(label), `expected hard blocker ${label}`);
    }
    assert.strictEqual(r.ok, false);
});

test('a selected sentry mcp without its token is a blocker; with it, clean', () => {
    const sel = { skills: [], mcps: ['sentry'], plugins: [] };
    const missing = evaluatePrereqs(sel, { bins: { node: true, npx: true, git: true, claude: true, uvx: true }, envs: {} }, {});
    assert.ok(missing.blockers.some(b => /Sentry/i.test(b.need)), 'sentry token blocker');
    const present = evaluatePrereqs(sel, { bins: { node: true, npx: true, git: true, claude: true, uvx: true }, envs: { SENTRY_ACCESS_TOKEN: true } }, {});
    assert.ok(!present.blockers.some(b => /Sentry/i.test(b.need)), 'sentry token satisfied');
});

test('a .NET skill without the dotnet SDK is a blocker', () => {
    const r = evaluatePrereqs({ skills: ['dotnet-web-backend'], mcps: [], plugins: [] }, { bins: { node: true, npx: true, git: true, claude: true, uvx: true }, envs: {} }, {});
    assert.ok(r.blockers.some(b => /\.NET SDK/.test(b.need)), 'dotnet SDK blocker for a dotnet-* skill');
});

test('full env with no risky selection is clean', () => {
    const r = evaluatePrereqs({ skills: ['csharp'], mcps: [], plugins: [] }, fullEnv, {});
    assert.strictEqual(r.ok, true);
    assert.deepStrictEqual(r.blockers, []);
});

test('chrome-devtools mcp missing Chrome is a warning, not a blocker', () => {
    const r = evaluatePrereqs({ skills: [], mcps: ['chrome-devtools'], plugins: [] }, { bins: { node: true, npx: true, git: true, claude: true, uvx: true }, envs: {} }, {});
    assert.ok(r.warnings.some(w => /Chrome/i.test(w.need)));
    assert.ok(!r.blockers.some(b => /Chrome/i.test(b.need)));
    assert.strictEqual(r.ok, true, 'a warning alone keeps ok true');
});
