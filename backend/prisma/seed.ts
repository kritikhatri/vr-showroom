import "dotenv/config";
import { readFile } from "node:fs/promises";
import { PrismaPg } from "@prisma/adapter-pg";
import { PrismaClient } from "../src/generated/prisma/client";

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

    const specifications = product.specifications ?? {};
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
