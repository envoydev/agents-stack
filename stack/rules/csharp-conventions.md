---
paths: ["**/*.cs"]
---

Editing C# - load `csharp` before the edit - skip the load when it is already in context (some seats preload it); conventions are the source of truth, not recall. This is the C# baseline for every `.cs` file, backend or desktop - a WPF view-model is still C#, so it loads here too, while WPF's .xaml view layer is governed separately. Skip one-line tweaks.
