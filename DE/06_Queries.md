# 6. Queries and Operations

16 queries written for the ThreadHaven VR showroom, each tied to a real feature of the application (the Unity catalogue, the product information panel, or catalogue and stock management). The guideline minimum is 10.

* **SQL file:** [`sql/queries.sql`](sql/queries.sql)
* **Full run log (SQL echoed together with the output):** [`outputs/queries_output.txt`](outputs/queries_output.txt)

**How to run** (PostgreSQL 15+):

```sh
createdb vr_showroom_de
psql -d vr_showroom_de -f DE/sql/schema.sql
psql -d vr_showroom_de -f DE/sql/sample_data.sql
psql -d vr_showroom_de -f DE/sql/queries.sql
```

The queries run **in order** on fresh sample data, and later queries see earlier changes. For example, the new shirt added in Q1 appears in Q2, Q6 and Q12, and Q14 removes it again. Run the three commands again on a new database to repeat the demo.

## Coverage of the required concepts

| Required concept | Queries |
|---|---|
| INSERT | Q1 |
| UPDATE | Q4, Q5 |
| DELETE | Q13, Q14 |
| SELECT with filtering and sorting | Q2, Q3, Q9 |
| Aggregate functions | Q6, Q7, Q8 |
| GROUP BY and HAVING | Q7, Q8 (HAVING) |
| JOIN operations | Q1, Q2, Q4, Q7, Q9, Q12, Q13 |
| Subqueries / advanced | Q10, Q11 (EXISTS), Q12 (CTE + window function), Q14 |
| Constraints and indexing | Q15, Q16 |

| # | Purpose | Concepts |
|---|---|---|
| [Q1](#q1) | add a new arrival (product + specifications + sizes in one transaction) | INSERT (with a transaction) |
| [Q2](#q2) | the Shirts page of the Unity catalogue | SELECT, JOIN, WHERE filter, ORDER BY, string_agg |
| [Q3](#q3) | the showroom search box ("cotton") | SELECT with ILIKE pattern matching and OR |
| [Q4](#q4) | restock the Oxford shirt in XL (15 units arrived) | UPDATE ... FROM (join in an update), RETURNING |
| [Q5](#q5) | correct the care instructions shown in the product information panel | UPDATE, RETURNING |
| [Q6](#q6) | overall stock summary for the catalogue | Aggregate functions: COUNT, COUNT(DISTINCT), SUM, AVG, MIN, MAX |
| [Q7](#q7) | products and stock per category (category selection panel counts) | GROUP BY with LEFT JOIN and COALESCE |
| [Q8](#q8) | restock priority - products with fewer than 30 units in total | GROUP BY + HAVING |
| [Q9](#q9) | low-stock alert (3 units or fewer) for the store team | Four-table JOIN, WHERE, multi-column ORDER BY |
| [Q10](#q10) | best-stocked products (above the average total stock per product) | Subqueries (derived table in FROM + scalar subquery in WHERE) |
| [Q11](#q11) | products with at least one size sold out (shown as "limited sizes" in VR) | Correlated subquery with EXISTS |
| [Q12](#q12) | rank products by stock within each category | CTE (WITH) + window function RANK() OVER (PARTITION BY ...) |
| [Q13](#q13) | stop offering the formal dress in XS (remove one many-to-many link) | DELETE ... USING (join in a delete), RETURNING |
| [Q14](#q14) | withdraw the trial linen shirt | DELETE with ON DELETE CASCADE |
| [Q15](#q15) | each of these must be REJECTED by the database | Constraint enforcement: UNIQUE, CHECK, FOREIGN KEY, NOT NULL, ON DELETE RESTRICT |
| [Q16](#q16) | query plan for the catalogue category filter | EXPLAIN (query plan), index usage |

## Q1
### INSERT: add a new arrival (product + specifications + sizes in one transaction)

**Concepts:** INSERT (with a transaction)  
**Used by:** `POST /api/products`

Adds a new arrival to the showroom. A product is spread over three tables (Product, ProductSpecifications, ProductSize), so the three inserts are wrapped in `BEGIN ... COMMIT`: either the whole product is saved or nothing is. The size rows are inserted with `INSERT ... SELECT` so the query looks up each size's `id` from its label instead of hard-coding numbers. The SELECT afterwards confirms the new rows.

```sql
BEGIN;
INSERT INTO "Product" ("id", "name", "description", "imagePath", "categoryId", "updatedAt")
VALUES ('SH-003', 'Linen Short-Sleeve Shirt',
        'A light linen shirt with a camp collar for warm days.',
        NULL, 'shirts', CURRENT_TIMESTAMP);

INSERT INTO "ProductSpecifications" ("productId", "material", "fit", "care")
VALUES ('SH-003', '100% linen', 'Relaxed', 'Hand wash cold');

INSERT INTO "ProductSize" ("productId", "sizeId", "stockQuantity")
SELECT 'SH-003', s."id", v.stock
FROM (VALUES ('S', 8), ('M', 12), ('L', 10)) AS v(label, stock)
JOIN "Size" s ON s."label" = v.label;
COMMIT;

SELECT p."id", p."name", c."name" AS category, ps."stockQuantity", s."label" AS size
FROM "Product" p
JOIN "Category" c     ON c."id" = p."categoryId"
JOIN "ProductSize" ps ON ps."productId" = p."id"
JOIN "Size" s         ON s."id" = ps."sizeId"
WHERE p."id" = 'SH-003'
ORDER BY s."sortOrder";
```

**Output:**

```text
id   |           name           | category | stockQuantity | size 
--------+--------------------------+----------+---------------+------
 SH-003 | Linen Short-Sleeve Shirt | Shirts   |             8 | S
 SH-003 | Linen Short-Sleeve Shirt | Shirts   |            12 | M
 SH-003 | Linen Short-Sleeve Shirt | Shirts   |            10 | L
(3 rows)
```

## Q2
### SELECT + JOIN + filter + sort: the Shirts page of the Unity catalogue

**Concepts:** SELECT, JOIN, WHERE filter, ORDER BY, string_agg  
**Used by:** `GET /api/products?category=Shirts` (Unity category panel)

This is the data Unity needs for one category page. It joins four tables. `LEFT JOIN` keeps a product even if it has no specifications or sizes yet. `string_agg(... ORDER BY sortOrder)` rebuilds the `availableSizes` text ("S, M, L, XL") that the Unity product information panel displays, in the correct size order rather than alphabetical order.

```sql
SELECT p."id",
       p."name",
       spec."material",
       spec."fit",
       string_agg(s."label", ', ' ORDER BY s."sortOrder") AS "availableSizes"
FROM "Product" p
JOIN "Category" c                    ON c."id" = p."categoryId"
LEFT JOIN "ProductSpecifications" spec ON spec."productId" = p."id"
LEFT JOIN "ProductSize" ps           ON ps."productId" = p."id"
LEFT JOIN "Size" s                   ON s."id" = ps."sizeId"
WHERE c."name" = 'Shirts'
GROUP BY p."id", p."name", spec."material", spec."fit"
ORDER BY p."name" ASC;
```

**Output:**

```text
id   |               name               |      material       |   fit   | availableSizes 
--------+----------------------------------+---------------------+---------+----------------
 SH-001 | Classic Cotton Crew Neck T-Shirt | 100% cotton         | Regular | S, M, L, XL
 SH-003 | Linen Short-Sleeve Shirt         | 100% linen          | Relaxed | S, M, L
 SH-002 | Oxford Button-Down Shirt         | Cotton Oxford cloth | Regular | S, M, L, XL
(3 rows)
```

## Q3
### SELECT with pattern filter: the showroom search box ("cotton")

**Concepts:** SELECT with ILIKE pattern matching and OR  
**Used by:** `GET /api/products?search=cotton` (search box)

Case-insensitive search used by the showroom search feature. It matches the product name or the material, so "cotton" also finds the Oxford shirt ("Cotton Oxford cloth") and the trousers made from cotton blends.

```sql
SELECT p."id", p."name", spec."material"
FROM "Product" p
LEFT JOIN "ProductSpecifications" spec ON spec."productId" = p."id"
WHERE p."name" ILIKE '%cotton%'
   OR spec."material" ILIKE '%cotton%'
ORDER BY p."id";
```

**Output:**

```text
id   |               name               |        material         
--------+----------------------------------+-------------------------
 DR-001 | Everyday A-Line Dress            | Cotton poplin
 JK-001 | Lightweight Utility Jacket       | Cotton-polyester twill
 SH-001 | Classic Cotton Crew Neck T-Shirt | 100% cotton
 SH-002 | Oxford Button-Down Shirt         | Cotton Oxford cloth
 TR-001 | Straight-Leg Work Trousers       | Cotton-polyester canvas
 TR-002 | Stretch Chino Trousers           | Cotton with elastane
(6 rows)
```

## Q4
### UPDATE: restock the Oxford shirt in XL (15 units arrived)

**Concepts:** UPDATE ... FROM (join in an update), RETURNING  
**Used by:** `PUT /api/products/SH-002/sizes/XL`

A delivery of 15 Oxford shirts in XL arrives. Stock is stored per product *and* size, so the update targets exactly one `ProductSize` row; it joins `Size` so staff can use the label "XL" instead of an internal id. `RETURNING` shows the new value (0 → 15) without a second query.

```sql
UPDATE "ProductSize" ps
SET "stockQuantity" = ps."stockQuantity" + 15
FROM "Size" s
WHERE s."id" = ps."sizeId"
  AND ps."productId" = 'SH-002'
  AND s."label" = 'XL'
RETURNING ps."productId", s."label" AS size, ps."stockQuantity";
```

**Output:**

```text
productId | size | stockQuantity 
-----------+------+---------------
 SH-002    | XL   |            15
(1 row)
```

## Q5
### UPDATE: correct the care instructions shown in the product information panel

**Concepts:** UPDATE, RETURNING  
**Used by:** `PATCH /api/products/JK-001`

Corrects the care text that the VR information panel shows for the utility jacket. Because specifications live in their own table, only one row in one table changes. The second statement refreshes `updatedAt` on the product (the API does this automatically through Prisma's `@updatedAt`).

```sql
UPDATE "ProductSpecifications"
SET "care" = 'Machine wash cold; tumble dry low'
WHERE "productId" = 'JK-001'
RETURNING "productId", "care";

UPDATE "Product" SET "updatedAt" = CURRENT_TIMESTAMP WHERE "id" = 'JK-001';
```

**Output:**

```text
productId |               care                
-----------+-----------------------------------
 JK-001    | Machine wash cold; tumble dry low
(1 row)
```

## Q6
### Aggregate functions: overall stock summary for the catalogue

**Concepts:** Aggregate functions: COUNT, COUNT(DISTINCT), SUM, AVG, MIN, MAX  
**Used by:** Catalogue/stock dashboard

One-line health check of the whole catalogue: how many products have sizes, how many product-size combinations exist, total units in stock, and the average, lowest and highest stock of a single size. Run after Q1 and Q4, so it includes the new linen shirt (30 units) and the 15 restocked shirts.

```sql
SELECT COUNT(DISTINCT ps."productId")          AS products_with_sizes,
       COUNT(*)                                AS product_size_rows,
       SUM(ps."stockQuantity")                 AS total_units,
       ROUND(AVG(ps."stockQuantity"), 2)       AS avg_units_per_size,
       MIN(ps."stockQuantity")                 AS min_units,
       MAX(ps."stockQuantity")                 AS max_units
FROM "ProductSize" ps;
```

**Output:**

```text
products_with_sizes | product_size_rows | total_units | avg_units_per_size | min_units | max_units 
---------------------+-------------------+-------------+--------------------+-----------+-----------
                   9 |                39 |         427 |              10.95 |         0 |        25
(1 row)
```

## Q7
### GROUP BY: products and stock per category (category selection panel counts)

**Concepts:** GROUP BY with LEFT JOIN and COALESCE  
**Used by:** `GET /api/categories` (productCount) and `GET /api/reports/stock-by-category`

Summarises each category: number of products and total units. `LEFT JOIN` + `COALESCE(..., 0)` means a newly created category with no products would still be listed with 0 instead of disappearing. The category counts are what the Unity category panel can show next to each button.

```sql
SELECT c."name"                            AS category,
       COUNT(DISTINCT p."id")              AS products,
       COALESCE(SUM(ps."stockQuantity"), 0) AS total_units
FROM "Category" c
LEFT JOIN "Product" p      ON p."categoryId" = c."id"
LEFT JOIN "ProductSize" ps ON ps."productId" = p."id"
GROUP BY c."name"
ORDER BY total_units DESC;
```

**Output:**

```text
category | products | total_units 
----------+----------+-------------
 Shirts   |        3 |         167
 Trousers |        2 |         127
 Dresses  |        2 |          72
 Jackets  |        2 |          61
(4 rows)
```

## Q8
### GROUP BY + HAVING: restock priority - products with fewer than 30 units in total

**Concepts:** GROUP BY + HAVING  
**Used by:** Restock planning

`WHERE` filters rows before grouping; `HAVING` filters the groups afterwards. Here the stock is totalled per product and only products with fewer than 30 units overall are kept, giving the buying team a restock priority list.

```sql
SELECT p."id", p."name", SUM(ps."stockQuantity") AS total_units
FROM "Product" p
JOIN "ProductSize" ps ON ps."productId" = p."id"
GROUP BY p."id", p."name"
HAVING SUM(ps."stockQuantity") < 30
ORDER BY total_units ASC;
```

**Output:**

```text
id   |           name           | total_units 
--------+--------------------------+-------------
 JK-002 | Quilted Insulated Jacket |          25
 DR-002 | Formal Woven Midi Dress  |          25
(2 rows)
```

## Q9
### Multi-table JOIN: low-stock alert (3 units or fewer) for the store team

**Concepts:** Four-table JOIN, WHERE, multi-column ORDER BY  
**Used by:** Low-stock alert

Lists every product-size combination with 3 units or fewer, with the category, product name and size label joined in so the result is readable by store staff. Sorted with sold-out items first.

```sql
SELECT c."name" AS category, p."id", p."name", s."label" AS size, ps."stockQuantity"
FROM "ProductSize" ps
JOIN "Product"  p ON p."id" = ps."productId"
JOIN "Category" c ON c."id" = p."categoryId"
JOIN "Size"     s ON s."id" = ps."sizeId"
WHERE ps."stockQuantity" <= 3
ORDER BY ps."stockQuantity" ASC, c."name", s."sortOrder";
```

**Output:**

```text
category |   id   |            name            | size | stockQuantity 
----------+--------+----------------------------+------+---------------
 Dresses  | DR-002 | Formal Woven Midi Dress    | XS   |             0
 Trousers | TR-001 | Straight-Leg Work Trousers | 36   |             2
 Dresses  | DR-002 | Formal Woven Midi Dress    | XL   |             3
 Jackets  | JK-002 | Quilted Insulated Jacket   | XL   |             3
(4 rows)
```

## Q10
### Subquery: best-stocked products (above the average total stock per product)

**Concepts:** Subqueries (derived table in FROM + scalar subquery in WHERE)  
**Used by:** Merchandising: which products to feature

The inner query in `FROM` totals stock per product. The scalar subquery in `WHERE` calculates the average of those totals across all products (about 47 units). Only products above that average are returned; these are well stocked and safe to feature prominently in the VR showroom.

```sql
SELECT p."id", p."name", t.total_units
FROM "Product" p
JOIN (SELECT "productId", SUM("stockQuantity") AS total_units
      FROM "ProductSize"
      GROUP BY "productId") t ON t."productId" = p."id"
WHERE t.total_units > (SELECT AVG(per_product.total)
                       FROM (SELECT SUM("stockQuantity") AS total
                             FROM "ProductSize"
                             GROUP BY "productId") per_product)
ORDER BY t.total_units DESC;
```

**Output:**

```text
id   |               name               | total_units 
--------+----------------------------------+-------------
 SH-001 | Classic Cotton Crew Neck T-Shirt |          74
 TR-002 | Stretch Chino Trousers           |          73
 SH-002 | Oxford Button-Down Shirt         |          63
 TR-001 | Straight-Leg Work Trousers       |          54
(4 rows)
```

## Q11
### EXISTS subquery: products with at least one size sold out (shown as "limited sizes" in VR)

**Concepts:** Correlated subquery with EXISTS  
**Used by:** "Limited sizes" label in VR

For each product, the subquery checks whether *any* of its sizes has 0 stock. `EXISTS` stops at the first match, so it is efficient. After the Q4 restock, only the formal midi dress (XS sold out) qualifies.

```sql
SELECT p."id", p."name"
FROM "Product" p
WHERE EXISTS (SELECT 1
              FROM "ProductSize" ps
              WHERE ps."productId" = p."id"
                AND ps."stockQuantity" = 0)
ORDER BY p."id";
```

**Output:**

```text
id   |          name           
--------+-------------------------
 DR-002 | Formal Woven Midi Dress
(1 row)
```

## Q12
### CTE + window function: rank products by stock within each category

**Concepts:** CTE (WITH) + window function RANK() OVER (PARTITION BY ...)  
**Used by:** Category bestseller/stock ranking

The CTE first totals stock per product. `RANK()` then numbers products within each category separately (`PARTITION BY category`), which a plain `GROUP BY` cannot do while still showing every product row.

```sql
WITH product_totals AS (
    SELECT p."id", p."name", c."name" AS category, SUM(ps."stockQuantity") AS total_units
    FROM "Product" p
    JOIN "Category" c     ON c."id" = p."categoryId"
    JOIN "ProductSize" ps ON ps."productId" = p."id"
    GROUP BY p."id", p."name", c."name"
)
SELECT category, "id", "name", total_units,
       RANK() OVER (PARTITION BY category ORDER BY total_units DESC) AS rank_in_category
FROM product_totals
ORDER BY category, rank_in_category;
```

**Output:**

```text
category |   id   |               name               | total_units | rank_in_category 
----------+--------+----------------------------------+-------------+------------------
 Dresses  | DR-001 | Everyday A-Line Dress            |          47 |                1
 Dresses  | DR-002 | Formal Woven Midi Dress          |          25 |                2
 Jackets  | JK-001 | Lightweight Utility Jacket       |          36 |                1
 Jackets  | JK-002 | Quilted Insulated Jacket         |          25 |                2
 Shirts   | SH-001 | Classic Cotton Crew Neck T-Shirt |          74 |                1
 Shirts   | SH-002 | Oxford Button-Down Shirt         |          63 |                2
 Shirts   | SH-003 | Linen Short-Sleeve Shirt         |          30 |                3
 Trousers | TR-002 | Stretch Chino Trousers           |          73 |                1
 Trousers | TR-001 | Straight-Leg Work Trousers       |          54 |                2
(9 rows)
```

## Q13
### DELETE: stop offering the formal dress in XS (remove one many-to-many link)

**Concepts:** DELETE ... USING (join in a delete), RETURNING  
**Used by:** `DELETE /api/products/DR-002/sizes/XS`

The formal dress will no longer be made in XS. Only the link row in the many-to-many table is removed; the product and the size "XS" itself stay, because other products (the A-line dress) still use XS.

```sql
DELETE FROM "ProductSize" ps
USING "Size" s
WHERE s."id" = ps."sizeId"
  AND ps."productId" = 'DR-002'
  AND s."label" = 'XS'
RETURNING ps."productId", s."label" AS removed_size;
```

**Output:**

```text
productId | removed_size 
-----------+--------------
 DR-002    | XS
(1 row)
```

## Q14
### DELETE with ON DELETE CASCADE: withdraw the trial linen shirt

**Concepts:** DELETE with ON DELETE CASCADE  
**Used by:** `DELETE /api/products/SH-003`

Withdraws the trial linen shirt added in Q1. Only one `DELETE` is written, on `Product`, but the before/after counts prove that PostgreSQL also removed its specifications row and its three size rows automatically (`ON DELETE CASCADE`). No orphan rows are left behind.

```sql
SELECT 'before' AS moment,
       (SELECT COUNT(*) FROM "Product" WHERE "id" = 'SH-003')                   AS product_rows,
       (SELECT COUNT(*) FROM "ProductSpecifications" WHERE "productId" = 'SH-003') AS spec_rows,
       (SELECT COUNT(*) FROM "ProductSize" WHERE "productId" = 'SH-003')         AS size_rows;

DELETE FROM "Product" WHERE "id" = 'SH-003';

SELECT 'after' AS moment,
       (SELECT COUNT(*) FROM "Product" WHERE "id" = 'SH-003')                   AS product_rows,
       (SELECT COUNT(*) FROM "ProductSpecifications" WHERE "productId" = 'SH-003') AS spec_rows,
       (SELECT COUNT(*) FROM "ProductSize" WHERE "productId" = 'SH-003')         AS size_rows;
```

**Output:**

```text
moment | product_rows | spec_rows | size_rows 
--------+--------------+-----------+-----------
 before |            1 |         1 |         3
(1 row)

 moment | product_rows | spec_rows | size_rows 
--------+--------------+-----------+-----------
 after  |            0 |         0 |         0
(1 row)
```

## Q15
### Data integrity: each of these must be REJECTED by the database

**Concepts:** Constraint enforcement: UNIQUE, CHECK, FOREIGN KEY, NOT NULL, ON DELETE RESTRICT  
**Used by:** API returns 400 / 409 for these cases

Each statement tries to save bad data and is rejected by the database itself, so bad data is blocked even if a bug in the API let it through: a duplicate size label, negative stock, a product in a category that does not exist, a product with no name, a badly formatted product code, and deleting a category that still contains products. The errors shown are the real PostgreSQL messages.

```sql
-- 15a. duplicate size label (UNIQUE)
INSERT INTO "Size" ("label", "sizeSystem", "sortOrder") VALUES ('M', 'ALPHA', 4);
-- 15b. negative stock (CHECK)
UPDATE "ProductSize" SET "stockQuantity" = -5 WHERE "productId" = 'SH-001';
-- 15c. product in a category that does not exist (FOREIGN KEY)
INSERT INTO "Product" ("id", "name", "description", "categoryId", "updatedAt")
VALUES ('HT-001', 'Wool Beanie', 'A warm knitted hat.', 'hats', CURRENT_TIMESTAMP);
-- 15d. product without a name (NOT NULL)
INSERT INTO "Product" ("id", "description", "categoryId", "updatedAt")
VALUES ('SH-009', 'No name given.', 'shirts', CURRENT_TIMESTAMP);
-- 15e. badly formatted product code (CHECK)
INSERT INTO "Product" ("id", "name", "description", "categoryId", "updatedAt")
VALUES ('shirt4', 'Test', 'Bad id.', 'shirts', CURRENT_TIMESTAMP);
-- 15f. deleting a category that still has products (ON DELETE RESTRICT)
DELETE FROM "Category" WHERE "id" = 'jackets';
```

**Output:**

```text
--- 15a. duplicate size label (UNIQUE) ---
ERROR:  duplicate key value violates unique constraint "Size_label_key"
DETAIL:  Key (label)=(M) already exists.
--- 15b. negative stock (CHECK) ---
ERROR:  new row for relation "ProductSize" violates check constraint "ProductSize_stock_non_negative_check"
DETAIL:  Failing row contains (SH-001, 2, -5).
--- 15c. product in a category that does not exist (FOREIGN KEY) ---
ERROR:  insert or update on table "Product" violates foreign key constraint "Product_categoryId_fkey"
DETAIL:  Key (categoryId)=(hats) is not present in table "Category".
--- 15d. product without a name (NOT NULL) ---
ERROR:  null value in column "name" of relation "Product" violates not-null constraint
DETAIL:  Failing row contains (SH-009, null, No name given., null, shirts, 2026-11-21 17:01:28.574, 2026-11-21 17:01:28.574).
--- 15e. badly formatted product code (CHECK) ---
ERROR:  new row for relation "Product" violates check constraint "Product_id_format_check"
DETAIL:  Failing row contains (shirt4, Test, Bad id., null, shirts, 2026-11-21 17:01:28.574, 2026-11-21 17:01:28.574).
--- 15f. deleting a category that still has products (ON DELETE RESTRICT) ---
ERROR:  update or delete on table "Category" violates foreign key constraint "Product_categoryId_fkey" on table "Product"
DETAIL:  Key (id)=(jackets) is still referenced from table "Product".
```

## Q16
### Indexing: query plan for the catalogue category filter

**Concepts:** EXPLAIN (query plan), index usage  
**Used by:** Performance check for `GET /api/products?category=...`

Shows that filtering by category uses the `Product_categoryId_idx` index (an index scan) instead of reading the whole table. With only 8 sample rows PostgreSQL would normally just read the table, because that is cheaper at this size, so sequential scans are switched off for this one statement to show the plan it uses at realistic data volumes.

```sql
SET enable_seqscan = off;  -- the sample table is tiny; this shows the plan PostgreSQL uses at larger volume
EXPLAIN SELECT "id", "name" FROM "Product" WHERE "categoryId" = 'shirts';
RESET enable_seqscan;
```

**Output:**

```text
QUERY PLAN                                       
---------------------------------------------------------------------------------------
 Bitmap Heap Scan on "Product"  (cost=4.16..9.50 rows=2 width=64)
   Recheck Cond: ("categoryId" = 'shirts'::text)
   ->  Bitmap Index Scan on "Product_categoryId_idx"  (cost=0.00..4.16 rows=2 width=0)
         Index Cond: ("categoryId" = 'shirts'::text)
(4 rows)
```

## Prisma equivalents

The API uses Prisma, which generates SQL like the queries above. For example:

| API code (Prisma) | SQL it runs (simplified) |
|---|---|
| `prisma.product.findMany({ where: { category: { name: 'Shirts' } }, include: {...} })` | `SELECT ... FROM "Product" JOIN "Category" ... WHERE "Category"."name" = 'Shirts'` (Q2) |
| `prisma.productSize.upsert({ where: { productId_sizeId }, update: { stockQuantity } })` | `INSERT ... ON CONFLICT` or `UPDATE "ProductSize" SET "stockQuantity" = ...` (Q4) |
| `prisma.product.delete({ where: { id } })` | `DELETE FROM "Product" WHERE "id" = ...` and the cascade removes the child rows (Q14) |
| `prisma.$queryRaw` in `/api/reports/stock-by-category` | Raw SQL with `LEFT JOIN`, `GROUP BY`, `SUM`, `COUNT ... FILTER` (Q7 extended) |
