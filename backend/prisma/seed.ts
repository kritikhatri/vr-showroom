import "dotenv/config";
import { readFile } from "node:fs/promises";
import { PrismaPg } from "@prisma/adapter-pg";
import { PrismaClient } from "../src/generated/prisma/client";
import { parseSizeList, sizeSystemFor, sortOrderFor } from "../src/sizes";

interface SeedProduct {
  id: string;
  name: string;
  category: string;
  description: string;
  specifications?: {
    material?: string;
    fit?: string;
    availableSizes?: string;
    care?: string;
  };
  image?: string;
}

interface SeedCatalogue {
  catalogueName?: string;
  industry?: string;
  products: SeedProduct[];
}

// Sample stock levels per product and size. The Unity JSON file has no stock
// information, so these demo values are kept here. Sizes not listed get 0.
const SAMPLE_STOCK: Record<string, Record<string, number>> = {
  "SH-001": { S: 18, M: 25, L: 22, XL: 9 },
  "SH-002": { S: 12, M: 20, L: 16, XL: 0 },
  "JK-001": { S: 7, M: 14, L: 11, XL: 4 },
  "JK-002": { S: 5, M: 9, L: 8, XL: 3 },
  "TR-001": { "28": 6, "30": 15, "32": 19, "34": 12, "36": 2 },
  "TR-002": { "28": 10, "30": 21, "32": 24, "34": 13, "36": 5 },
  "DR-001": { XS: 4, S: 11, M: 16, L: 10, XL: 6 },
  "DR-002": { XS: 0, S: 6, M: 9, L: 7, XL: 3 }
};

const connectionString = process.env.DATABASE_URL;
if (!connectionString) {
  throw new Error("DATABASE_URL is missing. Copy .env.example to .env and set your PostgreSQL connection string.");
}

const adapter = new PrismaPg({ connectionString });
const prisma = new PrismaClient({ adapter });
const sampleProductsUrl = new URL("../../Assets/Member3/Products/sample-products.json", import.meta.url);

function categoryId(name: string) {
  return name.trim().toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

async function main() {
  const catalogue = JSON.parse(await readFile(sampleProductsUrl, "utf8")) as SeedCatalogue;

  for (const product of catalogue.products) {
    const id = categoryId(product.category);
    await prisma.category.upsert({
      where: { name: product.category },
      update: {},
      create: { id, name: product.category }
    });

    const productFields = {
      name: product.name,
      description: product.description,
      imagePath: product.image || null,
      category: { connect: { name: product.category } }
    };

    const { availableSizes, ...specifications } = product.specifications ?? {};
    await prisma.product.upsert({
      where: { id: product.id },
      update: {
        ...productFields,
        specifications: {
          upsert: {
            create: specifications,
            update: specifications
          }
        }
      },
      create: {
        id: product.id,
        ...productFields,
        specifications: { create: specifications }
      }
    });

    for (const label of parseSizeList(availableSizes)) {
      const size = await prisma.size.upsert({
        where: { label },
        update: {},
        create: { label, sizeSystem: sizeSystemFor(label), sortOrder: sortOrderFor(label) }
      });

      const stockQuantity = SAMPLE_STOCK[product.id]?.[label] ?? 0;
      await prisma.productSize.upsert({
        where: { productId_sizeId: { productId: product.id, sizeId: size.id } },
        update: { stockQuantity },
        create: { productId: product.id, sizeId: size.id, stockQuantity }
      });
    }
  }

  console.log(`Seeded ${catalogue.products.length} products into PostgreSQL.`);
}

main()
  .catch((error: unknown) => {
    console.error("Could not seed showroom products:", error);
    process.exitCode = 1;
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
