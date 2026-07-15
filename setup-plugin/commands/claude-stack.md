---
description: Install or update the personal Claude Code stack (routes to the setup or configure skill).
---

Route by state, then follow the chosen skill exactly - never skip its selection-review or prerequisite gates:

- Nothing installed here (no populated `.claude/skills` / `.claude/agents`, and no global install to target) -> invoke the `setup` skill (fresh install from scratch).
- The stack is already installed (project or global) -> invoke the `configure` skill (update: refresh, add, or drop).

When both readings are plausible, ask the user which they want instead of guessing.
