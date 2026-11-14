# PostgreSQL and Prisma API

The Unity product catalogue is connected to a local REST API. The API uses Prisma ORM to read product data from PostgreSQL and returns the same JSON shape that Unity already understands.

```text
Unity ProductCatalogue --HTTP--> Express API --Prisma--> PostgreSQL
```

The database schema separates `Category`, `Product`, and `ProductSpecifications`. Products refer to a category by ID, and their optional specifications are stored in a one-to-one related table. The API assembles those related records into the catalogue shape expected by Unity.

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

   The migration creates the tables; the seed script imports the eight sample records from `Assets/Member3/Products/sample-products.json`.

## Start the API

Run `npm run dev` from this folder. The API listens at `http://127.0.0.1:3000` by default.

Available read endpoints:

- `GET /api/health` — confirms the API can query PostgreSQL.
- `GET /api/categories` — lists product categories.
- `GET /api/products` — returns the full catalogue.
- `GET /api/products?category=Shirts` — filters by category.
- `GET /api/products?search=cotton` — searches product names.
- `GET /api/products/:id` — returns one product or a 404.

You can inspect the database with `npm run db:studio` while the API is stopped or running.

## Connect Unity

On the active `ProductCatalogue` component, enable **Load From Backend** and set **Product Api Url**. The default `http://127.0.0.1:3000/api/products` works when Unity runs in the Editor on the same computer as the API. For a standalone VR headset, use the development computer's LAN IP address and make sure both devices can reach it.

The catalogue first loads the assigned local JSON file. If the API responds successfully, its database records replace the fallback list. If the API is unavailable, the JSON records remain visible and Unity logs a warning.

## Team setup

Commit the Prisma schema, migrations, and seed script. Each teammate should create their own local PostgreSQL role/database and `.env` file. Keep credentials out of Git. The SQLite/database file is not stored in the Unity project.
