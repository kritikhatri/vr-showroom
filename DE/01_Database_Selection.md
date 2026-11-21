# 1. Database Selection and Justification

## Chosen database: PostgreSQL (relational)

The ThreadHaven VR Showroom uses **PostgreSQL 15+**. The backend (`backend/`)
reaches it through **Prisma ORM** and the `pg` driver adapter, and the Unity
client reads the data from the backend's REST API.

## What data the application stores

The showroom is a virtual garment store in Unity. A visitor picks a
**category**, browses **products** in that category, and opens a product to see
its **specifications** (material, fit, care) and the **sizes** it comes in.
The backend also lets staff manage the catalogue and the **stock level of each
size**.

| Data | Nature |
|---|---|
| Categories (Shirts, Jackets, Trousers, Dresses) | Small, fixed set; each name must be unique |
| Products | The core records; each belongs to exactly one category |
| Specifications | Optional descriptive details; at most one set per product |
| Sizes | Shared list of labels (XS–XL, waist 28–36) used by many products |
| Stock per product and size | A number that belongs to a *pair* (product, size) |

This data is **structured**: every product has the same fields. It is also
**highly related**: categories have products, and products and sizes are
linked many-to-many. Those are exactly the conditions a relational database is
designed for.

## Why PostgreSQL and not MongoDB

| Requirement of the showroom | PostgreSQL | MongoDB |
|---|---|---|
| Every product must belong to a category that really exists | Enforced by a **foreign key** | Must be checked in application code |
| A category that still has products must not be deleted | `ON DELETE RESTRICT` blocks it | Must be checked in application code |
| Deleting a product should remove its specs and size rows | `ON DELETE CASCADE` | Manual clean-up of referenced documents |
| Many-to-many products ↔ sizes, without repeating size data | Junction table `ProductSize` | Either arrays of embedded copies (duplication) or manual references |
| Stock can never be negative; product codes must look like `SH-001` | `CHECK` constraints in the database | Schema validation is optional and weaker |
| Reports for staff: totals per category, low-stock lists, rankings | `JOIN`, `GROUP BY`, `HAVING`, subqueries, window functions | Possible with aggregation pipelines, but more complex for multi-collection joins |
| Updating a product, its specs and its sizes together | Multi-table **ACID transactions** | Supported, but documents are usually designed to avoid needing them |
| Fixed, predictable schema shared by the Unity client and the API | Schema defined once (Prisma + migrations) | Flexible schema is not needed here |

MongoDB's main strengths are flexible, changing document shapes and very
large write-heavy workloads. Neither applies to a product catalogue whose
structure is fixed and whose correctness depends on relationships between
records.

## Why PostgreSQL in particular (rather than another SQL database)

* **Free and open source.** Every team member can run it locally (see `backend/README.md`).
* **Rich SQL.** CTEs, window functions, `string_agg`, `FILTER` and `ILIKE` are used in our queries (see [06_Queries.md](06_Queries.md)).
* **Native ENUM types and regular-expression `CHECK` constraints.** These are used for `SizeSystem` and the product-code format.
* **First-class Prisma support.** The schema is versioned as migrations in Git, so every teammate gets an identical database with `npm run db:setup`.
* **Scales beyond the prototype.** It has B-tree indexes, connection pooling and read replicas when needed (see [08_Architecture_and_Decisions.md](08_Architecture_and_Decisions.md)).

## Summary

The showroom's data is structured, relational and must stay consistent: no
orphan products, no negative stock, no duplicated size data. PostgreSQL
enforces all of this inside the database itself and supports the reporting
queries staff need, so it is the most suitable choice for this project.
