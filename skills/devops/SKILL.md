---
name: devops
description: "Personal DevOps reference for the .NET/Angular house, by the delivery surface a change touches: container builds (multi-stage, cache-ordered layers, non-root, chiseled/distroless .NET images, base images pinned by digest not :latest), Compose local topology (healthchecks, no inline secrets), GitHub Actions CI/CD (a lint/test/build graph fail-fast, lockfile-hash cache keys, real service containers not mocks, masked secrets, actions pinned to a commit SHA, least-privilege permissions, OIDC over stored creds), and safe deploy (environments + approvals, expand-contract migrations gated before the roll, health-gated blue-green/rolling cutover with rollback). Load when authoring or reviewing a Dockerfile, a compose file, a workflow, a deploy pipeline, an env/secret template, or the Aspire AppHost - or when the devops vertical or security-auditor sweeps the delivery stack. Points at dotnet-aspire, dotnet-migrate, and dotnet-security / data-security. Do NOT load for application or schema code."
---

# DevOps - containers, CI/CD, and safe deploys for the .NET/Angular house

The pipeline is production code - a broken workflow blocks every merge and a leaked secret is an incident, not a warning. This is the delivery-surface map for the house stacks (ASP.NET Core, Angular, and their SQL/data layer). It pairs with `dotnet-aspire` (orchestration), `dotnet-migrate` (migration mechanics), and `dotnet-security` / `data-security` (secret handling). The rule under all of it - the build is reproducible, the secret never touches an image or a log, and every deploy is reversible.

## Docker - reproducible, minimal, non-root

- Multi-stage build - an SDK stage compiles and publishes, a slim runtime stage copies only the published output; the SDK image never ships.
- Order the layers for the cache - copy the project and lock files and restore BEFORE copying the source, so a source edit does not bust the restore layer. A Dockerfile that copies everything then restores never hits the cache.
- Mount a persistent package cache in the restore layer - `RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore` (and the npm cache) - so the cache survives even when the copy-lockfile-then-restore layer is busted.
- When a build genuinely needs a secret - a private NuGet-feed PAT during restore - pass it with `RUN --mount=type=secret,id=...` so it never lands in a layer or image history, distinct from the runtime secrets pulled from the store.
- Pin the base image by digest, never a floating :latest or a bare major tag - a moving tag makes the build non-reproducible and is a supply-chain hole. Prefer a chiseled or distroless .NET runtime image (no shell, minimal CVE surface).
- Build multi-arch images with `buildx --platform linux/amd64,linux/arm64` when developers are on Apple Silicon but production runs x64 - a locally-built image is otherwise the wrong architecture for the server.
- Run as a non-root USER, mount the root filesystem read-only where the app allows, and keep a .dockerignore that excludes bin, obj, node_modules, .git, and every secret-bearing file.
- Harden past non-root at runtime - drop all Linux capabilities (`cap_drop: [ALL]`), set no-new-privileges, and cap memory / CPU, so a compromised or leaking process cannot escalate or starve the host.
- Give the container a HEALTHCHECK and proper PID-1 signal handling (an init shim) so the orchestrator can tell ready from dead and a SIGTERM drains rather than kills.

## Compose - local topology, not a secret store

- Express service dependencies with a health condition, and give every backing service (Postgres, SQL Server, Redis) its own healthcheck, so a dependent waits for ready and not merely started.
- Keep state in named volumes; never bind-mount or inline a secret in the compose file - pull it from a gitignored env-file that never enters source control.

## GitHub Actions - the CI/CD contract

- Structure the graph - a lint/build job and a test job sequenced with needs, fail-fast on lint so a formatting break does not burn a full test run. A matrix covers multiple target frameworks or Node versions.
- Key the cache on a lockfile hash (packages.lock.json, yarn.lock) with restore-keys for partial hits; a cache key that ignores the lockfile serves a stale restore. Restore deterministically - restore in locked mode, npm ci, never a floating install.
- Pin every third-party action to a full commit SHA, not a moving major tag - the tag is mutable, and a compromised action runs with your token.
- Handle secrets as GitHub Secrets only; mask any derived secret before it can reach a log, never echo one, and set a least-privilege permissions block (default read, elevate per job). Federate to the cloud with OIDC (short-lived) rather than a long-lived stored credential.
- Run integration tests against real service containers, not a mock - a suite green against a stub proves nothing about the wired system.
- Add a security-scan stage past the dependency audit - a secret scanner (gitleaks) failing the build on a committed credential, and a Trivy scan of the built image gating CRITICAL/HIGH; run the dependency + image scans on a cron schedule off the PR path too, so a CVE disclosed against an already-merged clean dependency is still caught.
- Set timeout-minutes on every job so a hung step is killed in minutes instead of burning the runner's full default budget.
- Add a concurrency group keyed on workflow + ref with cancel-in-progress: true, so a fast follow-up push cancels the now-stale run instead of queueing behind it.
- Upload diagnostic artifacts on failure only (if: failure()) - test results and logs with a short retention - so a red run is debuggable without a rerun.

## Deploy and release - reversible and health-gated

- Promote one immutable artifact through the environments (with required reviewers on prod); never rebuild per environment, or you ship something you never tested.
- Run migrations as a discrete, gated step BEFORE the app rolls, expand-then-contract so the old and new app versions both work mid-deploy (mechanics in `dotnet-migrate`); every deploy carries a rollback path.
- Cut over health-gated - blue-green, or a rolling update behind readiness checks, never a big-bang replace that routes traffic to a not-ready instance.
- Pull config and secrets at runtime from the store (Key Vault, an OIDC-federated secret) - never bake them into the image (see `dotnet-security`, `data-security`).

## .NET Aspire - orchestration

- The Aspire AppHost is the composition root for the local run and the deployment manifest; service discovery and connection strings flow through it, not hardcoded per service. Depth in `dotnet-aspire`.

## Where the rest lives

Migration reversibility and the expand-contract mechanics are `dotnet-migrate`. Secret and crypto primitives are `dotnet-cryptography`; the app- and data-layer secret-handling surface is `dotnet-security` / `data-security`. The Aspire app-model is `dotnet-aspire`.
