# 4. Final Database Schema

The schema exists in three equivalent forms:

| Form | File | Used for |
|---|---|---|
| Prisma schema | [`backend/prisma/schema.prisma`](../backend/prisma/schema.prisma) | Source of truth for the application and the type-safe client |
| Prisma migrations | [`backend/prisma/migrations/`](../backend/prisma/migrations) | Applied by `npm run db:setup`; versioned in Git |
| Plain SQL | [`sql/schema.sql`](sql/schema.sql) | Reading, marking and running the DE queries in `psql` |

`sql/schema.sql` was checked against the migrations: a database built from it
and one built from the two migrations give identical `pg_dump --schema-only`
output.

## Tables

### Category

| Column | Type | Null | Default | Constraint |
|---|---|---|---|---|
| id | TEXT | NOT NULL | none | **PK**; CHECK slug format `^[a-z0-9]+(-[a-z0-9]+)*$` |
| name | TEXT | NOT NULL | none | **UNIQUE**; CHECK not blank |

### Product

| Column | Type | Null | Default | Constraint |
|---|---|---|---|---|
| id | TEXT | NOT NULL | none | **PK**; CHECK format `^[A-Z]{2}-[0-9]{3}$` (e.g. SH-001) |
| name | TEXT | NOT NULL | none | CHECK not blank; indexed |
| description | TEXT | NOT NULL | none | CHECK not blank |
| imagePath | TEXT | NULL | none | none |
| categoryId | TEXT | NOT NULL | none | **FK** → Category(id) ON DELETE RESTRICT ON UPDATE CASCADE; indexed |
| createdAt | TIMESTAMP(3) | NOT NULL | CURRENT_TIMESTAMP | none |
| updatedAt | TIMESTAMP(3) | NOT NULL | none | set by Prisma `@updatedAt` |

### ProductSpecifications

| Column | Type | Null | Default | Constraint |
|---|---|---|---|---|
| productId | TEXT | NOT NULL | none | **PK** and **FK** → Product(id) ON DELETE CASCADE |
| material | TEXT | NULL | none | none |
| fit | TEXT | NULL | none | none |
| care | TEXT | NULL | none | none |

### Size

| Column | Type | Null | Default | Constraint |
|---|---|---|---|---|
| id | SERIAL (INTEGER) | NOT NULL | auto-increment | **PK** |
| label | TEXT | NOT NULL | none | **UNIQUE**; CHECK not blank |
| sizeSystem | ENUM `SizeSystem` ('ALPHA', 'WAIST') | NOT NULL | none | only the two enum values allowed |
| sortOrder | INTEGER | NOT NULL | none | CHECK > 0 |

### ProductSize (junction table, M : N)

| Column | Type | Null | Default | Constraint |
|---|---|---|---|---|
| productId | TEXT | NOT NULL | none | **PK (part 1)**; **FK** → Product(id) ON DELETE CASCADE |
| sizeId | INTEGER | NOT NULL | none | **PK (part 2)**; **FK** → Size(id) ON DELETE RESTRICT; indexed |
| stockQuantity | INTEGER | NOT NULL | 0 | CHECK >= 0 |

## Indexes

| Index | Table (columns) | Type | Supports |
|---|---|---|---|
| Category_pkey | Category(id) | PK, unique | joins from Product |
| Category_name_key | Category(name) | unique | no duplicate categories; lookup by name |
| Product_pkey | Product(id) | PK, unique | `GET /api/products/:id` |
| Product_categoryId_idx | Product(categoryId) | B-tree | category filter in the catalogue (see Q16) |
| Product_name_idx | Product(name) | B-tree | sorting by name |
| ProductSpecifications_pkey | ProductSpecifications(productId) | PK, unique | 1 : 1 join |
| Size_pkey | Size(id) | PK, unique | joins from ProductSize |
| Size_label_key | Size(label) | unique | no duplicate sizes; lookup by label |
| ProductSize_pkey | ProductSize(productId, sizeId) | PK, unique | sizes of one product; no duplicate links |
| ProductSize_sizeId_idx | ProductSize(sizeId) | B-tree | "which products come in size X" |

## Full SQL

See [`sql/schema.sql`](sql/schema.sql). The key part:

```sql
CREATE TYPE "SizeSystem" AS ENUM ('ALPHA', 'WAIST');

CREATE TABLE "Category" (
    "id"   TEXT NOT NULL PRIMARY KEY,
    "name" TEXT NOT NULL UNIQUE
);

CREATE TABLE "Product" (
    "id"          TEXT NOT NULL PRIMARY KEY,
    "name"        TEXT NOT NULL,
    "description" TEXT NOT NULL,
    "imagePath"   TEXT,
    "categoryId"  TEXT NOT NULL REFERENCES "Category"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
    "createdAt"   TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt"   TIMESTAMP(3) NOT NULL
);

CREATE TABLE "ProductSpecifications" (
    "productId" TEXT NOT NULL PRIMARY KEY REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE,
    "material"  TEXT,
    "fit"       TEXT,
    "care"      TEXT
);

CREATE TABLE "Size" (
    "id"         SERIAL PRIMARY KEY,
    "label"      TEXT NOT NULL UNIQUE,
    "sizeSystem" "SizeSystem" NOT NULL,
    "sortOrder"  INTEGER NOT NULL CHECK ("sortOrder" > 0)
);

CREATE TABLE "ProductSize" (
    "productId"     TEXT NOT NULL REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE,
    "sizeId"        INTEGER NOT NULL REFERENCES "Size"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
    "stockQuantity" INTEGER NOT NULL DEFAULT 0 CHECK ("stockQuantity" >= 0),
    PRIMARY KEY ("productId", "sizeId")
);
```

(The real file names every constraint, as Prisma does, and adds the extra
CHECK rules and indexes listed above.)
