# NHibernate mechanics

Concrete NHibernate forms of the ORM-agnostic principles in `SKILL.md`. Load when the ORM is NHibernate. Authored from knowledge - there is no upstream skill for this; keep it current from the NHibernate reference docs.

## Session lifetime

- `ISessionFactory` is the expensive, thread-safe singleton - build it once at startup and reuse. Never rebuild per request.
- `ISession` is the per-request unit of work - cheap, stateful, **not thread-safe**. Open one per web request (session-per-request), commit in a transaction, dispose at request end.
- `IStatelessSession` for bulk / ETL: no identity map, no change tracking, no cascade, no first-level cache - fast, but you manage inserts/updates explicitly and lazy loading is unavailable.

## Transactions and flushing

- Always wrap writes in an explicit `ITransaction` (`using var tx = session.BeginTransaction(); ...; tx.Commit();`). Implicit transactions are discouraged.
- `FlushMode.Auto` (default) flushes before queries that might read pending changes, which can surprise you. Set `FlushMode.Commit` so the session flushes only at commit.
- Read-only paths: `session.DefaultReadOnly = true` or `SetReadOnly(entity, true)` to skip dirty-checking; or use `IStatelessSession`.

## Loading and N+1

- Associations are lazy by default; N+1 comes from touching a lazy collection in a loop. Fixes:
  - Eager fetch in the query: QueryOver `.Fetch(x => x.Lines).Eager`, LINQ `.Fetch(x => x.Lines)`, or HQL `join fetch`.
  - a batch-size on the collection or class mapping so NH loads proxies in batched `IN (...)` round trips instead of one-at-a-time.
  - Futures (`.Future()` / `.FutureValue()`) to batch several queries into one round trip.
- Multiple eager collection fetches cause a cartesian product - use futures or separate queries rather than fetching two collections in one query.

## Querying

- Prefer QueryOver (type-safe) or the LINQ provider (`session.Query<T>()`); drop to HQL for complex set operations. Criteria is legacy.
- Bulk DML without loading: HQL `session.CreateQuery("update Order o set o.Status = :s where ...").ExecuteUpdate()`.

## Projection and read models

- Project to a DTO: LINQ `session.Query<T>().Select(...)`, or QueryOver `.SelectList(...)` with a `Transformers.AliasToBean<TDto>()` result transformer - avoids loading full entities for reads.

## Second-level cache

- Opt-in per entity/collection plus a cache provider. Cache read-mostly reference data, not volatile write-heavy tables (staleness). The query cache is separate - use sparingly, it needs the entity cache to be useful.

## Mapping and schema

- Prefer mapping-by-code or Fluent NHibernate over XML `.hbm` for new work.
- NHibernate does not own schema migrations. Use FluentMigrator (or another migration tool) for schema evolution; `SchemaExport` / `SchemaUpdate` are for dev/test bootstrap only, never production.
