# 3. Normalization (UNF → 1NF → 2NF → 3NF)

## Starting point: the unnormalized catalogue (UNF)

The showroom's product data started life as one flat JSON file that Unity
reads (`Assets/Member3/Products/sample-products.json`). The first database
version (migration `20261114182147_init`) still stored sizes the same way, as
one comma-separated text value. If staff also kept stock per size, a single
"catalogue" record would look like this:

| productId | productName | description | categoryName | material | fit | care | availableSizes | stock |
|---|---|---|---|---|---|---|---|---|
| SH-001 | Classic Cotton Crew Neck T-Shirt | A breathable… | Shirts | 100% cotton | Regular | Machine wash cold | **S, M, L, XL** | **18, 25, 22, 9** |
| SH-002 | Oxford Button-Down Shirt | A durable woven… | Shirts | Cotton Oxford cloth | Regular | Machine wash cold; iron warm | **S, M, L, XL** | **12, 20, 16, 0** |
| TR-001 | Straight-Leg Work Trousers | Hard-wearing… | Trousers | Cotton-polyester canvas | Straight leg | Machine wash warm | **28, 30, 32, 34, 36** | **6, 15, 19, 12, 2** |

`availableSizes` and `stock` are **repeating groups**: several values packed
into one field.

### Problems with this design

* **Querying is hard.** "Which products come in XL?" or "total stock of size M" need string parsing (`LIKE '%XL%'` would wrongly match `XXL`).
* **Inconsistent values.** Nothing stops one row from saying `"s, m"` and another `"S,M"`, and a typo such as `"XLL"` cannot be rejected.
* **Update anomaly.** Changing the stock of one size means rewriting a whole text field, and the two lists can get out of step (four sizes but three stock numbers).
* **Insert anomaly.** A new size (e.g. XXL) cannot be recorded until some product uses it, and a new category cannot exist without a product.
* **Delete anomaly.** Deleting the only product in a category also deletes the only record that the category existed.

## Functional dependencies

From the business rules of the showroom:

| # | Functional dependency | Meaning |
|---|---|---|
| FD1 | `productId → productName, description, imagePath, categoryName` | A product code identifies one product. |
| FD2 | `productId → material, fit, care` | Each product has one set of specifications. |
| FD3 | `categoryName → categoryId` and `categoryId → categoryName` | A category's display name and slug identify each other (both are candidate keys). |
| FD4 | `sizeLabel → sizeSystem, sortOrder` | "M" is always an ALPHA size shown 4th; "32" is always a WAIST size. |
| FD5 | `(productId, sizeLabel) → stockQuantity` | Stock belongs to a product **in a given size**. |

## First Normal Form (1NF)

**Rule:** every field holds a single atomic value, there are no repeating
groups, and each row is identified by a key.

**Transformation:** split the size lists so there is **one row per product per
size**. The key becomes the pair (`productId`, `sizeLabel`).

**R1 (1NF)**: (<u>productId</u>, <u>sizeLabel</u>, productName, description, categoryId, categoryName, material, fit, care, sizeSystem, sortOrder, stockQuantity)

| <u>productId</u> | <u>sizeLabel</u> | productName | categoryName | material | … | sizeSystem | sortOrder | stockQuantity |
|---|---|---|---|---|---|---|---|---|
| SH-001 | S | Classic Cotton Crew Neck T-Shirt | Shirts | 100% cotton | … | ALPHA | 3 | 18 |
| SH-001 | M | Classic Cotton Crew Neck T-Shirt | Shirts | 100% cotton | … | ALPHA | 4 | 25 |
| SH-001 | L | Classic Cotton Crew Neck T-Shirt | Shirts | 100% cotton | … | ALPHA | 5 | 22 |
| SH-002 | M | Oxford Button-Down Shirt | Shirts | Cotton Oxford cloth | … | ALPHA | 4 | 20 |

R1 is in 1NF: every value is atomic, so "which products come in XL?" is now a
simple `WHERE sizeLabel = 'XL'`.

**Remaining redundancy:** the product name, description, category and
specifications are repeated for **every size** (4–5 times per product), and
`sizeSystem`/`sortOrder` are repeated for every product using that size.
Changing one product's description means updating 4–5 rows; missing one leaves
the data contradicting itself.

## Second Normal Form (2NF)

**Rule:** 1NF, and every non-key attribute depends on the **whole** primary
key, not just part of it. Partial dependencies are only possible here because
the key is composite.

**Partial dependencies in R1:**

* `productId → productName, description, imagePath, categoryId, categoryName, material, fit, care` (FD1, FD2). These depend on only *part* of the key.
* `sizeLabel → sizeSystem, sortOrder` (FD4). This also depends on only *part* of the key.
* Only `stockQuantity` depends on the full key (FD5).

**Transformation:** move each group of partially dependent attributes into its
own table, keyed by the part of the key it depends on:

* **Product2** (<u>productId</u>, productName, description, imagePath, categoryId, categoryName, material, fit, care)
* **Size** (<u>sizeLabel</u>, sizeSystem, sortOrder)
* **ProductSize** (<u>productId</u>, <u>sizeLabel</u>, stockQuantity)

All three are in 2NF. Each product's details are stored once, each size's
details are stored once, and `ProductSize` holds only the fact that depends
on both.

## Third Normal Form (3NF)

**Rule:** 2NF, and no non-key attribute depends on another non-key attribute
(no **transitive dependencies**).

**Transitive dependency in Product2:**

`productId → categoryId → categoryName` (FD1 + FD3). The category name is
determined by the category, not directly by the product. With the name stored
on every product:

* renaming "Trousers" to "Pants" would mean updating every trouser row (update anomaly);
* a new category cannot be created before it has a product (insert anomaly);
* deleting the last dress would lose the "Dresses" category (delete anomaly).

**Transformation:** move the category into its own table and keep only the
foreign key in Product:

* **Category** (<u>id</u>, name)
* **Product** (<u>id</u>, name, description, imagePath, *categoryId* → Category, createdAt, updatedAt)

The other tables (`Size`, `ProductSize`) have no transitive dependencies:
`sizeSystem` and `sortOrder` both depend directly on the size, and
`stockQuantity` directly on (product, size). They are already in 3NF.

## Final design decisions on top of 3NF

These are not required by 3NF, but they were made deliberately:

1. **`ProductSpecifications` split out as a 1 : 1 table.** `material`, `fit` and `care` depend only on `productId`, so keeping them in `Product` would also be 3NF. They are separate because they are an optional group used only by the product information panel, and because the original project already had this table.
2. **Surrogate key `Size.id` (SERIAL).** The natural key `label` stays UNIQUE, so FD4 is still enforced. The numeric id keeps the junction table compact and lets a label be corrected later (e.g. "XXL" → "2XL") without touching `ProductSize` rows.
3. **Slug key for `Category`** (`shirts`). It is readable in API calls, and `name` remains a UNIQUE candidate key.

## Final normalized schema (3NF)

| Table | Primary key | Foreign keys | Non-key attributes |
|---|---|---|---|
| Category | id | none | name (UNIQUE) |
| Product | id | categoryId → Category | name, description, imagePath, createdAt, updatedAt |
| ProductSpecifications | productId | productId → Product | material, fit, care |
| Size | id | none | label (UNIQUE), sizeSystem, sortOrder |
| ProductSize | (productId, sizeId) | productId → Product, sizeId → Size | stockQuantity |

Every non-key attribute depends on **the key, the whole key, and nothing but
the key**. The full SQL is in [04_Final_Schema.md](04_Final_Schema.md).

## How the change was applied to the real database

Migration
[`20261121180000_normalize_sizes_and_constraints`](../backend/prisma/migrations/20261121180000_normalize_sizes_and_constraints/migration.sql)
performs the 1NF step on existing data, so no product loses its sizes:

1. creates `Size` and `ProductSize`;
2. splits every old `availableSizes` value (`regexp_split_to_table(..., ',')`), trims and upper-cases each label, and inserts each distinct label once into `Size`;
3. inserts one `ProductSize` row per (product, size) pair;
4. drops the old `availableSizes` column and adds the CHECK constraints.

This was tested on a copy of the original database: all 8 products kept the
same sizes in the same order. The API rebuilds the `"S, M, L, XL"` text for
Unity from the normalized rows, so no Unity script had to change.
