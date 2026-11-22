# 01 – Database Selection

## Selected Database: PostgreSQL ✅

**Version used:** PostgreSQL 18 (via Homebrew on macOS)  
**ORM / Query layer:** Prisma ORM with TypeScript  
**Backend framework:** Express.js (Node.js)  
**Database Name:** `vr_showroom`

---

## Why We Chose PostgreSQL

| Requirement | How PostgreSQL meets it |
|---|---|
| Structured product data (fixed fields per item) | Relational tables with strict schema |
| Category ↔ Product relationship | Foreign key with `onDelete: Restrict` |
| Product specifications as sub-record | One-to-one `ProductSpecifications` table |
| Product ↔ Sizes & Inventory (Many-to-Many) | Normalized join table `ProductSize` with stock tracking |
| Search by name | `@@index([name])` for fast query lookup |
| Filter by category | `@@index([categoryId])` for fast indexed joins |
| Data integrity | Strict ACID transactions, constraints, cascading deletes |
| Scalability & Reliability | Industry-standard relational DB for e-commerce & VR catalogues |

---

## Database Schema (Relational Architecture)

```
Category
  id          TEXT  PRIMARY KEY   (e.g. "shirts", "jackets")
  name        TEXT  UNIQUE        (e.g. "Shirts", "Jackets")

Product
  id          TEXT  PRIMARY KEY   (e.g. "SH-001")
  name        TEXT
  description TEXT
  imagePath   TEXT  (nullable)
  categoryId  TEXT  → FK → Category.id
  createdAt   TIMESTAMP
  updatedAt   TIMESTAMP

ProductSpecifications   (1-to-1 with Product)
  productId      TEXT  PRIMARY KEY → FK → Product.id
  material       TEXT
  fit            TEXT
  availableSizes TEXT
  care           TEXT

Size
  id          TEXT  PRIMARY KEY   (e.g. "s", "m", "32")
  label       TEXT  UNIQUE        (e.g. "S", "M", "32")

ProductSize   (Many-to-Many Join Table between Product and Size)
  id            TEXT  PRIMARY KEY
  productId     TEXT  → FK → Product.id (ON DELETE CASCADE)
  sizeId        TEXT  → FK → Size.id (ON DELETE RESTRICT)
  stockQuantity INTEGER DEFAULT 0
```

---

## PostgreSQL vs MongoDB — Comparison

| Feature | **PostgreSQL** ✅ (Our Choice) | MongoDB |
|---|---|---|
| **Data model** | Relational (tables, primary/foreign keys) | Document-based (JSON/BSON collections) |
| **Schema** | Strict, enforced at DB level | Flexible / dynamic / schema-less |
| **Relationships** | Native `JOIN`, multi-table relationships, FK constraints | Manual referencing or nested embedded arrays |
| **Data integrity** | Strong ACID compliance & referential integrity | Eventual consistency by default |
| **Query language** | Standard SQL (powerful filtering, aggregate joins) | MongoDB Query Language (MQL) |
| **Search & Filtering** | Indexed queries (`B-Tree`, `GIN`, `LIKE`, `JOIN`) | `$regex`, `$text`, `$lookup` (heavier overhead) |
| **Category & Inventory** | Normalized join table (`ProductSize`) tracks exact stock | Array updates inside documents (can cause race conditions) |
| **Best for** | Structured catalogues, inventory management, e-commerce | Unstructured logs, real-time analytics, social feeds |
| **Our use case fit** | ✅ **Perfect** — garments have fixed attributes, sizes, and stock | ❌ Overkill and lacks strict foreign key enforcement |

### Why NOT MongoDB for this project?
1. **Garment & Industrial products have consistent attributes**: names, categories, specifications, and SKU sizes. A structured relational model is much cleaner and avoids duplicate data.
2. **Referential Integrity**: A product cannot exist with an invalid category or non-existent size. PostgreSQL blocks orphaned records automatically.
3. **Multi-Table Joins**: The VR showroom requires querying products alongside category details and live stock quantities in a single fast SQL query.

---

## Terminal Demonstration Queries

### 1. View All Tables
```sql
\dt
```

### 2. View Table Definitions
```sql
\d "Product"
\d "ProductSize"
```

### 3. Basic Inspection Queries
```sql
SELECT * FROM "Category";
SELECT * FROM "Product" LIMIT 5;
```

### 4. Advanced 4-Table JOIN Query (Product + Category + Size + Stock)
```sql
SELECT
    p."name",
    c."name" AS category,
    s."label" AS size,
    ps."stockQuantity"
FROM "Product" p
JOIN "Category" c
    ON p."categoryId" = c."id"
JOIN "ProductSize" ps
    ON p."id" = ps."productId"
JOIN "Size" s
    ON s."id" = ps."sizeId"
ORDER BY p."name";
```

> **Presentation Explanation:**  
> *"This query combines four tables to display product names, their categories, available sizes and stock quantities."*
