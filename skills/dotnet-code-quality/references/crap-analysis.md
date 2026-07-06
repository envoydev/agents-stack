# CRAP-score risk hotspots

Finding the methods most dangerous to change - high complexity, low coverage - and ranking them so tests land where they pay. Whether coverage gates a build, and the reward-hacks around coverage thresholds, live in the parent skill; this file owns only the risk-hotspot pipeline.

## What CRAP is

CRAP (Change Risk Anti-Patterns) multiplies a method's cyclomatic complexity by the square of its uncovered fraction: `complexity x (1 - coverage)^2`. A method that is both branch-heavy and untested is the top risk, because it is hard to reason about and nothing catches a regression when you touch it. The squared term is the point: coverage dominates. A complexity-32 method at 0% coverage scores 32; the same method at 85% coverage scores about 0.7. So the score ranks refactor/test targets by danger, not by raw size - a simple getter at 0% coverage is harmless (complexity 1) and never surfaces, while a thicket of branches with no tests floats to the top.

## Generate it

CRAP needs per-method cyclomatic complexity, which cobertura alone does not carry - the OpenCover format does. Emit both from coverlet, then let ReportGenerator compute the hotspots. A minimal `coverage.runsettings` at the repo root:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <!-- opencover carries the cyclomatic complexity CRAP needs -->
          <Format>cobertura,opencover</Format>
          <Exclude>[*.Tests]*,[*.Benchmark]*,[*.Migrations]*</Exclude>
          <ExcludeByAttribute>GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
          <ExcludeByFile>**/obj/**/*,**/*.g.cs,**/*.designer.cs,**/Migrations/**/*</ExcludeByFile>
          <IncludeTestAssembly>false</IncludeTestAssembly>
          <SkipAutoProps>true</SkipAutoProps>       <!-- don't count trivial auto-property branches -->
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

Install ReportGenerator as a local tool (pin it in `.config/dotnet-tools.json`, never global - same reproducibility rule as CSharpier), run the tests, then generate:

```bash
dotnet tool install dotnet-reportgenerator-globaltool   # or 'dotnet tool restore' if already pinned

dotnet test --settings coverage.runsettings \
  --collect:"XPlat Code Coverage" --results-directory ./TestResults

dotnet reportgenerator \
  -reports:"TestResults/**/coverage.opencover.xml" \
  -targetdir:"coverage" \
  -reporttypes:"Html;MarkdownSummaryGithub"
```

Point `-reports` at the `coverage.opencover.xml` file, not the cobertura one - the hotspot table needs the complexity metrics only OpenCover carries. Open `coverage/index.html` and find the Risk Hotspots section: methods sorted by CRAP, each row showing cyclomatic complexity, coverage, and the resulting score.

## Read it

A hotspot table reads top-down as a work queue:

```
Method                        Complexity  Coverage  CRAP
AuthService.ValidateToken()   32          0%        32.0   <- test now
DataImporter.ParseRecord()    54          52%       12.4   <- acceptable
OrderProcessor.Calculate()    28          85%       0.6    <- safe to change
```

Act on CRAP above 30, and treat it as a stop-ship on any method you are about to modify. The two ways down are the two factors: write tests to raise coverage (the cheaper move - it shrinks the squared term fastest), or refactor to cut complexity when the method is genuinely tangled. A high-complexity method that already has strong coverage (`ParseRecord` above) is not urgent - the tests make it safe to touch. Prioritize the top of the list; do not chase a uniform coverage percentage, chase the risky methods.

| CRAP | Read |
|---|---|
| < 5 | well-tested or trivial - leave it |
| 5 - 30 | acceptable, but watch complexity growth |
| > 30 | test or refactor before changing it |
