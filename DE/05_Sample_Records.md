# 5. Sample Records

The database is populated with the eight garments that the Unity catalogue
already uses (`Assets/Member3/Products/sample-products.json`), so the VR
showroom, the API and these documents all show the same data.

* **For the application:** `cd backend && npm run db:setup` runs the migrations and then
  `prisma/seed.ts`, which reads the JSON file and fills every table.
* **For psql / marking:** [`sql/sample_data.sql`](sql/sample_data.sql) inserts exactly the same records.

Stock levels are demo values (the JSON file has none). They are identical in
`seed.ts` (`SAMPLE_STOCK`) and `sample_data.sql`.

| Table | Rows |
|---|--:|
| Category | 4 |
| Product | 8 |
| ProductSpecifications | 8 |
| Size | 10 |
| ProductSize | 36 |

The tables below were exported from the database straight after loading the
sample data (before any of the queries in [06_Queries.md](06_Queries.md) were run).

## Category

| id | name |
|---|---|
| dresses | Dresses |
| jackets | Jackets |
| shirts | Shirts |
| trousers | Trousers |

## Size

| id | label | sizeSystem | sortOrder |
|---|---|---|---|
| 1 | XS | ALPHA | 2 |
| 2 | S | ALPHA | 3 |
| 3 | M | ALPHA | 4 |
| 4 | L | ALPHA | 5 |
| 5 | XL | ALPHA | 6 |
| 6 | 28 | WAIST | 28 |
| 7 | 30 | WAIST | 30 |
| 8 | 32 | WAIST | 32 |
| 9 | 34 | WAIST | 34 |
| 10 | 36 | WAIST | 36 |

`sortOrder` puts sizes in the order shoppers expect (XS, S, M, L, XL) instead of
alphabetical order. Letter sizes follow the list XXS, XS, S, M, L, XL, XXL, XXXL,
so XS is 2. Waist sizes use the number itself.

## Product

| id | name | categoryId | description |
|---|---|---|---|
| DR-001 | Everyday A-Line Dress | dresses | A comfortable knee-length A-line dress with short sleeves and a simple finish. |
| DR-002 | Formal Woven Midi Dress | dresses | A woven midi dress with a clean silhouette suited to formal occasions. |
| JK-001 | Lightweight Utility Jacket | jackets | A lightweight outer layer with practical front pockets and a zip closure. |
| JK-002 | Quilted Insulated Jacket | jackets | A quilted jacket designed to provide a warm, comfortable layer in cool weather. |
| SH-001 | Classic Cotton Crew Neck T-Shirt | shirts | A breathable everyday crew neck shirt made from soft cotton jersey. |
| SH-002 | Oxford Button-Down Shirt | shirts | A durable woven shirt with a button-down collar for work or casual wear. |
| TR-001 | Straight-Leg Work Trousers | trousers | Hard-wearing work trousers with a straight-leg cut and reinforced seams. |
| TR-002 | Stretch Chino Trousers | trousers | Versatile chinos made with a small amount of stretch for ease of movement. |

`imagePath` is empty for all sample products (the Unity cards currently use
generated placeholders), so it is not shown here.

## ProductSpecifications

| productId | material | fit | care |
|---|---|---|---|
| DR-001 | Cotton poplin | A-line | Machine wash cold; iron warm |
| DR-002 | Polyester-viscose blend | Regular | Gentle machine wash |
| JK-001 | Cotton-polyester twill | Relaxed | Machine wash cold |
| JK-002 | Recycled polyester shell; polyester fill | Regular | Gentle machine wash |
| SH-001 | 100% cotton | Regular | Machine wash cold |
| SH-002 | Cotton Oxford cloth | Regular | Machine wash cold; iron warm |
| TR-001 | Cotton-polyester canvas | Straight leg | Machine wash warm |
| TR-002 | Cotton with elastane | Slim straight | Machine wash cold |

## ProductSize (many-to-many with stock)

Stock per product and size (– = size not offered for that product):

| Product | XS | S | M | L | XL | 28 | 30 | 32 | 34 | 36 | Total |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|--:|
| DR-001 | 4 | 11 | 16 | 10 | 6 | – | – | – | – | – | 47 |
| DR-002 | 0 | 6 | 9 | 7 | 3 | – | – | – | – | – | 25 |
| JK-001 | – | 7 | 14 | 11 | 4 | – | – | – | – | – | 36 |
| JK-002 | – | 5 | 9 | 8 | 3 | – | – | – | – | – | 25 |
| SH-001 | – | 18 | 25 | 22 | 9 | – | – | – | – | – | 74 |
| SH-002 | – | 12 | 20 | 16 | 0 | – | – | – | – | – | 48 |
| TR-001 | – | – | – | – | – | 6 | 15 | 19 | 12 | 2 | 54 |
| TR-002 | – | – | – | – | – | 10 | 21 | 24 | 13 | 5 | 73 |

<details>
<summary>The same data as the 36 raw rows stored in <code>ProductSize</code></summary>

The table stores `sizeId`; the size label is joined in here for readability.

| productId | size | stockQuantity |
|---|---|---|
| DR-001 | XS | 4 |
| DR-001 | S | 11 |
| DR-001 | M | 16 |
| DR-001 | L | 10 |
| DR-001 | XL | 6 |
| DR-002 | XS | 0 |
| DR-002 | S | 6 |
| DR-002 | M | 9 |
| DR-002 | L | 7 |
| DR-002 | XL | 3 |
| JK-001 | S | 7 |
| JK-001 | M | 14 |
| JK-001 | L | 11 |
| JK-001 | XL | 4 |
| JK-002 | S | 5 |
| JK-002 | M | 9 |
| JK-002 | L | 8 |
| JK-002 | XL | 3 |
| SH-001 | S | 18 |
| SH-001 | M | 25 |
| SH-001 | L | 22 |
| SH-001 | XL | 9 |
| SH-002 | S | 12 |
| SH-002 | M | 20 |
| SH-002 | L | 16 |
| SH-002 | XL | 0 |
| TR-001 | 28 | 6 |
| TR-001 | 30 | 15 |
| TR-001 | 32 | 19 |
| TR-001 | 34 | 12 |
| TR-001 | 36 | 2 |
| TR-002 | 28 | 10 |
| TR-002 | 30 | 21 |
| TR-002 | 32 | 24 |
| TR-002 | 34 | 13 |
| TR-002 | 36 | 5 |

</details>

## How to view it yourself

```sh
psql -d vr_showroom -c 'SELECT * FROM "Product";'
# or, with the API stopped or running:
cd backend && npm run db:studio
```
