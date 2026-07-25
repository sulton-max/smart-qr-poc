# Data-seam POC — Dapper reads / EF writes

A throwaway console app probing whether an entity **read by Dapper** can be **attached to EF** and written correctly. Feeds the backend-SDK read-seam design and the validation § *Phases* (transition constraints need the prior state).

## Run

Needs a throwaway Postgres. Default connection `Host=localhost;Port=5432;Database=smartqr_dataseam;Username=postgres;Password=postgres`; override via `argv[0]` or the `DATASEAM_DB` env var.

```bash
createdb smartqr_dataseam           # or: psql -c 'create database smartqr_dataseam'
dotnet run
```

The app resets its own schema on each run (drops + recreates `codes`, seeds one row).

## What the probes measured (2026-07-23)

| Probe | Result |
|---|---|
| 1 · Dapper read → `Attach` (clean) | `Unchanged`, `OriginalValues[Mode] = static` — correct baseline |
| 2 · **mutate → `Attach`** | `Unchanged`, `OriginalValues = dynamic` (the mutation) → **SaveChanges writes nothing; the edit is silently lost** |
| 3 · `Attach` → mutate | `Modified`, `static / dynamic` → writes correctly |
| 4 · recover true original | `GetDatabaseValues()` returns `static` — but that is a **second query** |
| 5 · EF `AsNoTracking` baseline | `Detached`, no snapshot, `OriginalValues` reflects the mutation |
| 6 · jsonb column | Dapper hands back raw text; EF's `ValueConverter` must be applied by hand |

## The rules that fall out

- **Order is load → attach → mutate.** `Attach` snapshots `OriginalValues` from the current instance, so mutate-then-attach makes the mutation the "original" and the entity reads `Unchanged`.
- **`OriginalValues` / `AsNoTracking` are not a "compare against previous" source** before the handler owns the entity.
- **A Dapper-read entity needs EF's value converters applied by hand** for jsonb / owned columns.

## Handoff to the read-seam chat

The seam must, on a read meant to be written back: materialize via Dapper → apply the same converters EF's model declares → `Attach` **before** any mutation. Open: where converter application lives (reflect the EF model vs a hand-maintained map), and whether the seam exposes a single `LoadForUpdate<T>(id)` that guarantees the order.
