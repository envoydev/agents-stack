# C# on .NET Framework 4.8

This skill floors at .NET 8 / C# 12. On a .NET Framework 4.8 (net48) codebase the language ceiling and
the async runtime both differ - the deltas are here, and the modern conventions in SKILL.md otherwise
still apply. 4.8 is supported (tied to the Windows lifecycle) but frozen: write to these constraints,
and treat the move to modern .NET as debt reduction (that stance is `dotnet-migrate`'s
`references/net-framework-48.md`).

## Language version ceiling

- C# 7.3 is the last officially supported `<LangVersion>` for net48. You *can* set it higher and it
  works for a well-defined subset - but pin an explicit version (e.g. `8.0` or `9.0`), never `latest`
  or `default`, so a build is not machine-dependent and cannot silently pull a feature that fails at
  runtime. The `<LangVersion>` property itself lives in Directory.Build.props - `dotnet-project-setup`.
- **Compiler-only, safe up to C# 8+:** nullable reference types (`#nullable enable`), switch
  expressions and pattern matching, `using` declarations, static local functions, readonly struct
  members, tuples. NRT works, but the 4.8 BCL is not null-annotated, so you get your own annotations
  without the framework's; the nullable attributes (`[MaybeNull]`, `[NotNullWhen]`) need the `Nullable`
  NuGet package to supply their definitions.
- **Needs a polyfill package:** async streams / `IAsyncEnumerable<T>` and `await using` /
  `IAsyncDisposable` via `Microsoft.Bcl.AsyncInterfaces` (they are interfaces over `ValueTask`, so they
  light up); `Index` / `Range` need hand-defined `System.Index` / `System.Range` types (not shipped for
  net48). Add `System.Linq.Async` for LINQ over async streams.
- **Impossible on net48:** default interface methods, static abstract interface members, and anything
  that ties the language to a newer CLR (C# 8+ features with a runtime dependency). Do not reach for
  them.

## Modern BCL APIs are available - via NuGet, not in-box

- `Span<T>` / `Memory<T>` (`System.Memory`), `System.Text.Json`, and `ValueTask`
  (`System.Threading.Tasks.Extensions`) all target net462+ and run on 4.8. Adopting `IAsyncEnumerable`
  or `ValueTask` on net48 is a package reference plus `<LangVersion>` 8+, not a runtime upgrade.
- You get the API and the safety but not always the speed - which package supplies what, and why
  `Span<T>` is a 'slow span' on 4.8, is `dotnet-performance`'s `references/net-framework-48.md`.

## Async: there IS a SynchronizationContext (unlike the modern floor)

The SKILL.md async rule skips `ConfigureAwait(false)` in ASP.NET Core because it has no
`SynchronizationContext`. On .NET Framework that assumption inverts:

- Classic ASP.NET (`AspNetSynchronizationContext`), WPF, and WinForms each install a real
  single-threaded context, so **`ConfigureAwait(false)` on every library `await` is load-bearing** -
  your defense against the sync-over-async deadlock a blocking caller imposes, not an optimization.
- Blocking on async (`.Result` / `.Wait()` / `.GetAwaiter().GetResult()`) from a context-bound thread
  deadlocks. The mechanism and the `SemaphoreSlim` / cancellation rules around it are
  `dotnet-hosted-services`' `references/concurrency.md`; the fix is always async-all-the-way, never a
  blocking bridge.
- In app-level code (a classic-ASP.NET controller, a UI event handler) keep the default so the
  continuation resumes on the context that owns `HttpContext.Current`, culture, and the UI thread.
  `async void` stays for event handlers only.

Route out: pooling / `Span` speed / serialization -> `dotnet-performance`; packages, `<LangVersion>`,
and runtime config -> `dotnet-project-setup`; the migration path off 4.8 -> `dotnet-migrate`.
