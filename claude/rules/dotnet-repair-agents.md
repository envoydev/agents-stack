---
paths: ["**/*.cs", "**/*.csproj", "**/*.sln", "**/*.xaml"]
---

The two .NET repair loops routing - **`dotnet-build-error-resolver`** runs dotnet build,
categorizes CS/NU/MSB/MC errors (MC#### = WPF XAML markup compile), fixes minimally via
serena, rebuilds until clean (5-cycle cap); **`dotnet-test-failure-resolver`** drives dotnet
test red->green fixing the correct side, never gaming a test; use after the build is green.
Default to delegating fix-the-build / make-the-tests-pass to the matching resolver rather
than looping in-session - the subagent absorbs the repeated output and returns only a
diagnosis. Both pinned sonnet/high.
