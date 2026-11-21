import "dotenv/config";
import cors from "cors";
import express, { type NextFunction, type Request, type Response } from "express";
import { prisma } from "./prisma";
import { isValidSizeLabel, normalizeSizeLabel, sizeSystemFor, sortOrderFor } from "./sizes";

const app = express();
const port = Number(process.env.PORT ?? 3000);

app.use(cors());
app.use(express.json());

// ---------------------------------------------------------------------------
// Shapes and helpers
// ---------------------------------------------------------------------------

const productInclude = {
  category: { select: { name: true } },
  specifications: true,
  sizes: {
    include: { size: true },
    orderBy: { size: { sortOrder: "asc" as const } }
  }
} as const;

interface ProductWithRelations {
  id: string;
  name: string;
  description: string;
  imagePath: string | null;
  category: { name: string };
  specifications: {
    material: string | null;
    fit: string | null;
    care: string | null;
  } | null;
  sizes: { stockQuantity: number; size: { label: string } }[];
}

// Builds the JSON shape Unity's ProductCatalogue already understands.
// availableSizes is rebuilt from the normalized ProductSize rows, so the
// Unity scripts did not need to change when sizes moved to their own table.
function toCatalogueProduct(product: ProductWithRelations) {
  const specifications: Record<string, string> = {};
  if (product.specifications?.material) specifications.material = product.specifications.material;
  if (product.specifications?.fit) specifications.fit = product.specifications.fit;
  if (product.sizes.length > 0) {
    specifications.availableSizes = product.sizes.map((entry) => entry.size.label).join(", ");
  }
  if (product.specifications?.care) specifications.care = product.specifications.care;

  return {
    id: product.id,
    name: product.name,
    category: product.category.name,
    description: product.description,
    specifications,
    image: product.imagePath ?? "",
    sizeStock: product.sizes.map((entry) => ({
      size: entry.size.label,
      stockQuantity: entry.stockQuantity,
      inStock: entry.stockQuantity > 0
    }))
  };
}

function slugify(name: string) {
  return name.trim().toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

class ValidationError extends Error {
  constructor(public readonly details: string[]) {
    super("Validation failed");
  }
}

function optionalText(value: unknown, field: string, errors: string[], maxLength = 500) {
  if (value === undefined || value === null) return undefined;
  if (typeof value !== "string") {
    errors.push(`${field} must be text.`);
    return undefined;
  }
  const trimmed = value.trim();
  if (trimmed.length > maxLength) errors.push(`${field} must be at most ${maxLength} characters.`);
  return trimmed.length > 0 ? trimmed : null;
}

function requiredText(value: unknown, field: string, errors: string[], maxLength = 500) {
  const text = optionalText(value, field, errors, maxLength);
  if (!text) errors.push(`${field} is required.`);
  return text ?? "";
}

interface SizeInput {
  label: string;
  stockQuantity: number;
}

function parseSizes(value: unknown, errors: string[]): SizeInput[] | undefined {
  if (value === undefined) return undefined;
  if (!Array.isArray(value)) {
    errors.push("sizes must be a list such as [{ \"label\": \"M\", \"stockQuantity\": 10 }].");
    return undefined;
  }

  const seen = new Set<string>();
  const sizes: SizeInput[] = [];
  for (const entry of value) {
    const rawLabel = typeof entry === "string" ? entry : entry?.label;
    const rawStock = typeof entry === "string" ? 0 : entry?.stockQuantity ?? 0;
    if (typeof rawLabel !== "string" || rawLabel.trim() === "") {
      errors.push("Every size needs a label.");
      continue;
    }
    const label = normalizeSizeLabel(rawLabel);
    if (!isValidSizeLabel(label)) errors.push(`"${label}" is not a supported size (use XXS-XXXL or a 2-digit waist size).`);
    if (seen.has(label)) errors.push(`Size "${label}" is listed more than once.`);
    if (!Number.isInteger(rawStock) || rawStock < 0) errors.push(`Stock for size "${label}" must be a whole number of 0 or more.`);
    seen.add(label);
    sizes.push({ label, stockQuantity: rawStock });
  }
  return sizes;
}

function sizeCreateInput(sizes: SizeInput[]) {
  return sizes.map((size) => ({
    stockQuantity: size.stockQuantity,
    size: {
      connectOrCreate: {
        where: { label: size.label },
        create: { label: size.label, sizeSystem: sizeSystemFor(size.label), sortOrder: sortOrderFor(size.label) }
      }
    }
  }));
}

function prismaErrorCode(error: unknown) {
  return typeof error === "object" && error !== null && "code" in error
    ? String((error as { code: unknown }).code)
    : undefined;
}

// ---------------------------------------------------------------------------
// Read endpoints (used by Unity)
// ---------------------------------------------------------------------------

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
      select: { id: true, name: true, _count: { select: { products: true } } }
    });
    response.json(categories.map((category) => ({
      id: category.id,
      name: category.name,
      productCount: category._count.products
    })));
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

// Stock report: a raw SQL query with JOIN, GROUP BY and aggregates.
app.get("/api/reports/stock-by-category", async (_request, response, next) => {
  try {
    const rows = await prisma.$queryRaw<
      { category: string; products: number; totalUnits: number; outOfStockSizes: number }[]
    >`
      SELECT c."name"                                              AS "category",
             COUNT(DISTINCT p."id")::int                           AS "products",
             COALESCE(SUM(ps."stockQuantity"), 0)::int             AS "totalUnits",
             COUNT(*) FILTER (WHERE ps."stockQuantity" = 0)::int   AS "outOfStockSizes"
      FROM "Category" c
      LEFT JOIN "Product" p      ON p."categoryId" = c."id"
      LEFT JOIN "ProductSize" ps ON ps."productId" = p."id"
      GROUP BY c."name"
      ORDER BY "totalUnits" DESC
    `;
    response.json(rows);
  } catch (error) {
    next(error);
  }
});

// ---------------------------------------------------------------------------
// Write endpoints (catalogue management)
// ---------------------------------------------------------------------------

app.post("/api/categories", async (request, response, next) => {
  try {
    const errors: string[] = [];
    const name = requiredText(request.body?.name, "name", errors, 60);
    const id = slugify(name);
    if (name && !id) errors.push("name must contain letters or numbers.");
    if (errors.length > 0) throw new ValidationError(errors);

    const category = await prisma.category.create({ data: { id, name } });
    response.status(201).json(category);
  } catch (error) {
    next(error);
  }
});

app.delete("/api/categories/:id", async (request, response, next) => {
  try {
    await prisma.category.delete({ where: { id: request.params.id } });
    response.status(204).end();
  } catch (error) {
    next(error);
  }
});

app.post("/api/products", async (request, response, next) => {
  try {
    const body = request.body ?? {};
    const errors: string[] = [];

    const id = typeof body.id === "string" ? body.id.trim().toUpperCase() : "";
    if (!/^[A-Z]{2}-[0-9]{3}$/.test(id)) errors.push("id must look like SH-003 (two letters, a dash, three digits).");
    const name = requiredText(body.name, "name", errors, 120);
    const description = requiredText(body.description, "description", errors, 1000);
    const categoryName = requiredText(body.category, "category", errors, 60);
    const imagePath = optionalText(body.image, "image", errors, 300) ?? null;
    const material = optionalText(body.specifications?.material, "specifications.material", errors) ?? null;
    const fit = optionalText(body.specifications?.fit, "specifications.fit", errors) ?? null;
    const care = optionalText(body.specifications?.care, "specifications.care", errors) ?? null;
    const sizes = parseSizes(body.sizes, errors) ?? [];
    if (errors.length > 0) throw new ValidationError(errors);

    const category = await prisma.category.findFirst({
      where: { name: { equals: categoryName, mode: "insensitive" } }
    });
    if (!category) throw new ValidationError([`Category "${categoryName}" does not exist.`]);

    const product = await prisma.product.create({
      data: {
        id,
        name,
        description,
        imagePath,
        category: { connect: { id: category.id } },
        specifications: { create: { material, fit, care } },
        sizes: { create: sizeCreateInput(sizes) }
      },
      include: productInclude
    });

    response.status(201).json(toCatalogueProduct(product));
  } catch (error) {
    next(error);
  }
});

app.patch("/api/products/:id", async (request, response, next) => {
  try {
    const body = request.body ?? {};
    const errors: string[] = [];

    const name = body.name === undefined ? undefined : requiredText(body.name, "name", errors, 120);
    const description = body.description === undefined ? undefined : requiredText(body.description, "description", errors, 1000);
    const categoryName = body.category === undefined ? undefined : requiredText(body.category, "category", errors, 60);
    const imagePath = optionalText(body.image, "image", errors, 300);
    const specs = body.specifications ?? {};
    const material = optionalText(specs.material, "specifications.material", errors);
    const fit = optionalText(specs.fit, "specifications.fit", errors);
    const care = optionalText(specs.care, "specifications.care", errors);
    const sizes = parseSizes(body.sizes, errors);
    if (errors.length > 0) throw new ValidationError(errors);

    let categoryId: string | undefined;
    if (categoryName) {
      const category = await prisma.category.findFirst({
        where: { name: { equals: categoryName, mode: "insensitive" } }
      });
      if (!category) throw new ValidationError([`Category "${categoryName}" does not exist.`]);
      categoryId = category.id;
    }

    const specificationChanges = {
      ...(material !== undefined ? { material } : {}),
      ...(fit !== undefined ? { fit } : {}),
      ...(care !== undefined ? { care } : {})
    };

    // A transaction keeps the product, its specifications and its sizes consistent:
    // either every change is saved or none is.
    const product = await prisma.$transaction(async (tx) => {
      await tx.product.update({
        where: { id: request.params.id },
        data: {
          ...(name !== undefined ? { name } : {}),
          ...(description !== undefined ? { description } : {}),
          ...(imagePath !== undefined ? { imagePath } : {}),
          ...(categoryId ? { category: { connect: { id: categoryId } } } : {}),
          ...(Object.keys(specificationChanges).length > 0
            ? {
                specifications: {
                  upsert: { create: specificationChanges, update: specificationChanges }
                }
              }
            : {})
        }
      });

      if (sizes) {
        await tx.productSize.deleteMany({ where: { productId: request.params.id } });
        await tx.product.update({
          where: { id: request.params.id },
          data: { sizes: { create: sizeCreateInput(sizes) } }
        });
      }

      return tx.product.findUniqueOrThrow({ where: { id: request.params.id }, include: productInclude });
    });

    response.json(toCatalogueProduct(product));
  } catch (error) {
    next(error);
  }
});

// Set the stock level for one size of a product (adds the size if it is new).
app.put("/api/products/:id/sizes/:label", async (request, response, next) => {
  try {
    const label = normalizeSizeLabel(request.params.label);
    const stockQuantity = request.body?.stockQuantity;
    const errors: string[] = [];
    if (!isValidSizeLabel(label)) errors.push(`"${label}" is not a supported size.`);
    if (!Number.isInteger(stockQuantity) || stockQuantity < 0) errors.push("stockQuantity must be a whole number of 0 or more.");
    if (errors.length > 0) throw new ValidationError(errors);

    const product = await prisma.product.findUnique({ where: { id: request.params.id }, select: { id: true } });
    if (!product) {
      response.status(404).json({ error: "Product not found." });
      return;
    }

    const size = await prisma.size.upsert({
      where: { label },
      update: {},
      create: { label, sizeSystem: sizeSystemFor(label), sortOrder: sortOrderFor(label) }
    });

    const entry = await prisma.productSize.upsert({
      where: { productId_sizeId: { productId: request.params.id, sizeId: size.id } },
      update: { stockQuantity },
      create: { productId: request.params.id, sizeId: size.id, stockQuantity }
    });

    response.json({ productId: entry.productId, size: label, stockQuantity: entry.stockQuantity });
  } catch (error) {
    next(error);
  }
});

app.delete("/api/products/:id/sizes/:label", async (request, response, next) => {
  try {
    const label = normalizeSizeLabel(request.params.label);
    const size = await prisma.size.findUnique({ where: { label } });
    if (!size) {
      response.status(404).json({ error: `Size "${label}" not found.` });
      return;
    }
    await prisma.productSize.delete({
      where: { productId_sizeId: { productId: request.params.id, sizeId: size.id } }
    });
    response.status(204).end();
  } catch (error) {
    next(error);
  }
});

app.delete("/api/products/:id", async (request, response, next) => {
  try {
    // Specifications and size links are removed automatically by ON DELETE CASCADE.
    await prisma.product.delete({ where: { id: request.params.id } });
    response.status(204).end();
  } catch (error) {
    next(error);
  }
});

// ---------------------------------------------------------------------------
// Error handling
// ---------------------------------------------------------------------------

app.use((error: unknown, _request: Request, response: Response, _next: NextFunction) => {
  if (error instanceof ValidationError) {
    response.status(400).json({ error: "Invalid data.", details: error.details });
    return;
  }

  switch (prismaErrorCode(error)) {
    case "P2002": // unique constraint
      response.status(409).json({ error: "A record with this id or name already exists." });
      return;
    case "P2003": // foreign key (e.g. deleting a category that still has products)
    case "P2014": // required relation would be broken
      response.status(409).json({ error: "This record is still referenced by other records." });
      return;
    case "P2025": // record to update or delete was not found
      response.status(404).json({ error: "Record not found." });
      return;
    case "P2004": // database constraint failed (CHECK)
      response.status(400).json({ error: "The data breaks a database rule." });
      return;
  }

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
