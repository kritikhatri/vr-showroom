# 8. Database Architecture and Implementation Decisions

## Overview

| Layer | Technology | Location |
|---|---|---|
| Client | Unity (C#), `UnityWebRequest` | `Assets/Member3/Scripts/ProductCatalogue.cs` |
| API | Node.js, Express 5, TypeScript | `backend/src/server.ts` |
| Data access | Prisma ORM 7 + `@prisma/adapter-pg` (node-postgres) | `backend/src/prisma.ts`, `backend/prisma/schema.prisma` |
| Database | PostgreSQL 15+ | local database `vr_showroom` per team member |
| Schema changes | Prisma migrations (SQL files in Git) | `backend/prisma/migrations/` |
| Sample data | Seed script reading the Unity JSON catalogue | `backend/prisma/seed.ts` |

## Key decisions

| # | Decision | Reason |
|---|---|---|
| 1 | **PostgreSQL** over MongoDB | Structured, related data that must stay consistent (see [01](01_Database_Selection.md)). |
| 2 | **Prisma ORM** for the API, **raw SQL** where it is clearer | Type-safe queries and versioned migrations for the team. The stock report uses `$queryRaw` because `GROUP BY` with `FILTER` is clearer in SQL. |
| 3 | **Migrations committed to Git**; `.env` is not | Every teammate gets an identical schema from `npm run db:setup` while keeping their own local password. |
| 4 | **Sizes normalized** into `Size` + `ProductSize` | The original comma-separated `availableSizes` text broke 1NF (see [03](03_Normalization.md)). Normalizing enables size queries, stock per size, and validation. |
| 5 | **Data migration inside the schema migration** | Teammates' existing databases are converted in place with no lost sizes. Tested on a copy of the original database. |
| 6 | **API keeps the JSON shape Unity already uses** | `availableSizes` is rebuilt from rows, so the database could be improved without changing or re-testing any Unity scripts. New fields (`sizeStock`) are extra and ignored by Unity's `JsonUtility`. |
| 7 | **Constraints in the database, validation in the API** | The API gives friendly 400/409 messages; the database guarantees correctness even if API code has a bug or someone edits data in `psql` or Prisma Studio. |
| 8 | **CASCADE for owned data, RESTRICT for shared data** | Specs and size links belong to one product, so they are deleted with it. Categories and sizes are shared, so they are protected while in use. |
| 9 | **Readable natural keys** for Category (`shirts`) and Product (`SH-001`), **surrogate key** for Size | Readable ids appear in API URLs and match the codes in the Unity catalogue. Size uses an auto number so a label can be corrected without touching link rows. |
| 10 | **Unity falls back to local JSON** | The VR demo still works if the API or database is down; the database data replaces it once loaded. |

## Data integrity summary

* **Entity integrity:** every table has a primary key; `ProductSize` has a composite key, which also prevents duplicate product-size links.
* **Referential integrity:** 4 foreign keys with deliberate `ON DELETE` rules (2 × CASCADE, 2 × RESTRICT).
* **Domain integrity:** `NOT NULL`, `UNIQUE` (category name, size label), `ENUM` (`SizeSystem`), and 8 `CHECK` constraints (product code format, slug format, non-blank text, `sortOrder > 0`, `stockQuantity >= 0`).
* **Transactions:** multi-table writes (create or edit a product) run in one transaction.
* **Application validation:** trimming, upper-casing, type and range checks, a clear list of errors, and mapping of database errors to HTTP 400/404/409.

All of the constraints are demonstrated failing on purpose in Q15 of
[06_Queries.md](06_Queries.md).

## Performance and indexing

| Access pattern in the app | Index used |
|---|---|
| Catalogue filtered by category (`/api/products?category=`) | `Product_categoryId_idx` (plan shown in Q16) |
| Sort catalogue by name | `Product_name_idx` |
| Open one product (`/api/products/:id`) | `Product_pkey` |
| Sizes and stock of a product | `ProductSize_pkey` (leading column `productId`) |
| Which products come in a size | `ProductSize_sizeId_idx` |
| Look up a size or category by name or label | `Size_label_key`, `Category_name_key` |

PostgreSQL does not index foreign-key columns automatically. The two
foreign-key columns that are not already first in a primary key
(`Product.categoryId`, `ProductSize.sizeId`) have their own index, so joins
and `ON DELETE RESTRICT` checks do not scan whole tables.

## Supporting more data and more users

The prototype has 8 products. The design copes with growth as follows:

* **More products.** All catalogue lookups use indexes, so their cost grows with the logarithm of the table size, not linearly. The next step is to add a `LIMIT`/`OFFSET`, or better keyset pagination (`WHERE name > $last ORDER BY name LIMIT 20`), to `GET /api/products`, so Unity loads one page at a time.
* **Search at scale.** `ILIKE '%cotton%'` cannot use a normal B-tree index. For thousands of products, add a `pg_trgm` GIN index on `Product.name` (or PostgreSQL full-text search) without changing the API.
* **More sizes or categories.** These are rows in lookup tables, not schema changes. A new size system (e.g. shoe sizes) is one new ENUM value.
* **More users (many headsets).** The catalogue is read-heavy. Prisma's `pg` adapter already pools connections. Responses can be cached for a short time, and reads can go to a PostgreSQL read replica, while writes (staff updates) stay on the primary.
* **Concurrent stock updates.** Stock changes are single-row `UPDATE`/upsert statements, which PostgreSQL locks row by row, so two staff members editing different sizes do not block each other.
* **Monitoring.** `EXPLAIN ANALYZE` (as in Q16) and the `pg_stat_statements` extension show which queries need new indexes as usage grows.

## Running it

See [`backend/README.md`](../backend/README.md) for setup, and the
[DE README](README.md) for running the DE SQL files directly in `psql`.
