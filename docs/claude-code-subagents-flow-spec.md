# Claude Code Subagents Flow — Implementation Handoff Spec

Updated after follow-up discussion: execution modes, repo-separated frontend/backend flow, Ponytail/Caveman token policy, token budgeting, and issue-investigation flows are included.

## 1. Executive summary

This design is for a Claude Code **subagent-based engineering workflow**, not an Agent Team workflow. The main Claude Code session/orchestrator owns the lifecycle. It dispatches subagents, keeps the progress ledger, controls contract versions, pauses affected lanes when contracts change, and performs final integration gating.

The core design is:

```text
Requirements → Team Lead / Task Analyzer → Contract Freeze → Parallel Domain Pipelines → Final Integration Gate → Commit
```

The most important constraint is:

```text
Parallel across domains.
Sequential inside one domain.
Parallel only among implementers within the same domain after that domain's solution designer has finished.
```

For example, the backend lifecycle is always:

```text
aspnet-solution-designer → aspnet-implementer(s) → aspnet-verifier
```

The backend designer, backend implementers, and backend verifier should not all run at the same time for the same backend task. The implementers need the design first; the verifier needs the implementation first.

## 2. Problems this design solves

The original diagram had a good separation between BA, Team Lead, implementers, verifiers, and code reviewer, but it had several risks:

1. It looked too waterfall: database → backend → frontend.
2. It did not explicitly handle WPF, Ionic/mobile, and DevOps domains.
3. It did not protect parallel agents from drifting away from the shared contract.
4. It allowed the possibility that one domain silently changes a shared contract while another domain continues implementing the old plan.
5. It risked too much repeated boilerplate if model choices are encoded by duplicated agent variants.
6. It lacked a strict contract-change protocol.
7. It lacked explicit shared policies for model routing, agent output statuses, and progress ledger updates.
8. It did not clearly distinguish domain-level verification from final integration verification.

This spec addresses those issues.

---

## 3. Agent domains

The flow must support six domain pipelines:

| Domain | Designer | Implementer | Verifier |
|---|---|---|---|
| Data / SQL | `data-solution-designer` | `data-implementer` | `data-verifier` |
| ASP.NET Backend / API | `aspnet-solution-designer` | `aspnet-implementer` | `aspnet-verifier` |
| Angular Web | `angular-solution-designer` | `angular-implementer` | `angular-verifier` |
| WPF Desktop | `wpf-solution-designer` | `wpf-implementer` | `wpf-verifier` |
| Ionic / Capacitor Mobile | `mobile-solution-designer` | `mobile-implementer` | `mobile-verifier` |
| DevOps / Delivery | `devops-solution-designer` | `devops-implementer` | `devops-verifier` |

Additional cross-cutting agents:

| Agent | Purpose |
|---|---|
| `task-analyzer` | Classify task, decide affected domains, risk, and orchestration shape. |
| `cross-stack-contract-designer` | Freeze shared routes, DTOs, errors, auth, versioning, DB/API seams, config, and deployment seams before parallel domain work. |
| `security-auditor` | Read-only posture/security audit when auth, authorization, secrets, injection, data exposure, or production risk is involved. |
| `framework-upgrade-planner` | Used only for major framework upgrades or version/deprecation events. |
| `issue-diagnoser` | Reads logs/errors/screenshots and creates a decomposed fix plan; does not write code. |
| `evidence-gatherer` | Cheap read-only support agent for collecting logs, repro evidence, symbols, failing frames, etc. |
| Repair agents | Fix build/test failures after implementation: `dotnet-build-error-resolver`, `dotnet-test-failure-resolver`, `ng-build-error-resolver`, `angular-test-resolver`, `ci-failure-diagnoser`. |

---

## 4. Correct parallelism model

### 4.1 Cross-domain parallelism is allowed

After a contract is frozen, affected domain pipelines may run in parallel:

```text
Contract v1 frozen
  ├─ Data:    designer → implementers → verifier
  ├─ Backend: designer → implementers → verifier
  ├─ Angular: designer → implementers → verifier
  ├─ WPF:     designer → implementers → verifier
  ├─ Mobile:  designer → implementers → verifier
  └─ DevOps:  designer → implementers → verifier
```

### 4.2 Same-domain lifecycle is sequential

Within one domain, phases are sequential:

```text
<domain>-solution-designer
  ↓
<domain>-implementer(s)
  ↓
<domain>-verifier
```

### 4.3 Same-domain implementers can fan out

Within the implementation phase, multiple implementers can run in parallel if each receives a bounded task:

```text
aspnet-solution-designer
  ↓
  ├─ aspnet-implementer #1: endpoint
  ├─ aspnet-implementer #2: service/business logic
  ├─ aspnet-implementer #3: tests
  └─ aspnet-implementer #4: auth/error handling
  ↓
aspnet-verifier
```

### 4.4 Verifiers are parallel by domain but not final approval

Domain verifiers can run in parallel after their domain implementation is complete:

```text
Data verifier + Backend verifier + Angular verifier + WPF verifier + Mobile verifier + DevOps verifier
```

But their approval is not the final approval. A final integration reviewer must check the assembled feature across all domains.

---

## 5. Required shared policy files / skills

To avoid boilerplate, do not create model-specific agent variants like `aspnet-implementer-low`, `aspnet-implementer-medium`, and `aspnet-implementer-high`. Keep one agent per role/domain and move reusable behavior into shared policies.

Create these shared policies/skills:

```text
policies/task-routing-policy.md
policies/domain-flow-router.md
policies/model-routing-policy.md
policies/contract-change-protocol.md
policies/agent-output-protocol.md
policies/progress-ledger-protocol.md
policies/final-integration-gate.md
policies/repo-separation-protocol.md
policies/token-reduction-policy.md
policies/domain-task-card-template.md
policies/contract-template.md
policies/contract-change-request-template.md
policies/verification-report-template.md
```

The important implementation rule is: **do not create duplicate agents for task size or model effort**. Do not create `angular-small-task-agent`, `angular-medium-task-agent`, `aspnet-implementer-low`, `aspnet-implementer-high`, etc. Keep one durable agent per real role/domain and let policies decide task mode, model effort, token-reduction rules, and contract behavior.

Every relevant subagent should reference these policies rather than duplicating their full text.

---

## 6. Model routing policy

### 6.1 Principle

The Team Lead should not freely choose arbitrary model names. The Team Lead should classify the task; a shared model-routing policy maps task class and risk to model/effort.

Team Lead output should be compact:

```yaml
task_class: medium_feature
risk:
  - auth
  - database_migration
domains:
  - data
  - backend
routing:
  contract_designer: opus-xhigh
  solution_designer: opus-high
  implementer: sonnet-high
  verifier: sonnet-xhigh
  final_reviewer: opus-xhigh
```

### 6.2 Default routing table

| Role | Default | Upgrade when |
|---|---|---|
| BA / requirements clarification | Sonnet high | Opus high only for very ambiguous product logic or regulated domain. |
| Team Lead / Orchestrator | Sonnet high | Opus high/xhigh only for large cross-stack decomposition. |
| Task Analyzer | Sonnet high | Opus high/xhigh for large/ambiguous/refactor/security-critical work. |
| Cross-stack Contract Designer | Opus xhigh | Keep Opus xhigh by default for cross-domain feature work. |
| Domain Solution Designer | Opus high | Opus xhigh for backend/data/devops/security/major architecture risk. |
| Implementer | Sonnet medium | Sonnet high for auth, migrations, concurrency, messaging, DevOps, security-sensitive code, large refactors, legacy unknowns. |
| Domain Verifier | Sonnet high | Sonnet xhigh for auth, data, DevOps, security, contract-sensitive work. |
| Final Integration Reviewer | Sonnet xhigh | Opus xhigh for cross-domain contract changes, production-critical work, or security-sensitive features. |
| Repair Agents | Sonnet high | Opus only for difficult root-cause diagnosis, not for routine compile/test failures. |
| Evidence Gatherer | Haiku or Sonnet low | Sonnet medium if evidence collection requires repo understanding. |

### 6.3 Strict cost rules

Use `max` rarely. Use it only for:

- Very ambiguous requirements.
- Critical architecture decisions.
- Large refactors across many projects.
- Security-sensitive redesign.
- Production incident root-cause analysis.
- Framework upgrades with many breaking changes.

Do not use Opus for ordinary implementers by default.

---

## 7. Contract-first protocol

### 7.1 Contract freeze is mandatory for cross-domain work

Before parallel domain work starts, the orchestrator must produce an approved contract artifact.

Minimum contract fields:

```yaml
contract_id: feature-name-contract
version: v1
status: frozen
requirements_source:
  - BA notes
  - user confirmations
acceptance_criteria:
  - ...
affected_domains:
  - data
  - backend
  - angular
shared_api:
  routes:
    - method: POST
      path: /api/...
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
out_of_scope:
  - ...
```

### 7.2 Contract versioning

Every task card and every agent result must include `contract_version`.

If a contract change happens:

```text
v1 → Contract Change Request → v2 approved → affected agents rebase/rework against v2
```

Old results must not be verified against a stale contract.

---

## 8. Contract change protocol

### 8.1 Rule

No agent may silently change a shared contract.

Local implementation detail changed?

```text
Continue and report as DONE_WITH_CONCERNS if there is risk.
```

Shared contract changed?

```text
Stop and emit BLOCKED_CONTRACT_CHANGE.
```

Shared contract includes:

- Database schema/index semantics.
- API routes.
- Request/response DTOs.
- Error codes and envelopes.
- Auth/authorization policies.
- Event/message contracts.
- Config/env vars.
- Migration/deployment order.
- Frontend/WPF/mobile-visible behavior.

### 8.2 Required status

Add a fifth status beyond the existing basic statuses:

```text
DONE
DONE_WITH_CONCERNS
NEEDS_CONTEXT
BLOCKED
BLOCKED_CONTRACT_CHANGE
```

### 8.3 Contract Change Request template

```yaml
status: BLOCKED_CONTRACT_CHANGE
agent: data-implementer
task_id: data-02
contract_version: v1
summary: "Original unique-email design conflicts with soft-delete requirement."
current_plan_problem:
  - "UNIQUE(email) prevents reusing email after soft delete."
recommended_change:
  - "Use partial unique index WHERE deleted_at IS NULL."
affected_domains:
  - backend
  - data
  - devops
possibly_affected_domains:
  - angular
risk_if_ignored:
  - "Backend duplicate check will not match database semantics."
options:
  A: "Partial unique index for active users only."
  B: "Email can never be reused, even after soft delete."
  C: "Remove soft delete for users."
recommendation: A
files_or_symbols_involved:
  - "Users table"
  - "CreateUser endpoint"
  - "UserEmailAlreadyExists error"
```

### 8.4 Orchestrator reaction to contract change

When any domain returns `BLOCKED_CONTRACT_CHANGE`:

1. Mark the task as blocked in the progress ledger.
2. Identify affected domains.
3. Pause only affected lanes, not the whole system if unrelated domains can safely continue.
4. Re-run Team Lead / Task Analyzer and Cross-stack Contract Designer.
5. Consult affected domain designers.
6. Produce Contract v2.
7. Broadcast v2 to affected agents.
8. Require affected agents to report whether they can continue, need rework, or must discard stale work.
9. Verify against v2 only.

---

## 9. Agent output protocol

Every agent must return structured output. No free-form-only result.

### 9.1 Implementer output

```yaml
agent: aspnet-implementer
task_id: backend-02
domain: backend
contract_version: v2
status: DONE | DONE_WITH_CONCERNS | NEEDS_CONTEXT | BLOCKED | BLOCKED_CONTRACT_CHANGE
summary: ...
files_changed:
  - path: src/...
    reason: ...
tests_added_or_changed:
  - path: tests/...
validation_performed:
  - dotnet test ...
risks_or_concerns:
  - ...
contract_deviations:
  - none
next_recommended_action:
  - run aspnet-verifier
```

### 9.2 Verifier output

```yaml
agent: aspnet-verifier
domain: backend
contract_version: v2
status: SIGNED_OFF | PUNCH_LIST | BLOCKED_BY_BUILD | BLOCKED_BY_TESTS | CONTRACT_MISMATCH
summary: ...
checks_performed:
  - build
  - tests
  - architecture boundaries
  - auth behavior
  - error handling
findings:
  - severity: high
    task_owner: backend-02
    problem: ...
    required_fix: ...
signoff: false
```

### 9.3 Designer output

```yaml
agent: aspnet-solution-designer
domain: backend
contract_version: v1
status: PLAN_READY | NEEDS_CONTEXT | BLOCKED_CONTRACT_CHANGE
summary: ...
tasks:
  - task_id: backend-01
    title: ...
    scope: []
    acceptance: []
    dependencies: []
    allowed_files_or_areas: []
    forbidden_changes: []
    suggested_model_risk: medium | high
verification_notes: []
```

---

## 10. Progress ledger

The main orchestrator must maintain a compact ledger that survives context compaction.

```yaml
feature: user-invitations
current_contract: v2
lanes:
  data:
    phase: verified
    tasks:
      data-01: DONE
      data-02: DONE_WITH_CONCERNS
    verifier: SIGNED_OFF
  backend:
    phase: implementing
    tasks:
      backend-01: DONE
      backend-02: BLOCKED_CONTRACT_CHANGE
    verifier: not_started
  angular:
    phase: paused_affected_by_contract_change
    tasks: {}
  wpf:
    phase: not_affected
  mobile:
    phase: not_affected
  devops:
    phase: needs_rework_due_to_migration_order
contract_changes:
  - from: v1
    to: v2
    reason: partial unique index for soft delete
final_gate:
  status: not_started
```

---

## 11. Orchestrator algorithm

```pseudo
function run_feature(request):
  ba_result = clarify_requirements(request)
  task_profile = task_analyzer(ba_result)
  affected_domains = task_profile.domains
  routing = model_routing_policy(task_profile)

  contract_v1 = cross_stack_contract_designer(ba_result, affected_domains)
  freeze(contract_v1)
  ledger.current_contract = contract_v1

  parallel for each domain in affected_domains:
    domain_plan = domain_solution_designer(domain, contract_v1)
    if domain_plan.status == BLOCKED_CONTRACT_CHANGE:
      handle_contract_change(domain_plan)
      continue

    parallel for each task in domain_plan.tasks:
      result = domain_implementer(domain, task, contract_v1)
      update_ledger(result)
      if result.status == BLOCKED_CONTRACT_CHANGE:
        handle_contract_change(result)
        pause_affected_lanes()
        rerun_affected_designers_and_implementers()

    verifier_result = domain_verifier(domain, domain_plan, contract_current)
    while verifier_result.status == PUNCH_LIST:
      dispatch_punch_list_to_owners(verifier_result.findings)
      verifier_result = domain_verifier(domain, domain_plan, contract_current)

  final_result = final_integration_gate(all_domain_results, contract_current)
  while final_result.status != SIGNED_OFF:
    dispatch_final_punch_list(final_result)
    rerun_affected_domain_verifiers()
    final_result = final_integration_gate(all_domain_results, contract_current)

  optional_security_audit_if_risk()
  commit_when_all_gates_signed_off()
```

---

## 12. Repair loops

Use repair agents after implementation when build/test failures occur.

| Failure | Route to |
|---|---|
| .NET build failure | `dotnet-build-error-resolver` |
| .NET test failure | `dotnet-test-failure-resolver` |
| Angular/Ionic build failure | `ng-build-error-resolver` |
| Angular/Ionic test failure | `angular-test-resolver` |
| CI failure | `ci-failure-diagnoser`, possibly `evidence-gatherer` |
| Unknown bug/regression | `issue-diagnoser`, possibly `evidence-gatherer` |

Repair agents should fix bounded failures only. They should not redesign the feature. If a repair requires changing the shared contract, they must emit `BLOCKED_CONTRACT_CHANGE`.

---

## 13. Final integration gate

Final integration reviewer checks the whole assembled feature, not isolated domains.

Required checks:

- Current contract version is used everywhere.
- All affected domain verifiers signed off.
- Build passes.
- Unit tests pass.
- Integration tests pass.
- E2E tests pass where applicable.
- DB migrations apply from clean DB and from previous version.
- Rollback or forward-fix path is known.
- API matches frontend/mobile/WPF expectations.
- Auth and authorization are tested.
- Error envelope and error codes match contract.
- Observability/logging is adequate.
- Secrets/config changes are documented.
- DevOps/deployment order is safe.
- No stale tasks remain in the ledger.

Final output:

```yaml
status: SIGNED_OFF | PUNCH_LIST | CONTRACT_MISMATCH | BLOCKED_BY_TESTS | BLOCKED_BY_SECURITY
contract_version: v2
summary: ...
required_fixes: []
commit_allowed: true | false
```

---

## 14. Implementation checklist for another agent

### 14.1 Create or update shared policy files

- [ ] `task-routing-policy.md`
- [ ] `domain-flow-router.md`
- [ ] `model-routing-policy.md`
- [ ] `contract-change-protocol.md`
- [ ] `agent-output-protocol.md`
- [ ] `progress-ledger-protocol.md`
- [ ] `final-integration-gate.md`
- [ ] `repo-separation-protocol.md`
- [ ] `token-reduction-policy.md`
- [ ] `domain-task-card-template.md`
- [ ] `contract-template.md`
- [ ] `contract-change-request-template.md`
- [ ] `verification-report-template.md`

### 14.2 Update Team Lead / orchestrator behavior

- [ ] Decide affected domains.
- [ ] Classify task size and risk.
- [ ] Apply model routing policy.
- [ ] Dispatch cross-stack contract designer.
- [ ] Freeze contract before parallel domain pipelines.
- [ ] Maintain progress ledger.
- [ ] Pause affected lanes on contract changes.
- [ ] Dispatch final integration gate.
- [ ] Prevent commit until final gate signs off.

### 14.3 Update each domain designer

- [ ] Accept `contract_version`.
- [ ] Produce task cards.
- [ ] Declare dependencies between tasks.
- [ ] Declare forbidden contract changes.
- [ ] Emit structured status.
- [ ] Use `BLOCKED_CONTRACT_CHANGE` for impossible plans.

### 14.4 Update each implementer

- [ ] Implement only one bounded task.
- [ ] Stay inside task card and contract.
- [ ] Write code and tests.
- [ ] Never silently change shared contract.
- [ ] Emit structured status.
- [ ] Include files changed and validation performed.

### 14.5 Update each verifier

- [ ] Run after its domain implementers complete.
- [ ] Verify against current contract version.
- [ ] Re-run relevant build/tests.
- [ ] Return punch-list mapped to task owners.
- [ ] Loop until signoff.

### 14.6 Add final integration reviewer/gate

- [ ] Check all domains together.
- [ ] Check contract consistency.
- [ ] Check build/test/E2E/migration/deployment safety.
- [ ] Produce commit approval or punch-list.

---


## 15. Task routing and execution modes

The system must not always run the full multi-agent flow. The Team Lead / Task Analyzer should classify the task and select the smallest safe execution mode.

This should be implemented as a shared skill/policy, not as separate agents:

```text
task-routing-policy.md
  decides size, risk, domains, contract impact, and mode

domain-flow-router.md
  executes the selected sequence using existing domain agents
```

### 15.1 Execution modes

| Mode | Flow | Use when | Approx token profile |
|---|---|---|---|
| `single_chat` | Main Claude Code session only | Tiny, clear, safe changes | Lowest |
| `implementer_only` | Main session → one domain implementer → main session verifies | Small domain-local coding task | Low |
| `domain_trio` | domain designer → one implementer → domain verifier | Medium domain-local feature | Medium |
| `fanout_domain_trio` | domain designer → 2-4 implementers → domain verifier | Large/risky work inside one domain | Medium-high |
| `cross_domain_light` | light contract → domain implementers → final reviewer | 2+ domains, obvious/stable contract | High |
| `full_cross_domain` | BA/Team Lead → contract designer → domain designers → implementer fan-out → domain verifiers → final gate | DB/API/UI/security/devops or production-critical work | Highest |

### 15.2 Routing rules

Use this decision ladder:

```text
If task is tiny and one-domain:
  use single_chat.

If task is small and one-domain but needs code edits:
  use implementer_only.

If task is medium and one-domain:
  use domain_trio.

If task is large/risky but one-domain:
  use fanout_domain_trio.

If task touches 2+ domains but the contract is obvious and stable:
  use cross_domain_light.

If task touches DB + Backend + Frontend, security, DevOps/deploy, migrations, auth, or production-critical behavior:
  use full_cross_domain.
```

### 15.3 Angular example

Do not create separate Angular agents per mode. Keep only:

```text
angular-solution-designer
angular-implementer
angular-verifier
```

Use routing policy:

```yaml
angular_small:
  when:
    - one component/service/file area
    - no API contract change
    - no auth/security impact
    - no complex state or routing restructure
  flow:
    - single_chat_or_angular_implementer_only
  model:
    implementer: sonnet-medium

angular_medium:
  when:
    - new component/page
    - local state or API service changes
    - tests needed
    - existing API contract is used unchanged
  flow:
    - angular-solution-designer
    - angular-implementer
    - angular-verifier
  model:
    designer: opus-high
    implementer: sonnet-medium
    verifier: sonnet-high

angular_large_or_risky:
  when:
    - multiple feature areas
    - auth-sensitive UI
    - complex state/forms
    - performance or accessibility concern
    - cross-domain API change
  flow:
    - angular-solution-designer
    - angular-implementer[component-template]
    - angular-implementer[state-api]
    - angular-implementer[tests]
    - angular-verifier
  model:
    designer: opus-xhigh
    implementers: sonnet-high
    verifier: sonnet-xhigh
```

### 15.4 Team Lead routing output

The Team Lead should output compact metadata, not huge repeated prompts:

```yaml
task_id: ANG-042
domain: angular
classification: medium
risk_level: medium
flow: domain_trio
agents:
  - angular-solution-designer
  - angular-implementer
  - angular-verifier
contract_version: none
reason:
  - adds new routed page
  - includes form validation
  - requires tests
  - no backend contract change
```

The `domain-flow-router` then executes the flow.

---

## 16. Single chat and single implementer mode

The full subagent flow must not be the default for every task. Many tasks are cheaper and safer in a single Claude Code session.

### 16.1 Single chat mode

Use when the task is small, obvious, low risk, and either no repo-wide architecture reasoning is required or the change is localized.

Examples:

```text
Fix typo.
Rename property.
Update one validation message.
Fix one failing unit test.
Change one CSS class.
Adjust one endpoint response mapping without contract change.
```

Flow:

```text
Main Claude Code session
  ↓
plan lightly
  ↓
edit
  ↓
run focused validation
  ↓
report
```

### 16.2 Single implementer mode

Use when a bounded domain-specific implementer is useful but the designer/verifier overhead is not worth it.

Flow:

```text
Main session
  ↓
one domain implementer
  ↓
main session runs build/test/review
```

Examples:

```text
Backend-only endpoint adjustment.
Frontend-only component fix.
DB-only migration correction.
WPF-only ViewModel bug.
Ionic-only styling or permission wrapper update.
DevOps-only small CI config fix.
```

### 16.3 Guardrails for scaled-down modes

Single chat or implementer-only mode must stop and escalate when it detects:

```text
cross-domain contract impact
auth/authorization risk
migration or data-loss risk
deployment order risk
security-sensitive behavior
large refactor surface
unclear legacy behavior
```

Escalation target:

```text
single_chat → implementer_only or domain_trio
implementer_only → domain_trio
one-domain mode → cross_domain_light or full_cross_domain
```

---

## 17. Separate repository protocol

If frontend and backend are in different repositories, do not force every agent to load both repos. Use a shared contract plus separate repo flows.

### 17.1 Principle

```text
One repo = one local domain flow.
Multiple repos = shared contract + separate repo flows + final integration gate.
```

### 17.2 Recommended split

For backend and frontend in different repos:

```text
Shared planning / ticket / contract doc
  ↓
Cross-stack Contract Designer
  ↓
Contract v1 frozen
  ↓
Backend repo flow:
  data-solution-designer → data-implementer(s) → data-verifier
  aspnet-solution-designer → aspnet-implementer(s) → aspnet-verifier
  ↓
Backend PR
  ↓
Frontend repo flow:
  angular-solution-designer → angular-implementer(s) → angular-verifier
  ↓
Frontend PR
  ↓
Final integration reviewer checks both PRs against Contract v1/v2
```

Backend and frontend implementation may run in parallel **only after the contract is frozen**.

### 17.3 Shared contract storage

Store the contract in a place both repos/agents can read:

```text
docs/contracts/<feature>.contract.md
openapi.yaml or openapi.json
ticket acceptance criteria
shared-api-schema.yaml
generated TypeScript client snapshot
generated C# DTO/API snapshot
```

The contract must include:

```text
routes
request DTOs
response DTOs
error envelope and error codes
auth/permissions
pagination/filtering/versioning
feature flags
migration/deployment order
backward compatibility rules
```

### 17.4 Contract drift handling across repos

If backend discovers the contract is impossible, it must not silently change DTOs/routes/errors. It must emit:

```text
BLOCKED_CONTRACT_CHANGE
```

Then:

```text
Contract v1 → Contract Change Request → Contract v2
Backend repo updates to v2
Frontend repo receives v2 and updates
Final integration reviewer verifies both PRs against v2
```

---

## 18. Token reduction policy: Ponytail and Caveman

Use token reduction as a policy, not as ad-hoc instructions inside every agent.

Create:

```text
policies/token-reduction-policy.md
```

### 18.1 Ponytail recommendation

Ponytail should be the primary token/cost reducer because it reduces unnecessary work and code, not only response verbosity.

Use Ponytail rules for:

```text
implementers
repair agents
code reviewer
final integration reviewer
```

Use Ponytail-lite for:

```text
domain solution designers
cross-stack contract designer
Team Lead / Task Analyzer
```

Core Ponytail behavior expected from implementers:

```text
1. Do not write code if the task can be solved by configuration, existing code, or deletion.
2. Search for existing patterns before adding new abstractions.
3. Prefer framework/native/stdlib features before new dependencies.
4. Implement the smallest change satisfying the contract and acceptance criteria.
5. Do not future-proof speculatively.
6. Do not cut security, accessibility, validation, data-loss prevention, or migration safety.
```

Verifier/reviewer Ponytail checks:

```text
Did the agent overbuild?
Did it add unnecessary abstractions?
Did it add dependencies without need?
Did it duplicate existing utilities?
Did it change more files than the task required?
Can the diff be smaller without losing correctness/safety?
```

### 18.2 Caveman recommendation

Caveman should be used selectively, not globally.

Good uses:

```text
compact implementer final reports
compact verifier punch-lists
repair-agent summaries
commit messages
PR comments
compressing shared policy/memory files
shrinking large MCP/tool descriptions
```

Avoid Caveman for:

```text
BA requirements clarification
cross-stack contract output
solution-design docs
security-audit reports
final architecture decisions
contract-change requests that need high readability
```

Reason: Caveman mainly reduces output tokens. It does not reduce file reads, tool outputs, reasoning, or repeated context. It can even be net-negative if every agent loads extra Caveman instructions while producing already-short outputs.

### 18.3 Recommended combined configuration

```yaml
token_reduction_policy:
  ponytail:
    implementers: full
    repair_agents: full
    code_reviewer: full
    final_integration_reviewer: full
    designers: lite
    contract_designer: lite

  caveman:
    reports: lite
    punch_lists: lite
    commit_messages: lite
    pr_comments: lite
    contracts: off
    solution_design: off
    security_audit: off

  caveman_compress:
    shared_policies: enabled
    CLAUDE_md: enabled
    long_rules: enabled
    MCP_descriptions: enabled_when_large
```

---

## 19. Token estimates and budgeting

Token estimates are rough. Actual usage depends on repo size, file search volume, build/test logs, number of implementers, and rework loops.

### 19.1 DB + Backend + Frontend task without Ponytail/Caveman

For a medium task touching database, backend, and frontend:

```text
Clean run:
  ~1.0M–2.6M tokens

Normal realistic run with some fixes:
  ~1.4M–3.2M tokens

Messy run with contract change or failing tests:
  ~2.0M–4.5M+ tokens
```

Rule of thumb:

```text
Each domain touched adds ~300k–800k tokens.
Contract + orchestration + final review adds ~300k–800k tokens.
```

### 19.2 Compared to single chat

For the same DB + Backend + Frontend task:

```text
Single chat:
  ~300k–900k tokens

Full subagent flow:
  ~1.5M–3.0M tokens normally
  ~2.0M–4.0M+ with rework
```

The full subagent flow can easily cost 2x–5x more tokens than single chat. Parallelism saves wall-clock time and improves separation of concerns, but it does not save tokens.

### 19.3 With Ponytail and selective Caveman

Realistic estimate:

```text
Without plugins:
  ~1.5M–3.0M tokens

With Ponytail only:
  ~1.15M–2.4M tokens

With Caveman response style only:
  ~1.35M–2.9M tokens, sometimes no real saving

With Ponytail + compressed shared policies + compact reports:
  ~900k–2.0M tokens
```

Expected total reduction:

```text
Ponytail: 10–30% total reduction on normal tasks.
Caveman response style: 0–15% total-session reduction; may be neutral or negative.
Caveman-compressed policies/MCP descriptions: useful recurring savings if many agents repeatedly load the same long instructions.
Combined best realistic reduction: ~20–35%.
```

### 19.4 Angular-only flow token estimates

For:

```text
angular-solution-designer → angular-implementer → angular-verifier
```

Estimate:

```text
Small Angular task:
  ~180k–450k tokens

Medium Angular task:
  ~400k–900k tokens

Large Angular task:
  ~900k–1.8M+ tokens
```

With 2–3 parallel Angular implementers:

```text
Medium Angular task with fan-out:
  ~700k–1.4M tokens
```

With Ponytail + compressed policies + compact reports:

```text
Medium Angular domain_trio:
  ~280k–650k tokens
```

### 19.5 Cost control rules

```text
Do not use full_cross_domain by default.
Use single_chat for tiny tasks.
Use implementer_only for small domain-local tasks.
Use domain_trio for medium domain-local work.
Use full_cross_domain only when cross-domain coordination risk justifies the cost.
Use Ponytail everywhere implementation/review happens.
Use Caveman mainly for repeated policies and final reports, not for contracts.
```

---

## 20. Skill vs agent boundary

A frequent implementation mistake is turning every workflow option into a new agent. Do not do that.

### 20.1 Agents should represent worker roles

Agents do work:

```text
design
implement
verify
diagnose
repair
audit
```

Examples:

```text
aspnet-solution-designer
aspnet-implementer
aspnet-verifier
data-implementer
angular-verifier
security-auditor
```

### 20.2 Skills/policies should represent orchestration rules

Skills/policies decide how work flows:

```text
which agents to call
how many implementers to fan out
what model effort to use
when to freeze contract
when to pause lanes
when to escalate
when to run final integration gate
when to use Ponytail/Caveman
```

Examples:

```text
task-routing-policy
domain-flow-router
model-routing-policy
contract-change-protocol
token-reduction-policy
final-integration-gate
```

### 20.3 Boilerplate control

Keep every agent description short and reference shared policy files.

Example implementer header:

```text
aspnet-implementer

Purpose:
Implement one bounded ASP.NET backend task according to the approved contract and aspnet-solution-designer task card.

Must follow:
- task-routing-policy when invoked through router
- model-routing-policy
- contract-change-protocol
- agent-output-protocol
- token-reduction-policy
- dotnet-web-backend
- dotnet-testing
- dotnet-security when relevant
```

---

## 21. Strict anti-patterns

Do not do these:

```text
Do not run designer, implementer, and verifier for the same domain at the same time.
Do not allow silent shared-contract changes.
Do not use generic verifier when a domain-specific verifier exists.
Do not let Code Reviewer ask Team Lead to approve code quality; reviewer must independently compare spec + contract + diff + tests.
Do not duplicate agents just to vary model effort.
Do not use Opus Max everywhere.
Do not use Sonnet Low for serious implementation by default.
Do not commit after domain verifier signoff only; final integration gate is mandatory.
Do not verify against old contract versions.
Do not create separate agents for small/medium/large task modes. Use task-routing-policy instead.
Do not create separate agents only to vary model effort. Use model-routing-policy instead.
Do not use the full cross-domain flow for tiny tasks.
Do not load multiple repos into every agent when a shared contract + repo-separated flow would be cheaper.
Do not use Caveman globally for contracts or design specs.
Do not allow Ponytail/minimalism to cut security, accessibility, validation, migration safety, or data-loss safeguards.
```

---

## 22. Recommended final shape

```text
BA / Requirements Clarification
  ↓
Team Lead / Task Analyzer
  ↓
Cross-stack Contract Designer → Contract v1 frozen
  ↓
Parallel domain pipelines:
  Data:    data-solution-designer    → data-implementer(s)    → data-verifier
  Backend: aspnet-solution-designer  → aspnet-implementer(s)  → aspnet-verifier
  Angular: angular-solution-designer → angular-implementer(s) → angular-verifier
  WPF:     wpf-solution-designer     → wpf-implementer(s)     → wpf-verifier
  Mobile:  mobile-solution-designer  → mobile-implementer(s)  → mobile-verifier
  DevOps:  devops-solution-designer  → devops-implementer(s)  → devops-verifier
  ↓
Final Integration Reviewer / Code Reviewer
  ↓
Security Auditor if risk requires
  ↓
Commit allowed only after final signoff
```



---

## 23. Issue investigation flow

Issue investigation must use a different flow from feature implementation.
Do **not** start bug/incident work with the normal feature path:

```text
BA → Team Lead → Solution Designers → Implementers
```

For bugs, CI failures, production incidents, runtime errors, flaky tests, and unclear behavior, start with evidence and diagnosis:

```text
Issue / Failure Report
  ↓
Issue Intake / Triage
  ↓
issue-diagnoser or ci-failure-diagnoser
  ↓
parallel evidence-gatherer agents
  ↓
Diagnosis Report
  ↓
Fix Decision Gate
  ├─ Stop after investigation
  ├─ Resolver fix loop
  ├─ Single-domain fix
  └─ Cross-domain fix
```

The strict rule is:

```text
Diagnose before coding.
Evidence-gatherers may run in parallel.
Implementers must not start until the diagnosis is proven or the fix route is explicitly approved.
```

### 23.1 Issue-flow agents

| Agent | Responsibility | Writes code? |
|---|---|---:|
| `issue-diagnoser` | Reproduce/isolate root cause from logs, errors, screenshots, code paths, and evidence. Produces diagnosis and fix plan. | No |
| `ci-failure-diagnoser` | Reads failing CI/build/test output and identifies likely broken domain and fix route. | No |
| `evidence-gatherer` | Cheap read-only helper for repro steps, log extraction, stack traces, code references, recent diffs, screenshots, environment facts. | No |
| Repair resolvers | Execute bounded red-to-green loops for compile/test failures. | Yes |
| Domain implementers | Implement the approved fix when root cause is known. | Yes |
| Domain verifiers | Verify fix against diagnosis, reproduction, tests, and contract. | No / read-only preferred |
| Final integration reviewer | Checks cross-domain issue fixes and prevents merging partial fixes. | No / read-only preferred |

The important rule is that `issue-diagnoser` is the **bug-side equivalent of a solution designer**, but only after it has enough evidence. It plans and routes the fix; it does not write the fix.

### 23.2 Investigation-only mode

Use this when the user asks:

```text
Investigate why this happens.
Find root cause.
Explain what is broken.
Check why CI failed.
Tell me what needs to be fixed.
```

Flow:

```text
Issue report
  ↓
issue-diagnoser
  ├─ evidence-gatherer: reproduce locally
  ├─ evidence-gatherer: inspect logs / stack trace
  ├─ evidence-gatherer: locate related code / symbols
  └─ evidence-gatherer: compare recent changes if needed
  ↓
Diagnosis Report
  ↓
Stop. Do not implement unless user/policy asks for fix.
```

Required output:

```yaml
status: DIAGNOSED | NOT_REPRODUCED | NEEDS_MORE_EVIDENCE | LIKELY_FLAKE | INCONCLUSIVE
confidence: high | medium | low
root_cause:
  summary:
  files:
    - path:
      symbol:
      reason:
evidence:
  - type: log | stack_trace | failing_command | screenshot | code_reference | repro_step
    detail:
affected_domains:
  - backend | data | angular | wpf | mobile | devops
fix_recommendation:
  route: no_fix_needed | resolver | single_domain_implementer | cross_domain_contract_change | user_decision_needed
fix_plan:
  - step:
risk:
  - regression risk
  - migration/data-loss risk
  - security risk
open_questions:
  - question:
```

### 23.3 Investigation + optional fix mode

Use this when the user asks:

```text
Investigate and fix if obvious.
Find the cause, then implement fix if safe.
Diagnose CI failure and repair it.
```

Flow:

```text
Issue report
  ↓
issue-diagnoser + evidence-gatherer(s)
  ↓
Diagnosis Report
  ↓
Fix Decision Gate
  ↓
If safe/local:
  domain implementer or resolver
    ↓
  domain verifier
    ↓
  final investigation report

If risky/cross-domain:
  contract/fix plan
    ↓
  affected domain pipelines
    ↓
  final integration reviewer
```

The fix decision gate must be explicit. Do not silently move from diagnosis into implementation if the user only requested investigation.

### 23.4 Fix routing rules

#### Build failure

```text
CI/build failure
  ↓
ci-failure-diagnoser
  ↓
dotnet-build-error-resolver or ng-build-error-resolver
  ↓
re-run build
  ↓
domain verifier
```

Use this for:

```text
C# compile error
NuGet restore issue
TypeScript compile error
Angular template error
bundler/build pipeline error
```

#### Test failure

```text
Failing tests
  ↓
issue-diagnoser or ci-failure-diagnoser
  ↓
dotnet-test-failure-resolver or angular-test-resolver
  ↓
re-run focused tests
  ↓
domain verifier
```

Use this when the failure is already localized to test behavior or a known test suite.

#### Runtime bug, single domain

Example: backend endpoint returns wrong status code.

```text
issue-diagnoser
  ↓
aspnet-implementer
  ↓
aspnet-verifier
```

For small proven bugs, the diagnosis report can replace the normal solution-designer step. Do not over-plan.

#### Runtime bug, multiple domains

Example: frontend sends valid payload, backend accepts it, but DB constraint fails.

```text
issue-diagnoser
  ├─ evidence-gatherer: frontend request payload
  ├─ evidence-gatherer: backend logs/code path
  ├─ evidence-gatherer: DB constraint/migration
  └─ evidence-gatherer: recent related diffs
  ↓
Diagnosis Report
  ↓
Contract/fix plan
  ↓
Data:    implementer → verifier
Backend: implementer → verifier
Angular: implementer → verifier
  ↓
Final integration reviewer
```

If the fix changes a shared contract, the issue flow must use the same `BLOCKED_CONTRACT_CHANGE` protocol as feature work.

#### Security issue

```text
security-auditor
  ↓
OWASP/CWE punch-list
  ↓
affected domain implementer(s)
  ↓
affected domain verifier(s)
  ↓
security re-check
  ↓
final integration reviewer if cross-domain
```

Do not let Ponytail/minimalism remove security checks, validation, authorization safeguards, logging, auditability, migration safety, or data-loss protections.

### 23.5 Parallelism rules for investigation

Parallelism is encouraged in the **evidence-gathering phase**:

```text
issue-diagnoser
  ├─ evidence-gatherer: reproduce
  ├─ evidence-gatherer: logs / stack trace
  ├─ evidence-gatherer: backend code path
  ├─ evidence-gatherer: frontend code path
  ├─ evidence-gatherer: DB schema / migration
  └─ evidence-gatherer: CI/runtime environment
```

Parallel coding before diagnosis is dangerous and should be blocked:

```text
BAD:
  backend implementer starts fixing
  frontend implementer starts fixing
  data implementer starts fixing
  before root cause is known
```

Allowed parallelism:

```text
Read-only evidence collection: yes.
Independent fix experiments: no, unless explicitly sandboxed and not committed.
Domain implementers: only after diagnosis and fix route are approved.
```

### 23.6 Investigation execution modes

Add these modes to `task-routing-policy` and `domain-flow-router`.

| Mode | Use when | Flow | Approx tokens |
|---|---|---|---:|
| `investigation_only` | User wants root cause/report only. | `issue-diagnoser → evidence-gatherer(s) → report` | 100k–1.5M depending on scope |
| `investigation_safe_fix` | Root cause likely local and fix is allowed if obvious. | `diagnose → implementer/resolver → verifier` | 250k–1.3M |
| `ci_repair_loop` | Build/test failure is the main issue. | `ci-failure-diagnoser → resolver → verifier` | 200k–1.0M |
| `cross_domain_issue_fix` | Bug spans DB/backend/frontend/mobile/devops or changes contract. | `diagnose → contract/fix plan → affected domain flows → final reviewer` | 1.0M–4.0M+ |
| `security_issue_fix` | Security finding or suspicious auth/data exposure issue. | `security-auditor → punch-list → domain fixes → security re-check` | 800k–3.0M+ |

More detailed token estimates:

```text
Investigation only:
  Small/local issue:       ~100k–300k
  Medium issue:            ~300k–700k
  Large/cross-domain:      ~700k–1.5M+

Investigation + safe local fix:
  Small fix:               ~250k–700k
  Medium fix:              ~600k–1.3M

Investigation + cross-domain fix:
  Normal cross-domain bug: ~1.0M–2.5M
  Messy root cause:        ~2.0M–4.0M+
```

### 23.7 Issue investigation policy file

Create a shared policy/skill:

```text
policies/issue-investigation-flow.md
```

It must enforce:

```text
1. Diagnose before coding.
2. Evidence-gatherers may run in parallel.
3. Diagnosis must include proof, not guesses.
4. No implementer starts until diagnosis status is DIAGNOSED, or a bounded resolver route is selected.
5. If fix is optional, stop after report unless policy/user allows fix.
6. If shared contract changes, pause affected lanes and create Contract v2.
7. Verifier checks the fix against the diagnosis and original reproduction, not just against code style.
8. Final report must include root cause, changed files, tests run, remaining risk, and any non-reproduced assumptions.
9. Do not hide uncertainty. If the issue cannot be reproduced, return NOT_REPRODUCED or INCONCLUSIVE.
10. Do not let repair agents make broad refactors; repair loops must be bounded and focused on the failing symptom.
```

Add this policy to the required shared policy list from Section 5:

```text
policies/issue-investigation-flow.md
```

### 23.8 Issue-flow task card template

```yaml
task_id:
mode: investigation_only | investigation_safe_fix | ci_repair_loop | cross_domain_issue_fix | security_issue_fix
issue_summary:
reported_by:
source:
  type: user_report | CI | log | screenshot | production_alert | test_failure
reproduction:
  known_steps:
  expected:
  actual:
constraints:
  can_modify_code: true | false
  require_user_approval_before_fix: true | false
  max_repair_iterations:
affected_domains_guess:
  - data | backend | angular | wpf | mobile | devops
allowed_agents:
  - issue-diagnoser
  - evidence-gatherer
  - ci-failure-diagnoser
  - domain implementers if fix is approved
stop_conditions:
  - diagnosis_status is DIAGNOSED and fix not approved
  - diagnosis_status is NOT_REPRODUCED
  - repair_iterations exceed limit
required_output:
  - diagnosis_report
  - evidence_list
  - fix_route
  - tests_or_repro_commands
```

### 23.9 Issue-flow final report template

```yaml
status: FIXED | DIAGNOSED_ONLY | NOT_REPRODUCED | PARTIALLY_FIXED | BLOCKED | INCONCLUSIVE
root_cause:
  summary:
  evidence:
fix_applied:
  changed_files:
  summary:
  contract_version:
verification:
  commands_run:
  results:
  reproduction_retested: true | false
remaining_risk:
  - risk:
follow_up:
  - task:
```

### 23.10 Issue-flow anti-patterns

Do not do these:

```text
Do not start coding before root cause is known.
Do not let evidence-gatherers write code.
Do not let issue-diagnoser implement the fix.
Do not run full feature planning for a tiny proven bug.
Do not treat test snapshots as truth without checking product behavior.
Do not silence or delete failing tests just to make CI green.
Do not change shared API/DB/frontend contracts without a Contract Change Request.
Do not keep all domain lanes running when one lane discovers a contract-breaking root cause.
Do not merge after a resolver says green; domain verifier or final reviewer must verify the fix route.
```

---

## 24. Updated final routing overview

The system should support two top-level routing families.

### 24.1 Feature / change request family

```text
Requirements / BA
  ↓
Team Lead / Task Analyzer
  ↓
Task routing policy selects mode:
  ├─ single_chat
  ├─ implementer_only
  ├─ domain_trio
  ├─ fanout_domain_trio
  ├─ lightweight_cross_domain
  └─ full_cross_domain
```

### 24.2 Issue / bug / incident family

```text
Issue / failure report
  ↓
Issue Intake / Triage
  ↓
Task routing policy selects mode:
  ├─ investigation_only
  ├─ investigation_safe_fix
  ├─ ci_repair_loop
  ├─ cross_domain_issue_fix
  └─ security_issue_fix
```

Top-level decision rule:

```text
If the task asks to build or change expected behavior, use feature/change-request routing.
If the task asks why something is broken, failing, flaky, slow, crashing, or inconsistent, use issue-investigation routing first.
```

