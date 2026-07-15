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
