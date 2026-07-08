# .NET performance on .NET Framework 4.8

The type-design and serialization guidance in SKILL.md and its references applies on net48, with two
Framework caveats: the fast-path primitives are NuGet packages, and one of them is not actually fast
here. Measure first, as always.

## Span is a 'slow span' on 4.8

- `Span<T>` / `Memory<T>` come from the `System.Memory` NuGet package (net462+). Microsoft deliberately
  kept `Span<T>` out of the 4.8 runtime, so it lacks the JIT intrinsics and the `ByReference<T>` / GC
  support that make it fast on .NET Core 2.1+. You get the API and the safety, not the speed - and many
  BCL methods on 4.8 have no span-based overload, forcing conversions. Benchmark before assuming a span
  win; do not port a hot path to spans on 4.8 expecting modern numbers.

## Allocation-reduction tools, via NuGet

- `ArrayPool<T>` (`System.Buffers`) - rent and return large buffers to dodge the Large Object Heap
  (>= 85,000 bytes lands on the LOH, collected only in expensive Gen 2). Return in a `finally`; never
  trust the returned length (it may exceed what you asked for).
- `Microsoft.IO.RecyclableMemoryStream` - a pooling `MemoryStream` replacement; keep one
  process-lifetime manager. Ideal for serialization and network buffers.
- `System.Text.Json` (net462+) is faster and lower-allocation than Newtonsoft, but its edge is smaller
  on 4.8 than on modern .NET. Migrate the hot path only, guided by profiling - do not rip-and-replace a
  mature Newtonsoft codebase (the trade-off is SKILL.md's serialization reference).
- `readonly struct` plus `in` parameters are pure-compiler wins and apply unchanged on 4.8.

## Benchmark on 4.8 itself

- BenchmarkDotNet targets `net48` directly - measure there, because the Framework JIT differs enough
  from modern .NET that a modern-runtime benchmark does not transfer. The BenchmarkDotNet mechanics are
  `dotnet-diagnostics`'.
