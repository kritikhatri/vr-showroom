# 01 – Database Selection

## Selected Database: PostgreSQL ✅

**Version used:** PostgreSQL 18 (via Homebrew on macOS)  
**ORM / Query layer:** Prisma ORM with TypeScript  
**Backend framework:** Express.js (Node.js)

---

## Why We Chose PostgreSQL

| Requirement | How PostgreSQL meets it |
|---|---|
| Structured product data (fixed fields per item) | Relational tables with strict schema |
| Category ↔ Product relationship | Foreign key with `onDelete: Restrict` |
| Product specifications as sub-record | One-to-one `ProductSpecifications` table |
| Search by name | `@@index([name])` for fast LIKE queries |
| Filter by category | `@@index([categoryId])` for fast joins |
| Data integrity | Transactions, constraints, cascading deletes |
| Scalability | Handles millions of rows without schema changes |

---

## Database Schema

```
Category
  id          TEXT  PRIMARY KEY   (e.g. "shirts")
  name        TEXT  UNIQUE        (e.g. "Shirts")

Product
  id          TEXT  PRIMARY KEY   (e.g. "SH-001")
  name        TEXT
  description TEXT
  imagePath   TEXT  (nullable)
  categoryId  TEXT  → FK → Category.id
  createdAt   TIMESTAMP
  updatedAt   TIMESTAMP

ProductSpecifications   (one-to-one with Product)
  productId      TEXT  PRIMARY KEY → FK → Product.id
  material       TEXT
  fit            TEXT
  availableSizes TEXT
  care           TEXT
```

---

## PostgreSQL vs MongoDB — Comparison

| Feature | **PostgreSQL** ✅ (our choice) | MongoDB |
|---|---|---|
| **Data model** | Relational (tables + foreign keys) | Document-based (JSON collections) |
| **Schema** | Strict, enforced at DB level | Flexible / schema-less |
| **Relationships** | Native JOIN, FK constraints | Manual references or embedded docs |
| **Data integrity** | Strong (transactions, constraints) | Weaker by default |
| **Query language** | Standard SQL | MongoDB Query Language (MQL) |
| **Search** | Full SQL with indexes | `$regex`, `$text` operators |
| **Category filtering** | `WHERE categoryId = 'shirts'` (indexed, fast) | `db.products.find({category:"Shirts"})` |
| **Joins** | `INNER JOIN`, `LEFT JOIN` | `$lookup` (slower, manual) |
| **Best for** | Structured, relational data ✅ | Unstructured, nested, frequently changing data |
| **Our use case fit** | ✅ Perfect — fixed product fields, categories | ❌ Overkill — adds complexity for no benefit |

### Why NOT MongoDB for this project?

1. Our product data has **fixed, well-defined fields** — name, category, specs. A rigid schema is an advantage, not a limitation.
2. We need **referential integrity** — a product must belong to a valid category. PostgreSQL enforces this automatically via foreign keys. MongoDB does not.
3. **Category filtering and product search** are standard SQL operations — `WHERE`, `JOIN`, `LIKE` — that PostgreSQL executes efficiently with indexes.
4. The team is using **Prisma ORM**, which works best with relational databases.

---

## Database Connection

```
Host:     localhost
Port:     5432
Database: vr_showroom
Schema:   public
ORM:      Prisma (TypeScript)
```

`.env` configuration:
```
DATABASE_URL="postgresql://dishimukati@localhost:5432/vr_showroom?schema=public"
PORT=3000
```

---

## Sample Data in the Database

**Categories (4 rows):**

| id | name |
|---|---|
| shirts | Shirts |
| jackets | Jackets |
| trousers | Trousers |
| dresses | Dresses |

**Products (8 rows):**

| id | name | category |
|---|---|---|
| SH-001 | Classic Cotton Crew Neck T-Shirt | Shirts |
| SH-002 | Oxford Button-Down Shirt | Shirts |
| JK-001 | Lightweight Utility Jacket | Jackets |
| JK-002 | Quilted Insulated Jacket | Jackets |
| TR-001 | Straight-Leg Work Trousers | Trousers |
| TR-002 | Stretch Chino Trousers | Trousers |
| DR-001 | Everyday A-Line Dress | Dresses |
| DR-002 | Formal Woven Midi Dress | Dresses |

---

## SQL Queries for Demonstration

```sql
-- Show first 5 products
SELECT * FROM "Product" LIMIT 5;

-- Show all categories
SELECT * FROM "Category";

-- Filter products by category
SELECT p.id, p.name, c.name AS category
FROM "Product" p
JOIN "Category" c ON p."categoryId" = c.id
WHERE c.name = 'Shirts';

-- Show products with specifications
SELECT p.name, s.material, s.fit, s."availableSizes"
FROM "Product" p
JOIN "ProductSpecifications" s ON p.id = s."productId";
```
