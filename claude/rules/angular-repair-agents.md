---
paths: ["**/angular.json", "**/*.component.ts", "**/*.spec.ts", "**/*.scss"]
---

Two Angular repair loops: **`ng-build-error-resolver`** (ng build - covers Ionic/Capacitor apps, ionic
build wraps ng build; TS/NG/bundler triage; 5-cycle cap) and **`angular-test-resolver`** (ng
test / Jest red->green, never xit/skip/weaken; Ionic specs in scope); default to delegating a
fix-the-build / make-the-tests-pass task to the matching resolver rather than looping in-session
(the subagent absorbs the output and returns only a diagnosis); both pinned sonnet/high.
A fix that would need a shared-contract change is outside a resolver's bounded scope - it
stops as BLOCKED_CONTRACT_CHANGE for `cross-stack-agents-flow` to route, never edits the contract to go green.
