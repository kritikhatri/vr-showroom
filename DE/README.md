# DE Integrated Project: ThreadHaven VR Showroom Database

Data Engineering (DE) submission for the ThreadHaven VR Showroom: a Unity
virtual garment showroom whose product catalogue is stored in **PostgreSQL**
and served to Unity by an **Express + Prisma** API (`backend/`).

## Deliverables

| # | Deliverable (guidelines §3) | Document | Supporting files |
|---|---|---|---|
| 1 | Database selection and justification | [01_Database_Selection.md](01_Database_Selection.md) | none |
| 2 | ER diagram | [02_ER_Diagram.md](02_ER_Diagram.md) | [diagrams/](diagrams/) (PNG, SVG, Graphviz source, Mermaid) |
| 3 | Normalization (1NF, 2NF, 3NF) | [03_Normalization.md](03_Normalization.md) | [normalization migration](../backend/prisma/migrations/20261121180000_normalize_sizes_and_constraints/migration.sql) |
| 4 | Final database schema | [04_Final_Schema.md](04_Final_Schema.md) | [sql/schema.sql](sql/schema.sql), [schema.prisma](../backend/prisma/schema.prisma) |
| 5 | Sample records | [05_Sample_Records.md](05_Sample_Records.md) | [sql/sample_data.sql](sql/sample_data.sql), [seed.ts](../backend/prisma/seed.ts) |
| 6 | 10+ project-specific queries with explanations and outputs | [06_Queries.md](06_Queries.md) (16 queries) | [sql/queries.sql](sql/queries.sql), [outputs/queries_output.txt](outputs/queries_output.txt) |
| 7 | Database–backend integration evidence | [07_Backend_Integration.md](07_Backend_Integration.md) | [integration/demo.sh](integration/demo.sh), `outputs/integration_demo_output.txt` |
| 8 | Architecture and implementation decisions | [08_Architecture_and_Decisions.md](08_Architecture_and_Decisions.md) | none |

Data integrity and performance (guidelines §2 G) are covered in
[08](08_Architecture_and_Decisions.md), with the constraints demonstrated in
Q15 and indexing in Q16 of [06](06_Queries.md).

## Database at a glance

```text
Category 1 ──< Product 1 ──1 ProductSpecifications
                  1
                  │
                  ^
              ProductSize >── 1 Size          (Product M : N Size)
```

* 5 tables, 4 foreign keys, 1 one-to-one, 3 one-to-many, 1 many-to-many (via `ProductSize`)
* Normalized to 3NF; constraints: PK, FK, UNIQUE, NOT NULL, ENUM, 8 CHECKs
* Sample data: 4 categories, 8 products, 10 sizes, 36 product-size stock rows

## Running the DE SQL files directly (PostgreSQL 15+)

```sh
createdb vr_showroom_de
psql -d vr_showroom_de -f DE/sql/schema.sql
psql -d vr_showroom_de -f DE/sql/sample_data.sql
psql -d vr_showroom_de -f DE/sql/queries.sql
```

## Running the application with the database

```sh
cd backend
cp .env.example .env    # then put your PostgreSQL user/password in .env
npm install
npm run db:setup        # migrations + seed
npm run dev             # API on http://127.0.0.1:3000
```

Then press Play in Unity with **Load From Backend** enabled on the
`ProductCatalogue` component, or run `bash DE/integration/demo.sh` for the
create, read, update and delete demo.

## Folder contents

```text
DE/
├── README.md                         ← this file
├── 01_Database_Selection.md
├── 02_ER_Diagram.md
├── 03_Normalization.md
├── 04_Final_Schema.md
├── 05_Sample_Records.md
├── 06_Queries.md
├── 07_Backend_Integration.md
├── 08_Architecture_and_Decisions.md
├── diagrams/    er_diagram.png, er_diagram.svg, er_diagram.dot
├── sql/         schema.sql, sample_data.sql, queries.sql
├── outputs/     queries_output.txt (+ integration_demo_output.txt after running the demo)
└── integration/ demo.sh
```
