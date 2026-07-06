---
name: sqlite
description: "SQLite engine specialist - the SQLite-specific delta on top of the cross-engine database-conventions hub: when SQLite fits (and when it doesn't), the single-writer / WAL concurrency model and busy-timeout, PRAGMAs (foreign_keys, journal_mode, synchronous), type affinity vs STRICT tables and date/bool storage, limited ALTER TABLE and the table-rebuild, connection-per-thread and in-memory test DBs, B-tree-only indexing, FTS5, and backup. Load for a SQLite .db, a PRAGMA, an embedded/desktop/mobile/test store, or an EF Core SQLite provider quirk. Companions: database-conventions (cross-engine hub - load first), dotnet-data-access (the EF Core / ORM side), postgres (the other engine)."
metadata:
  type: reference
  sources: "Authored from knowledge (no upstream skill). Engine-neutral conventions owned by database-conventions; this holds the SQLite-specific delta only."
---

# sqlite (engine specialist)

The SQLite-specific layer. **Cross-engine conventions live in `database-conventions`** (surrogate keys, no `SELECT *`, bound result sets, migration reversibility) - load that hub first and do not restate it. The EF Core side lives in `dotnet-data-access`. This is only what changes *because the engine is SQLite*.

## When it fits

- Good: embedded / desktop / mobile app storage, single-node edge, a local cache, and test databases. It is a file, not a server.
- Bad: high-write-concurrency multi-client web. SQLite allows **one writer at a time** for the whole database - reach for `postgres` there.

## Concurrency

- Enable WAL: `PRAGMA journal_mode=WAL` - readers no longer block the single writer, which is the big throughput win. WAL persists on the file.
- `PRAGMA busy_timeout=5000` (ms) so a contended writer waits instead of failing instantly with `SQLITE_BUSY`.
- `PRAGMA synchronous=NORMAL` with WAL is the usual durability/speed balance (`FULL` is safest, slower).

## PRAGMAs and typing

- `PRAGMA foreign_keys=ON` on **every connection** - FK enforcement is OFF by default.
- SQLite is dynamically typed (type *affinity*, not strict) - a column will accept any type. Declare `STRICT` tables (3.37+) to enforce the declared types.
- No native boolean or date/time type: store dates as ISO-8601 `TEXT` or epoch `INTEGER`, booleans as `0`/`1`. Sort/compare accordingly.

## Schema changes

- `ALTER TABLE` supports only `ADD COLUMN` and `RENAME` (dropping/altering a column is limited even post-3.35). Other changes need the 12-step rebuild: create the new table, `INSERT INTO ... SELECT`, drop the old, rename.
- EF Core migrations on SQLite rebuild tables for many operations - can be slow and occasionally lossy. Review the generated SQL before applying.

## Queries and indexes

- No `RIGHT`/`FULL OUTER JOIN` before 3.39 - rewrite as `LEFT JOIN`.
- B-tree indexes only (no GIN/BRIN); partial and expression indexes are supported. Run `ANALYZE` / `PRAGMA optimize` for planner statistics, and `EXPLAIN QUERY PLAN` to confirm an index is used.
- Full-text search: use an FTS5 virtual table, not `LIKE '%term%'`.

## Connections and testing

- No server, no pooling - a connection is a file handle. Keep a connection per thread; do not share one connection across threads.
- An in-memory database (`:memory:`) is private to its connection unless you use a shared-cache name. For tests, hold the connection open for the database's lifetime, or the schema vanishes when it closes.

## Backup

The database is a single file: copy it while idle, or use the online backup API / `VACUUM INTO 'backup.db'` for a consistent copy of a live database.
