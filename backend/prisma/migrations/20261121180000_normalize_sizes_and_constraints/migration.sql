-- Normalize product sizes (1NF) and add data-integrity constraints.
--
-- Before: ProductSpecifications."availableSizes" held a comma-separated list
--         such as 'S, M, L, XL' (a non-atomic, repeating value).
-- After:  each size label is stored once in "Size", and the many-to-many link
--         between products and sizes lives in "ProductSize".
-- Existing data is moved across before the old column is dropped, so no
-- product loses its sizes.

-- CreateEnum
CREATE TYPE "SizeSystem" AS ENUM ('ALPHA', 'WAIST');

-- CreateTable
CREATE TABLE "Size" (
    "id" SERIAL NOT NULL,
    "label" TEXT NOT NULL,
    "sizeSystem" "SizeSystem" NOT NULL,
    "sortOrder" INTEGER NOT NULL,

    CONSTRAINT "Size_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "ProductSize" (
    "productId" TEXT NOT NULL,
    "sizeId" INTEGER NOT NULL,
    "stockQuantity" INTEGER NOT NULL DEFAULT 0,

    CONSTRAINT "ProductSize_pkey" PRIMARY KEY ("productId","sizeId")
);

-- CreateIndex
CREATE UNIQUE INDEX "Size_label_key" ON "Size"("label");

-- CreateIndex
CREATE INDEX "ProductSize_sizeId_idx" ON "ProductSize"("sizeId");

-- AddForeignKey
ALTER TABLE "ProductSize" ADD CONSTRAINT "ProductSize_productId_fkey" FOREIGN KEY ("productId") REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "ProductSize" ADD CONSTRAINT "ProductSize_sizeId_fkey" FOREIGN KEY ("sizeId") REFERENCES "Size"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- Data migration step 1: one "Size" row per distinct label found in the old column.
INSERT INTO "Size" ("label", "sizeSystem", "sortOrder")
SELECT label,
       CASE WHEN label ~ '^[0-9]+$' THEN 'WAIST'::"SizeSystem" ELSE 'ALPHA'::"SizeSystem" END,
       CASE
           WHEN label ~ '^[0-9]+$' THEN label::INTEGER
           ELSE COALESCE(array_position(ARRAY['XXS','XS','S','M','L','XL','XXL','XXXL'], label), 99)
       END
FROM (
    SELECT DISTINCT upper(trim(part)) AS label
    FROM "ProductSpecifications",
         regexp_split_to_table("availableSizes", ',') AS part
    WHERE "availableSizes" IS NOT NULL AND trim(part) <> ''
) AS labels;

-- Data migration step 2: link each product to its sizes.
INSERT INTO "ProductSize" ("productId", "sizeId")
SELECT DISTINCT ps."productId", s."id"
FROM "ProductSpecifications" ps,
     regexp_split_to_table(ps."availableSizes", ',') AS part
JOIN "Size" s ON s."label" = upper(trim(part))
WHERE ps."availableSizes" IS NOT NULL AND trim(part) <> '';

-- AlterTable: the repeating column is no longer needed.
ALTER TABLE "ProductSpecifications" DROP COLUMN "availableSizes";

-- CHECK constraints (Prisma has no schema syntax for these, so they are added here).
ALTER TABLE "Category"
    ADD CONSTRAINT "Category_id_slug_check" CHECK ("id" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'),
    ADD CONSTRAINT "Category_name_not_blank_check" CHECK (btrim("name") <> '');

ALTER TABLE "Product"
    ADD CONSTRAINT "Product_id_format_check" CHECK ("id" ~ '^[A-Z]{2}-[0-9]{3}$'),
    ADD CONSTRAINT "Product_name_not_blank_check" CHECK (btrim("name") <> ''),
    ADD CONSTRAINT "Product_description_not_blank_check" CHECK (btrim("description") <> '');

ALTER TABLE "Size"
    ADD CONSTRAINT "Size_label_not_blank_check" CHECK (btrim("label") <> ''),
    ADD CONSTRAINT "Size_sortOrder_positive_check" CHECK ("sortOrder" > 0);

ALTER TABLE "ProductSize"
    ADD CONSTRAINT "ProductSize_stock_non_negative_check" CHECK ("stockQuantity" >= 0);
