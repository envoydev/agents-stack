#!/usr/bin/env node
'use strict';

// instrument-tool-usage.js - OPT-IN PreToolUse instrumentation (NOT wired by default).
//
// Why: the orchestrator cannot see which Skill / MCP a dispatched subagent loaded or
// called - only that subagent's aggregate token/tool_use totals. That makes a real run's
// skill / MCP / hook usage un-auditable (an audit or benchmark can only ASSESS it, not
// MEASURE it). This hook logs every `Skill` and `mcp__*` tool call as one JSONL line so a
// run can be tallied exactly. It NEVER blocks a call - it observes and exits 0.
//
// It is deliberately NOT in the installer's HOOKS=() array (so it costs nothing by
// default) and is inert unless STACK_INSTRUMENT is set. To enable it for a benchmark /
// audit run, see claude/README.md ('Optional: tool-usage instrumentation'):
//   1. place this file at .claude/hooks/instrument-tool-usage.js
//   2. add a PreToolUse hook wired to it with matcher "Skill|mcp__.*"
//   3. run with STACK_INSTRUMENT=1 (optionally STACK_INSTRUMENT_LOG=<path>)
//
// Output: one JSONL row per matched call at
//   $STACK_INSTRUMENT_LOG  (default: <project>/.claude/tool-usage.<session>.jsonl)
// Coverage note: PreToolUse fires for the session's tool calls; where the running Claude
// Code build propagates PreToolUse into dispatched subagents, their internal Skill / MCP
// calls are captured too - verify coverage against a known run before trusting a tally.

if (!process.env.STACK_INSTRUMENT) process.exit(0); // opt-in: default no-op, zero overhead

let raw = '';
process.stdin.on('data', (d) => (raw += d));
process.stdin.on('end', () => {
  try {
    const ev = JSON.parse(raw || '{}');
    const tool = ev.tool_name || '';
    if (tool === 'Skill' || tool.startsWith('mcp__')) {
      const input = ev.tool_input || {};
      const rec = {
        ts: new Date().toISOString(),
        session: ev.session_id || null,
        tool,
        // the skill slug for Skill, the server for an mcp__<server>__<method> call - never the payload
        detail:
          tool === 'Skill'
            ? input.skill || input.name || null
            : (tool.split('__')[1] || null),
        cwd: ev.cwd || null,
      };
      const path = require('path');
      const fs = require('fs');
      const dir = process.env.CLAUDE_PROJECT_DIR || ev.cwd || '.';
      const sid = String(ev.session_id || 'session').slice(0, 12);
      const out =
        process.env.STACK_INSTRUMENT_LOG ||
        path.join(dir, '.claude', `tool-usage.${sid}.jsonl`);
      fs.mkdirSync(path.dirname(out), { recursive: true });
      fs.appendFileSync(out, JSON.stringify(rec) + '\n');
    }
  } catch {
    // never break a tool call because instrumentation hiccuped
  }
  process.exit(0);
});
