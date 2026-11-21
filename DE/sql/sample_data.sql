-- Sample records for the ThreadHaven VR showroom.
-- Products come from Assets/Member3/Products/sample-products.json (the same
-- file the Unity catalogue and backend/prisma/seed.ts use).
-- Stock levels are demo values, identical to SAMPLE_STOCK in seed.ts.
-- Run after schema.sql:  psql -d vr_showroom -f DE/sql/sample_data.sql

BEGIN;

INSERT INTO "Category" ("id", "name") VALUES
    ('shirts', 'Shirts'),
    ('jackets', 'Jackets'),
    ('trousers', 'Trousers'),
    ('dresses', 'Dresses');

INSERT INTO "Size" ("label", "sizeSystem", "sortOrder") VALUES
    ('XS', 'ALPHA', 2),
    ('S', 'ALPHA', 3),
    ('M', 'ALPHA', 4),
    ('L', 'ALPHA', 5),
    ('XL', 'ALPHA', 6),
    ('28', 'WAIST', 28),
    ('30', 'WAIST', 30),
    ('32', 'WAIST', 32),
    ('34', 'WAIST', 34),
    ('36', 'WAIST', 36);

INSERT INTO "Product" ("id", "name", "description", "imagePath", "categoryId", "updatedAt") VALUES
    ('SH-001', 'Classic Cotton Crew Neck T-Shirt',
     'A breathable everyday crew neck shirt made from soft cotton jersey.',
     NULL, 'shirts', CURRENT_TIMESTAMP),
    ('SH-002', 'Oxford Button-Down Shirt',
     'A durable woven shirt with a button-down collar for work or casual wear.',
     NULL, 'shirts', CURRENT_TIMESTAMP),
    ('JK-001', 'Lightweight Utility Jacket',
     'A lightweight outer layer with practical front pockets and a zip closure.',
     NULL, 'jackets', CURRENT_TIMESTAMP),
    ('JK-002', 'Quilted Insulated Jacket',
     'A quilted jacket designed to provide a warm, comfortable layer in cool weather.',
     NULL, 'jackets', CURRENT_TIMESTAMP),
    ('TR-001', 'Straight-Leg Work Trousers',
     'Hard-wearing work trousers with a straight-leg cut and reinforced seams.',
     NULL, 'trousers', CURRENT_TIMESTAMP),
    ('TR-002', 'Stretch Chino Trousers',
     'Versatile chinos made with a small amount of stretch for ease of movement.',
     NULL, 'trousers', CURRENT_TIMESTAMP),
    ('DR-001', 'Everyday A-Line Dress',
     'A comfortable knee-length A-line dress with short sleeves and a simple finish.',
     NULL, 'dresses', CURRENT_TIMESTAMP),
    ('DR-002', 'Formal Woven Midi Dress',
     'A woven midi dress with a clean silhouette suited to formal occasions.',
     NULL, 'dresses', CURRENT_TIMESTAMP);

INSERT INTO "ProductSpecifications" ("productId", "material", "fit", "care") VALUES
    ('SH-001', '100% cotton', 'Regular', 'Machine wash cold'),
    ('SH-002', 'Cotton Oxford cloth', 'Regular', 'Machine wash cold; iron warm'),
    ('JK-001', 'Cotton-polyester twill', 'Relaxed', 'Machine wash cold'),
    ('JK-002', 'Recycled polyester shell; polyester fill', 'Regular', 'Gentle machine wash'),
    ('TR-001', 'Cotton-polyester canvas', 'Straight leg', 'Machine wash warm'),
    ('TR-002', 'Cotton with elastane', 'Slim straight', 'Machine wash cold'),
    ('DR-001', 'Cotton poplin', 'A-line', 'Machine wash cold; iron warm'),
    ('DR-002', 'Polyester-viscose blend', 'Regular', 'Gentle machine wash');

-- Many-to-many links: which sizes each product is offered in, with stock.
INSERT INTO "ProductSize" ("productId", "sizeId", "stockQuantity")
SELECT v.product_id, s."id", v.stock
FROM (VALUES
    ('SH-001', 'S', 18),
    ('SH-001', 'M', 25),
    ('SH-001', 'L', 22),
    ('SH-001', 'XL', 9),
    ('SH-002', 'S', 12),
    ('SH-002', 'M', 20),
    ('SH-002', 'L', 16),
    ('SH-002', 'XL', 0),
    ('JK-001', 'S', 7),
    ('JK-001', 'M', 14),
    ('JK-001', 'L', 11),
    ('JK-001', 'XL', 4),
    ('JK-002', 'S', 5),
    ('JK-002', 'M', 9),
    ('JK-002', 'L', 8),
    ('JK-002', 'XL', 3),
    ('TR-001', '28', 6),
    ('TR-001', '30', 15),
    ('TR-001', '32', 19),
    ('TR-001', '34', 12),
    ('TR-001', '36', 2),
    ('TR-002', '28', 10),
    ('TR-002', '30', 21),
    ('TR-002', '32', 24),
    ('TR-002', '34', 13),
    ('TR-002', '36', 5),
    ('DR-001', 'XS', 4),
    ('DR-001', 'S', 11),
    ('DR-001', 'M', 16),
    ('DR-001', 'L', 10),
    ('DR-001', 'XL', 6),
    ('DR-002', 'XS', 0),
    ('DR-002', 'S', 6),
    ('DR-002', 'M', 9),
    ('DR-002', 'L', 7),
    ('DR-002', 'XL', 3)
) AS v(product_id, size_label, stock)
JOIN "Size" s ON s."label" = v.size_label;

COMMIT;
