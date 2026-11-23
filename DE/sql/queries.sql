-- ThreadHaven VR Showroom: DE project queries.
-- Every query answers a real need of the showroom (the Unity catalogue,
-- the product information panel, or catalogue/stock management).
--
-- Run on a fresh copy of the sample data, in order (later queries see the
-- changes made by earlier ones):
--   psql -d vr_showroom_de -f DE/sql/schema.sql
--   psql -d vr_showroom_de -f DE/sql/sample_data.sql
--   psql -d vr_showroom_de -f DE/sql/queries.sql
-- The output of this exact run is saved in DE/outputs/queries_output.txt.

\pset null '(null)'

\echo
\echo '=== Q1. INSERT: add a new arrival (product + specifications + sizes in one transaction) ==='
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

\echo
\echo '=== Q2. SELECT + JOIN + filter + sort: the Shirts page of the Unity catalogue ==='
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

\echo
\echo '=== Q3. SELECT with pattern filter: the showroom search box ("cotton") ==='
SELECT p."id", p."name", spec."material"
FROM "Product" p
LEFT JOIN "ProductSpecifications" spec ON spec."productId" = p."id"
WHERE p."name" ILIKE '%cotton%'
   OR spec."material" ILIKE '%cotton%'
ORDER BY p."id";

\echo
\echo '=== Q4. UPDATE: restock the Oxford shirt in XL (15 units arrived) ==='
UPDATE "ProductSize" ps
SET "stockQuantity" = ps."stockQuantity" + 15
FROM "Size" s
WHERE s."id" = ps."sizeId"
  AND ps."productId" = 'SH-002'
  AND s."label" = 'XL'
RETURNING ps."productId", s."label" AS size, ps."stockQuantity";

\echo
\echo '=== Q5. UPDATE: correct the care instructions shown in the product information panel ==='
UPDATE "ProductSpecifications"
SET "care" = 'Machine wash cold; tumble dry low'
WHERE "productId" = 'JK-001'
RETURNING "productId", "care";

UPDATE "Product" SET "updatedAt" = CURRENT_TIMESTAMP WHERE "id" = 'JK-001';

\echo
\echo '=== Q6. Aggregate functions: overall stock summary for the catalogue ==='
SELECT COUNT(DISTINCT ps."productId")          AS products_with_sizes,
       COUNT(*)                                AS product_size_rows,
       SUM(ps."stockQuantity")                 AS total_units,
       ROUND(AVG(ps."stockQuantity"), 2)       AS avg_units_per_size,
       MIN(ps."stockQuantity")                 AS min_units,
       MAX(ps."stockQuantity")                 AS max_units
FROM "ProductSize" ps;

\echo
\echo '=== Q7. GROUP BY: products and stock per category (category selection panel counts) ==='
SELECT c."name"                            AS category,
       COUNT(DISTINCT p."id")              AS products,
       COALESCE(SUM(ps."stockQuantity"), 0) AS total_units
FROM "Category" c
LEFT JOIN "Product" p      ON p."categoryId" = c."id"
LEFT JOIN "ProductSize" ps ON ps."productId" = p."id"
GROUP BY c."name"
ORDER BY total_units DESC;

\echo
\echo '=== Q8. GROUP BY + HAVING: restock priority - products with fewer than 30 units in total ==='
SELECT p."id", p."name", SUM(ps."stockQuantity") AS total_units
FROM "Product" p
JOIN "ProductSize" ps ON ps."productId" = p."id"
GROUP BY p."id", p."name"
HAVING SUM(ps."stockQuantity") < 30
ORDER BY total_units ASC;

\echo
\echo '=== Q9. Multi-table JOIN: low-stock alert (3 units or fewer) for the store team ==='
SELECT c."name" AS category, p."id", p."name", s."label" AS size, ps."stockQuantity"
FROM "ProductSize" ps
JOIN "Product"  p ON p."id" = ps."productId"
JOIN "Category" c ON c."id" = p."categoryId"
JOIN "Size"     s ON s."id" = ps."sizeId"
WHERE ps."stockQuantity" <= 3
ORDER BY ps."stockQuantity" ASC, c."name", s."sortOrder";

\echo
\echo '=== Q10. Subquery: best-stocked products (above the average total stock per product) ==='
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

\echo
\echo '=== Q11. EXISTS subquery: products with at least one size sold out (shown as "limited sizes" in VR) ==='
SELECT p."id", p."name"
FROM "Product" p
WHERE EXISTS (SELECT 1
              FROM "ProductSize" ps
              WHERE ps."productId" = p."id"
                AND ps."stockQuantity" = 0)
ORDER BY p."id";

\echo
\echo '=== Q12. CTE + window function: rank products by stock within each category ==='
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

\echo
\echo '=== Q13. DELETE: stop offering the formal dress in XS (remove one many-to-many link) ==='
DELETE FROM "ProductSize" ps
USING "Size" s
WHERE s."id" = ps."sizeId"
  AND ps."productId" = 'DR-002'
  AND s."label" = 'XS'
RETURNING ps."productId", s."label" AS removed_size;

\echo
\echo '=== Q14. DELETE with ON DELETE CASCADE: withdraw the trial linen shirt ==='
SELECT 'before' AS moment,
       (SELECT COUNT(*) FROM "Product" WHERE "id" = 'SH-003')                   AS product_rows,
       (SELECT COUNT(*) FROM "ProductSpecifications" WHERE "productId" = 'SH-003') AS spec_rows,
       (SELECT COUNT(*) FROM "ProductSize" WHERE "productId" = 'SH-003')         AS size_rows;

DELETE FROM "Product" WHERE "id" = 'SH-003';

SELECT 'after' AS moment,
       (SELECT COUNT(*) FROM "Product" WHERE "id" = 'SH-003')                   AS product_rows,
       (SELECT COUNT(*) FROM "ProductSpecifications" WHERE "productId" = 'SH-003') AS spec_rows,
       (SELECT COUNT(*) FROM "ProductSize" WHERE "productId" = 'SH-003')         AS size_rows;

\echo
\echo '=== Q15. Data integrity: each of these must be REJECTED by the database ==='
\echo '--- 15a. duplicate size label (UNIQUE) ---'
INSERT INTO "Size" ("label", "sizeSystem", "sortOrder") VALUES ('M', 'ALPHA', 4);
\echo '--- 15b. negative stock (CHECK) ---'
UPDATE "ProductSize" SET "stockQuantity" = -5 WHERE "productId" = 'SH-001';
\echo '--- 15c. product in a category that does not exist (FOREIGN KEY) ---'
INSERT INTO "Product" ("id", "name", "description", "categoryId", "updatedAt")
VALUES ('HT-001', 'Wool Beanie', 'A warm knitted hat.', 'hats', CURRENT_TIMESTAMP);
\echo '--- 15d. product without a name (NOT NULL) ---'
INSERT INTO "Product" ("id", "description", "categoryId", "updatedAt")
VALUES ('SH-009', 'No name given.', 'shirts', CURRENT_TIMESTAMP);
\echo '--- 15e. badly formatted product code (CHECK) ---'
INSERT INTO "Product" ("id", "name", "description", "categoryId", "updatedAt")
VALUES ('shirt4', 'Test', 'Bad id.', 'shirts', CURRENT_TIMESTAMP);
\echo '--- 15f. deleting a category that still has products (ON DELETE RESTRICT) ---'
DELETE FROM "Category" WHERE "id" = 'jackets';

\echo
\echo '=== Q16. Indexing: query plan for the catalogue category filter ==='
SET enable_seqscan = off;  -- the sample table is tiny; this shows the plan PostgreSQL uses at larger volume
EXPLAIN SELECT "id", "name" FROM "Product" WHERE "categoryId" = 'shirts';
RESET enable_seqscan;
