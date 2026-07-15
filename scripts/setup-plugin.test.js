'use strict';
const test = require('node:test');
const assert = require('node:assert');
const fs = require('node:fs');
const path = require('node:path');

const ROOT = path.join(__dirname, '..');
const PLUGIN_DIR = path.join(ROOT, 'claude', 'setup-plugin');

test('marketplace.json is valid and points at the setup-plugin subdir', () => {
    const mp = JSON.parse(fs.readFileSync(path.join(ROOT, '.claude-plugin', 'marketplace.json'), 'utf8'));
    assert.strictEqual(mp.name, 'agents-stack');
    assert.ok(Array.isArray(mp.plugins) && mp.plugins.length === 1);
    const p = mp.plugins[0];
    assert.strictEqual(p.name, 'claude-stack-setup');
    assert.strictEqual(p.source, './claude/setup-plugin');
    assert.ok(typeof p.description === 'string' && p.description.trim() !== '');
});

test('plugin.json is valid and the command exists', () => {
    const pj = JSON.parse(fs.readFileSync(path.join(PLUGIN_DIR, '.claude-plugin', 'plugin.json'), 'utf8'));
    assert.strictEqual(pj.name, 'claude-stack-setup');
    assert.ok(typeof pj.version === 'string' && pj.version.trim() !== '');
    assert.ok(typeof pj.description === 'string' && pj.description.trim() !== '');
    assert.ok(fs.existsSync(path.join(PLUGIN_DIR, 'commands', 'claude-stack.md')), 'the /claude-stack command exists');
});
// Note: the bundled SKILL.md is asserted in Task 3's test additions (it lands there).

test('no tracked plugin file leaks an email address', () => {
    for (const rel of ['.claude-plugin/marketplace.json', 'claude/setup-plugin/.claude-plugin/plugin.json'])
    {
        const text = fs.readFileSync(path.join(ROOT, rel), 'utf8');
        assert.ok(!/@[a-z0-9.-]+\.[a-z]{2,}/i.test(text.replace(/@agents-stack|@main/g, '')), `${rel} must not contain an email`);
    }
});
