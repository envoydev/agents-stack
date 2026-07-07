# Contract-First Protocol

The shared contract is the source of truth every affected stack builds against. It is frozen before any parallel domain work, versioned, and changed only through an explicit request - never silently.

## Freeze before parallel work

For any cross-domain feature (cross_domain_light or full_cross_domain), the orchestrator must produce an approved, frozen contract before dispatching a single domain designer. cross-stack-contract-designer produces it; the orchestrator freezes it and records the version in the progress ledger. Old results are never verified against a stale contract.

Minimum contract fields:

```yaml
contract_id: feature-name-contract
version: v1
status: frozen
requirements_source: [BA notes, user confirmations]
acceptance_criteria: [...]
affected_domains: [data, backend, angular]
shared_api:
  routes: [{ method: POST, path: /api/... }]
  request_dtos: []
  response_dtos: []
  error_envelope: {}
  pagination: {}
  versioning: {}
auth:
  policies: []
  roles: []
data_contract:
  entities: []
  migrations: []
  indexes: []
  rollback_strategy: ...
frontend_contract:
  states: []
  screens: []
  validation_messages: []
wpf_contract:
  viewmodels: []
  commands: []
  bindings: []
mobile_contract:
  native_permissions: []
  offline_behavior: ...
devops_contract:
  env_vars: []
  secrets: []
  ci_jobs: []
  deployment_order: []
observability:
  logs: []
  metrics: []
  traces: []
out_of_scope: [...]
```

Fill only the domain blocks the feature actually touches. Every element is a concrete named decision, not a description to settle later - vagueness is the exact drift this artifact exists to prevent.

## Versioning

Every task card and every agent result carries `contract_version`. A change goes `v1 -> Contract Change Request -> v2 approved -> affected seats rebase against v2`. Verification runs against the current version only; a result produced against v1 is re-verified against v2, never accepted as-is.

## What counts as a shared-contract change

A local implementation detail can change and continue (report DONE_WITH_CONCERNS if it carries risk). A change to any of these is a SHARED-contract change and must stop:

- Database schema or index semantics.
- API routes, verbs, request/response DTOs.
- Error codes and the error envelope.
- Auth / authorization policies.
- Event or message contracts.
- Config / env vars.
- Migration or deployment order.
- Frontend / WPF / mobile-visible behavior.

## The change protocol

No seat may silently change a shared contract. When a seat finds the frozen contract cannot be met, it stops and emits BLOCKED_CONTRACT_CHANGE with a Contract Change Request:

```yaml
status: BLOCKED_CONTRACT_CHANGE
agent: data-implementer
task_id: data-02
contract_version: v1
summary: "Unique-email design conflicts with soft-delete requirement."
current_plan_problem:
  - "UNIQUE(email) prevents reusing an email after soft delete."
recommended_change:
  - "Partial unique index WHERE deleted_at IS NULL."
affected_domains: [backend, data, devops]
possibly_affected_domains: [angular]
risk_if_ignored:
  - "Backend duplicate check will not match database semantics."
options:
  A: "Partial unique index for active users only."
  B: "Email can never be reused, even after soft delete."
  C: "Remove soft delete for users."
recommendation: A
files_or_symbols_involved: [Users table, CreateUser endpoint, UserEmailAlreadyExists error]
```

## Orchestrator reaction to a contract change

When any lane returns BLOCKED_CONTRACT_CHANGE:

1. Mark the task blocked in the ledger.
2. Identify the affected domains.
3. Pause ONLY the affected lanes - unrelated domains that are safe keep running.
4. Re-run task-analyzer / architecture-analyzer and cross-stack-contract-designer as needed; consult the affected domain designers.
5. Produce Contract v2 and freeze it.
6. Broadcast v2 to the affected seats.
7. Require each affected seat to report: can continue, needs rework, or must discard stale work.
8. Verify against v2 only.

The integration-reviewer confirms at the final gate that the current version is used everywhere - a lane that signed off against a superseded version fails the gate.
