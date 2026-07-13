---
paths: ["**/*.xaml"]
---

Editing WPF XAML - load `dotnet-wpf` before the edit - skip the load when it is already in context (some seats preload it); conventions are the source of truth, not recall. Governs the `.xaml` view + MVVM binding layer; the `.cs` view-models are C#, governed by `csharp`. Skip one-line tweaks.
