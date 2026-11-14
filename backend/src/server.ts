import "dotenv/config";
import cors from "cors";
import express, { type NextFunction, type Request, type Response } from "express";
import { prisma } from "./prisma";

const app = express();
const port = Number(process.env.PORT ?? 3000);

app.use(cors());
app.use(express.json());

function toCatalogueProduct(product: {
  id: string;
  name: string;
  description: string;
  imagePath: string | null;
  category: { name: string };
  specifications: {
    material: string | null;
    fit: string | null;
    availableSizes: string | null;
    care: string | null;
  } | null;
}) {
  return {
    id: product.id,
    name: product.name,
    category: product.category.name,
    description: product.description,
    specifications: product.specifications ?? {},
    image: product.imagePath ?? ""
  };
}

const productInclude = {
  category: { select: { name: true } },
  specifications: true
} as const;

app.get("/api/health", async (_request, response, next) => {
  try {
    await prisma.$queryRaw`SELECT 1`;
    response.json({ status: "ok", database: "connected" });
  } catch (error) {
    next(error);
  }
});

app.get("/api/categories", async (_request, response, next) => {
  try {
    const categories = await prisma.category.findMany({
      orderBy: { name: "asc" },
      select: { id: true, name: true }
    });
    response.json(categories);
  } catch (error) {
    next(error);
  }
});

app.get("/api/products", async (request, response, next) => {
  try {
    const category = typeof request.query.category === "string" ? request.query.category.trim() : "";
    const search = typeof request.query.search === "string" ? request.query.search.trim() : "";

    const products = await prisma.product.findMany({
      where: {
        ...(category && category.toLowerCase() !== "all"
          ? { category: { name: { equals: category, mode: "insensitive" as const } } }
          : {}),
        ...(search ? { name: { contains: search, mode: "insensitive" as const } } : {})
      },
      include: productInclude,
      orderBy: [{ category: { name: "asc" } }, { name: "asc" }]
    });

    response.json({
      catalogueName: "ThreadHaven Virtual Showroom",
      industry: "Garments",
      products: products.map(toCatalogueProduct)
    });
  } catch (error) {
    next(error);
  }
});

app.get("/api/products/:id", async (request, response, next) => {
  try {
    const product = await prisma.product.findUnique({
      where: { id: request.params.id },
      include: productInclude
    });

    if (!product) {
      response.status(404).json({ error: "Product not found." });
      return;
    }

    response.json(toCatalogueProduct(product));
  } catch (error) {
    next(error);
  }
});

app.use((error: unknown, _request: Request, response: Response, _next: NextFunction) => {
  console.error("API request failed:", error);
  response.status(500).json({ error: "The showroom API could not complete the request." });
});

const server = app.listen(port, () => {
  console.log(`VR Showroom API listening at http://127.0.0.1:${port}`);
});

async function shutdown() {
  server.close();
  await prisma.$disconnect();
}

process.on("SIGINT", shutdown);
process.on("SIGTERM", shutdown);
