---
name: project-task-cycle
description: "Use to run a task, feature, or bug through the whole single-chat vertical with a hard user gate between every step: design (project-solution-design writes the plan file) -> plan audit (project-verify-plan) -> user approval + build-mode choice -> build (project-implementer in-session by default, domain implementer agents on explicit ask) -> plan-conformance verify (skippable) -> /code-review + done-gate. Every stop is a real pause - switch model or effort, add context, or edit the plan before saying go - and the plan file plus a serena cycle note make every step resumable after compaction or in a fresh session. Trigger on run the task cycle, build this with approvals, gated implementation, step-by-step with my sign-off. Not the dispatched multi-agent flow (project-task-flow - trios, cross-domain), not greenfield (project-build-from-scratch), not a one-line edit."
disable-model-invocation: true
---

# Task Cycle - the gated single-chat vertical

One task/feature/bug, six steps, and the user holds the gate between every two. The three twin
skills do the work; this skill owns the chain, the stops, the mode choices, and the state that
survives a compaction or a fresh session. It never designs, builds, or reviews anything itself.

## State - two layers, split by durability

- **The plan file** (`<docs-root>/superpowers/plans/<feature>.md`, docs root per the project's
  CLAUDE.md) is the durable truth: the tasks, every stamp this cycle adds (`Gated`, `Approved` +
  build mode, `Conformance` verdict or `skipped`, `Completed`), per-task status + evidence. On any
  conflict with memory or the chat, the file wins.
- **The serena cycle note** (`write_memory` named `<feature>__cycle`) is the working cursor:
  current step, chosen modes, resume pointer (plan path + next task), any mid-task scratch worth
  carrying. Update it at EVERY stop and after every task tick; it is never more than one step
  stale when compaction hits. Local and disposable - everything essential is in the plan file.

**On invocation, resume before starting:** `list_memories` -> `read_memory` the feature's cycle
note, and read the plan file's stamps. A cycle mid-flight resumes at its cursor - never restart a
step whose stamp says it already passed.

## The stop contract

At each stop: report one line of result, the artifact path, and what the next step will be - then
END THE TURN and wait. The stop is the user's window to switch model or effort, paste context, or
edit the plan file directly. Proceed only on their explicit word; silence is not a go.

## The steps

1. **DESIGN** - run `project-solution-design`. It writes the plan to the plans folder above; the
   file, not the chat, is the artifact. *Stop.*
2. **GATE** - run `project-verify-plan` over the plan file. It stamps `Gated: passed` or the gaps
   found. Gaps route back to step 1 on the user's word. *Stop.*
3. **APPROVE** - present the gated plan and ask two things in one gate: approval to build, and the
   build mode - **session** (default: the build runs in this chat) or **agents** (each task
   dispatched to its stack's `<stack>-implementer` seat). Stamp `Approved: <date> - mode <session|agents>`
   into the plan file. Nothing builds without this stamp. Agents mode exists only where subagent
   dispatch is available; otherwise session is the only mode - say so rather than pretending.
4. **BUILD** - per the approved mode:
   - *session*: run `project-implementer` - it marks each task `IN_PROGRESS` before code, ticks it
     `DONE` with evidence after its green gate, and keeps the plan's resume note current.
   - *agents*: fan the plan's task cards out to the matching `<stack>-implementer` seats - flat
     fan-out, the main session is the only orchestrator; a red build/test routes per the
     repair-agent rules; tick the same plan file per task as reports land.
   *Stop* - and this stop doubles as the conformance decision: run the verify in-session
   (default), dispatch the stack's `<stack>-verifier` for independent eyes, or **skip** straight
   to step 6. The user can inspect the diff themselves here first.
5. **CONFORMANCE** (unless skipped - a skip is stamped `Conformance: skipped by user`, an honest
   record, not a silent gap) - audit the code against the plan file: every task present, nothing
   built outside a task's boundary, each acceptance criterion DEMONSTRATED the way
   `superpowers:verification-before-completion` prescribes - by a run in this session, quoted,
   never assumed from reading the diff. Deviations become a punch list routed back to step 4.
   Stamp the verdict. *Stop.*
6. **CLOSE** - `/code-review` over the assembled diff, pointed at the plan file so it reviews
   against the plan, not in isolation; apply the findings; then the done-gate
   (`superpowers:verification-before-completion` on the whole feature). Stamp
   `Completed: <date>` with the per-task evidence table. Delete or archive the cycle note.

## Do not

- Never pass a stop without the user's explicit word, and never approve the plan yourself - the
  APPROVE stamp records the user's decision, not yours.
- Never dispatch a seat the user did not choose at a stop - dispatch is explicit-only house-wide.
- Never keep cycle state only in chat: a stamp or tick that is not in the plan file does not
  exist. The serena note is a cursor, never the truth.
- Never re-run a stamped step on resume; pick up at the cursor.
