# 7. Database and Application Integration

## Architecture

```text
┌──────────────────────┐   HTTP + JSON    ┌───────────────────────┐   Prisma ORM    ┌──────────────┐
│ Unity VR client      │ ───────────────► │ Express API (Node.js) │ ──────────────► │ PostgreSQL   │
│ ProductCatalogue.cs  │ ◄─────────────── │ backend/src/server.ts │ ◄────────────── │ vr_showroom  │
│ (UnityWebRequest)    │                  │ + validation          │  SQL over TCP   │ 5 tables     │
└──────────────────────┘                  └───────────────────────┘  (pg adapter)   └──────────────┘
        reads catalogue                      create / read / update / delete
```

1. **Unity → API.** `Assets/Member3/Scripts/ProductCatalogue.cs` sends
   `UnityWebRequest.Get("http://127.0.0.1:3000/api/products")` when **Load From
   Backend** is enabled. If the API cannot be reached, it falls back to the local
   JSON file and logs a warning.
2. **API → database.** `backend/src/prisma.ts` creates one `PrismaClient` using
   the `@prisma/adapter-pg` driver adapter and the `DATABASE_URL` connection
   string from `backend/.env`. Each request handler calls Prisma, which turns the
   call into parameterised SQL and runs it over a pooled `pg` connection.
3. **Database → API → Unity.** Rows from `Product`, `Category`,
   `ProductSpecifications`, `ProductSize` and `Size` are joined and turned back
   into the JSON shape Unity already uses (`toCatalogueProduct`). The
   `availableSizes` text (`"S, M, L, XL"`) is rebuilt from the normalized
   `ProductSize` rows, so no Unity code had to change after normalization.

## API endpoints and the database operations behind them

| Method and path | Purpose | Database operation |
|---|---|---|
| `GET /api/health` | API ↔ DB connectivity check | `SELECT 1` |
| `GET /api/categories` | Category panel (with product counts) | `SELECT` + `COUNT` per category |
| `GET /api/products?category=&search=` | Catalogue used by Unity | `SELECT` with 4-table JOIN, `ILIKE` filter, `ORDER BY` |
| `GET /api/products/:id` | One product (information panel) | `SELECT ... WHERE id = $1` with JOINs |
| `GET /api/reports/stock-by-category` | Stock report for staff | **Raw SQL** via `prisma.$queryRaw`: `LEFT JOIN`, `GROUP BY`, `SUM`, `COUNT FILTER` |
| `POST /api/categories` | Add a category | `INSERT INTO "Category"` |
| `DELETE /api/categories/:id` | Remove an empty category | `DELETE`; blocked by **ON DELETE RESTRICT** if it has products (HTTP 409) |
| `POST /api/products` | Add a product with specs and sizes | `INSERT` into `Product`, `ProductSpecifications`, `ProductSize` (and `Size` if new) in one transaction |
| `PATCH /api/products/:id` | Edit a product | `UPDATE` (+ upsert specs, replace sizes) inside `prisma.$transaction` |
| `PUT /api/products/:id/sizes/:label` | Set stock for one size | Upsert on `ProductSize`: `UPDATE` the row if it exists, otherwise `INSERT` |
| `DELETE /api/products/:id/sizes/:label` | Stop offering a size | `DELETE FROM "ProductSize"` |
| `DELETE /api/products/:id` | Remove a product | `DELETE FROM "Product"`; specs and sizes removed by **ON DELETE CASCADE** |

## Key code

**Connection** (`backend/src/prisma.ts`):

```ts
const adapter = new PrismaPg({ connectionString: process.env.DATABASE_URL });
export const prisma = new PrismaClient({ adapter });
```

**Create: one product across three tables** (`POST /api/products`):

```ts
const product = await prisma.product.create({
  data: {
    id, name, description, imagePath,
    category: { connect: { id: category.id } },              // FK -> Category
    specifications: { create: { material, fit, care } },     // 1 : 1 row
    sizes: { create: sizeCreateInput(sizes) }                // M : N rows in ProductSize
  },
  include: productInclude
});
```

**Update inside a transaction** (`PATCH /api/products/:id`): either every
change is saved or none is.

```ts
const product = await prisma.$transaction(async (tx) => {
  await tx.product.update({ where: { id }, data: { ...changes } });
  if (sizes) {
    await tx.productSize.deleteMany({ where: { productId: id } });
    await tx.product.update({ where: { id }, data: { sizes: { create: sizeCreateInput(sizes) } } });
  }
  return tx.product.findUniqueOrThrow({ where: { id }, include: productInclude });
});
```

**Raw SQL in the application** (`GET /api/reports/stock-by-category`):

```ts
const rows = await prisma.$queryRaw`
  SELECT c."name" AS "category",
         COUNT(DISTINCT p."id")::int                         AS "products",
         COALESCE(SUM(ps."stockQuantity"), 0)::int           AS "totalUnits",
         COUNT(*) FILTER (WHERE ps."stockQuantity" = 0)::int AS "outOfStockSizes"
  FROM "Category" c
  LEFT JOIN "Product" p      ON p."categoryId" = c."id"
  LEFT JOIN "ProductSize" ps ON ps."productId" = p."id"
  GROUP BY c."name"
  ORDER BY "totalUnits" DESC`;
```

## Handling invalid, duplicate and missing data

Data is checked twice: once in the API, for clear messages, and once by the
database, as the final guarantee.

| Problem | API check (HTTP response) | Database guarantee |
|---|---|---|
| Missing name, description or category | 400 with a list of what is missing | `NOT NULL` + `CHECK (btrim(...) <> '')` |
| Bad product code (`shirt4`) | 400 "id must look like SH-003" | `CHECK (id ~ '^[A-Z]{2}-[0-9]{3}$')` |
| Unknown category | 400 "Category … does not exist" | FOREIGN KEY |
| Unknown or repeated size label, negative stock | 400 | UNIQUE on `Size.label`, composite PK, `CHECK (stockQuantity >= 0)` |
| Same product id twice | 409 (Prisma error P2002) | PRIMARY KEY |
| Deleting a category that still has products | 409 (P2003) | `ON DELETE RESTRICT` |
| Updating or deleting something that does not exist | 404 (P2025) | none |

Input is also normalised before saving: text is trimmed, size labels are
upper-cased (`" m "` → `M`), and product codes are upper-cased.

## Evidence

### How to produce it (about 5 minutes)

```sh
cd backend
npm install
npm run db:setup        # applies both migrations and seeds the sample data
npm run dev             # leave running; API on http://127.0.0.1:3000

# in a second terminal, from the repository root:
bash DE/integration/demo.sh | tee DE/outputs/integration_demo_output.txt
```

The script ([`integration/demo.sh`](integration/demo.sh)) calls every write
endpoint and, after each one, queries PostgreSQL directly with `psql` to show
the change really reached the database:

1. health check, 2. list categories,
3. **create** product `SH-004` → 4. `psql` shows the new rows in all three tables,
5. read it back the way Unity does,
6. **update** care text → `psql` confirms, 7. **update** stock → `psql` confirms,
8. filtered catalogue, 9. stock report (raw SQL),
10. invalid data → 400, 11. duplicate id → 409, 12. delete a category in use → 409,
13. **delete** the product → `psql` shows 0 rows in all three tables, 14. → 404.

It ends with the database back in its starting state, so it can be re-run for
the live demo.

### Evidence checklist for the submission

- [ ] `DE/outputs/integration_demo_output.txt`: output of the script above
- [ ] `DE/screenshots/unity_catalogue_from_api.png`: the Unity catalogue loaded with **Load From Backend** enabled (API terminal visible, showing the request)
- [ ] `DE/screenshots/prisma_studio.png`: `npm run db:studio` showing the `ProductSize` table
- [ ] `DE/screenshots/unity_after_update.png`: change a product with `PATCH`, press play again, and the new text appears in the VR information panel
