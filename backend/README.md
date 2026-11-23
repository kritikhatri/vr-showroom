# PostgreSQL and Prisma API

The Unity product catalogue is connected to a local REST API. The API uses Prisma ORM to read product data from PostgreSQL and returns the same JSON shape that Unity already understands.

```text
Unity ProductCatalogue --HTTP--> Express API --Prisma--> PostgreSQL
```

The database schema separates `Category`, `Product`, `ProductSpecifications`, `Size`, and `ProductSize`. Products refer to a category by ID, their optional specifications are stored in a one-to-one related table, and the sizes each product is offered in (with stock per size) are stored in the `ProductSize` many-to-many table. The API assembles those related records into the catalogue shape expected by Unity, including the `availableSizes` text. The full database documentation for the DE submission is in [`../DE`](../DE/README.md).

## Requirements

- Node.js 26 or newer
- PostgreSQL 15 or newer, running locally

## One-time setup

1. Create a PostgreSQL database named `vr_showroom` and a login role for the backend. In `psql`, for example:

   ```sql
   CREATE ROLE vr_showroom_app LOGIN CREATEDB PASSWORD 'choose-a-local-password';
   CREATE DATABASE vr_showroom OWNER vr_showroom_app;
   ```

2. From this folder, install packages and copy `.env.example` to `.env`.
3. Edit `.env` with your PostgreSQL username and password. Do not commit `.env`.
4. Run the migration and seed commands:

   ```sh
   npm install
   npm run db:setup
   ```

   The migrations create the tables (the second one moves the old comma-separated sizes into the `Size`/`ProductSize` tables and adds CHECK constraints); the seed script imports the eight sample records from `Assets/Member3/Products/sample-products.json` and sets demo stock levels.

   If you already ran the first migration before, just run `npm run db:setup` again: your existing products keep their sizes.

## Start the API

Run `npm run dev` from this folder. The API listens at `http://127.0.0.1:3000` by default.

Available read endpoints:

- `GET /api/health` — confirms the API can query PostgreSQL.
- `GET /api/categories` — lists product categories.
- `GET /api/products` — returns the full catalogue.
- `GET /api/products?category=Shirts` — filters by category.
- `GET /api/products?search=cotton` — searches product names.
- `GET /api/products/:id` — returns one product or a 404.
- `GET /api/reports/stock-by-category` — products, total units, and sold-out sizes per category.

Write endpoints (catalogue management; JSON body):

- `POST /api/categories` — `{ "name": "Hats" }`
- `DELETE /api/categories/:id` — refused with 409 while the category has products.
- `POST /api/products` — `{ "id": "SH-004", "name": "...", "category": "Shirts", "description": "...", "specifications": { "material": "...", "fit": "...", "care": "..." }, "sizes": [{ "label": "M", "stockQuantity": 6 }] }`
- `PATCH /api/products/:id` — any of the fields above; `sizes`, if given, replaces the product's sizes.
- `PUT /api/products/:id/sizes/:label` — `{ "stockQuantity": 0 }` sets the stock for one size.
- `DELETE /api/products/:id/sizes/:label` — stops offering a size.
- `DELETE /api/products/:id` — deletes the product, its specifications, and its sizes.

Invalid data returns 400 with a list of problems, duplicates and records still in use return 409, and missing records return 404.

You can inspect the database with `npm run db:studio` while the API is stopped or running.

## Connect Unity

On the active `ProductCatalogue` component, enable **Load From Backend** and set **Product Api Url**. The default `http://127.0.0.1:3000/api/products` works when Unity runs in the Editor on the same computer as the API. For a standalone VR headset, use the development computer's LAN IP address and make sure both devices can reach it.

The catalogue first loads the assigned local JSON file. If the API responds successfully, its database records replace the fallback list. If the API is unavailable, the JSON records remain visible and Unity logs a warning.

## Team setup

Commit the Prisma schema, migrations, and seed script. Each teammate should create their own local PostgreSQL role/database and `.env` file. Keep credentials out of Git. The SQLite/database file is not stored in the Unity project.
