-- ThreadHaven VR Showroom: final normalized PostgreSQL schema (3NF).
--
-- This file builds the same structure as the two Prisma migrations in
-- backend/prisma/migrations (init + normalize_sizes_and_constraints), in one
-- readable script. The application itself is set up with `npm run db:setup`;
-- this file is for reading, marking and running the DE queries directly in psql:
--
--   createdb vr_showroom_de
--   psql -d vr_showroom_de -f DE/sql/schema.sql
--   psql -d vr_showroom_de -f DE/sql/sample_data.sql

-- Sizing scale a size label belongs to.
CREATE TYPE "SizeSystem" AS ENUM ('ALPHA', 'WAIST');

-- One row per garment category shown in the Unity category panel.
CREATE TABLE "Category" (
    "id"   TEXT NOT NULL,                       -- slug, e.g. 'shirts'
    "name" TEXT NOT NULL,                       -- display name, e.g. 'Shirts'

    CONSTRAINT "Category_pkey" PRIMARY KEY ("id"),
    CONSTRAINT "Category_id_slug_check" CHECK ("id" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'),
    CONSTRAINT "Category_name_not_blank_check" CHECK (btrim("name") <> '')
);
CREATE UNIQUE INDEX "Category_name_key" ON "Category"("name");

-- One row per product displayed in the showroom.
CREATE TABLE "Product" (
    "id"          TEXT NOT NULL,                -- product code, e.g. 'SH-001'
    "name"        TEXT NOT NULL,
    "description" TEXT NOT NULL,
    "imagePath"   TEXT,                         -- optional thumbnail path for Unity
    "categoryId"  TEXT NOT NULL,                -- FK -> Category (many products : one category)
    "createdAt"   TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt"   TIMESTAMP(3) NOT NULL,        -- maintained by Prisma (@updatedAt)

    CONSTRAINT "Product_pkey" PRIMARY KEY ("id"),
    CONSTRAINT "Product_id_format_check" CHECK ("id" ~ '^[A-Z]{2}-[0-9]{3}$'),
    CONSTRAINT "Product_name_not_blank_check" CHECK (btrim("name") <> ''),
    CONSTRAINT "Product_description_not_blank_check" CHECK (btrim("description") <> '')
);
CREATE INDEX "Product_categoryId_idx" ON "Product"("categoryId");
CREATE INDEX "Product_name_idx" ON "Product"("name");

-- Optional details shown in the product information panel (one-to-one with Product).
CREATE TABLE "ProductSpecifications" (
    "productId" TEXT NOT NULL,                  -- PK and FK -> Product
    "material"  TEXT,
    "fit"       TEXT,
    "care"      TEXT,

    CONSTRAINT "ProductSpecifications_pkey" PRIMARY KEY ("productId")
);

-- Lookup table: every size label is stored exactly once.
CREATE TABLE "Size" (
    "id"         SERIAL NOT NULL,
    "label"      TEXT NOT NULL,                 -- 'XS'..'XL' or waist '28'..'36'
    "sizeSystem" "SizeSystem" NOT NULL,
    "sortOrder"  INTEGER NOT NULL,              -- display order (XS before S before M ...)

    CONSTRAINT "Size_pkey" PRIMARY KEY ("id"),
    CONSTRAINT "Size_label_not_blank_check" CHECK (btrim("label") <> ''),
    CONSTRAINT "Size_sortOrder_positive_check" CHECK ("sortOrder" > 0)
);
CREATE UNIQUE INDEX "Size_label_key" ON "Size"("label");

-- Junction table: many-to-many between Product and Size, with stock per pair.
CREATE TABLE "ProductSize" (
    "productId"     TEXT NOT NULL,              -- FK -> Product
    "sizeId"        INTEGER NOT NULL,           -- FK -> Size
    "stockQuantity" INTEGER NOT NULL DEFAULT 0,

    CONSTRAINT "ProductSize_pkey" PRIMARY KEY ("productId", "sizeId"),
    CONSTRAINT "ProductSize_stock_non_negative_check" CHECK ("stockQuantity" >= 0)
);
CREATE INDEX "ProductSize_sizeId_idx" ON "ProductSize"("sizeId");

-- Foreign keys
ALTER TABLE "Product"
    ADD CONSTRAINT "Product_categoryId_fkey" FOREIGN KEY ("categoryId")
    REFERENCES "Category"("id") ON DELETE RESTRICT ON UPDATE CASCADE;   -- cannot delete a category that still has products

ALTER TABLE "ProductSpecifications"
    ADD CONSTRAINT "ProductSpecifications_productId_fkey" FOREIGN KEY ("productId")
    REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE;     -- specs are removed with their product

ALTER TABLE "ProductSize"
    ADD CONSTRAINT "ProductSize_productId_fkey" FOREIGN KEY ("productId")
    REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE;     -- size links are removed with their product

ALTER TABLE "ProductSize"
    ADD CONSTRAINT "ProductSize_sizeId_fkey" FOREIGN KEY ("sizeId")
    REFERENCES "Size"("id") ON DELETE RESTRICT ON UPDATE CASCADE;       -- cannot delete a size still offered by a product
