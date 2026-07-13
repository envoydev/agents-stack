---
paths: ["**/*.md"]
---

Authoring or restructuring any .md (README, ADR, runbook) - load `markdown-style` first - skip the load when it is already in context; its
own keywords only catch explicit lint asks, so a content edit misses it. Skip one-line tweaks.
