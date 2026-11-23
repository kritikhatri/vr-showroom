-- =====================================================================
-- ThreadHaven VR Garment Showroom – PostgreSQL schema (3NF)
-- Run:  psql -d vr_showroom -f DE/04_schema.sql
-- =====================================================================

DROP VIEW  IF EXISTS v_product_catalogue;
DROP TABLE IF EXISTS enquiries, product_views, visitors, showroom_displays,
                     product_sizes, sizes, product_specifications, products,
                     collections, categories CASCADE;

-- ---------------------------------------------------------------------
-- 1. Lookup tables
-- ---------------------------------------------------------------------
CREATE TABLE categories (
    category_id   SMALLINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name          VARCHAR(50)  NOT NULL UNIQUE,
    description   TEXT
);

CREATE TABLE collections (
    collection_id SMALLINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name          VARCHAR(60)  NOT NULL UNIQUE,
    season        VARCHAR(12)  NOT NULL
                  CHECK (season IN ('Spring', 'Summer', 'Autumn', 'Winter', 'All Season')),
    launch_year   SMALLINT     NOT NULL CHECK (launch_year BETWEEN 2000 AND 2100)
);

CREATE TABLE sizes (
    size_id       SMALLINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    label         VARCHAR(10)  NOT NULL UNIQUE,          -- 'S', 'M', '32' ...
    size_system   VARCHAR(10)  NOT NULL CHECK (size_system IN ('LETTER', 'WAIST')),
    sort_order    SMALLINT     NOT NULL
);

-- ---------------------------------------------------------------------
-- 2. Core product data
-- ---------------------------------------------------------------------
CREATE TABLE products (
    product_id    VARCHAR(10)   PRIMARY KEY
                  CHECK (product_id ~ '^[A-Z]{2}-[0-9]{3}$'),   -- e.g. SH-001
    name          VARCHAR(120)  NOT NULL CHECK (length(trim(name)) > 0),
    description   TEXT          NOT NULL,
    price         NUMERIC(10,2) NOT NULL CHECK (price > 0),       -- INR
    category_id   SMALLINT      NOT NULL
                  REFERENCES categories(category_id) ON DELETE RESTRICT,
    collection_id SMALLINT
                  REFERENCES collections(collection_id) ON DELETE SET NULL,
    image_path    VARCHAR(255),
    is_active     BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ   NOT NULL DEFAULT now()
);

-- 1 : 1 with products (shared primary key)
CREATE TABLE product_specifications (
    product_id    VARCHAR(10)  PRIMARY KEY
                  REFERENCES products(product_id) ON DELETE CASCADE,
    material      VARCHAR(120) NOT NULL,
    fit           VARCHAR(40),
    care          VARCHAR(120)
);

-- M : N between products and sizes, with stock as a relationship attribute
CREATE TABLE product_sizes (
    product_id    VARCHAR(10)  NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    size_id       SMALLINT     NOT NULL REFERENCES sizes(size_id)       ON DELETE RESTRICT,
    stock_qty     INTEGER      NOT NULL DEFAULT 0 CHECK (stock_qty >= 0),
    PRIMARY KEY (product_id, size_id)
);

-- Where each product sits inside the 3D Unity showroom (1 : 1 with products)
CREATE TABLE showroom_displays (
    display_id    SMALLINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id    VARCHAR(10)  NOT NULL UNIQUE
                  REFERENCES products(product_id) ON DELETE CASCADE,
    zone          VARCHAR(30)  NOT NULL
                  CHECK (zone IN ('Left Rack', 'Right Rack', 'Left Pedestal',
                                  'Right Pedestal', 'Centre Platform')),
    scene_object  VARCHAR(80)  NOT NULL UNIQUE   -- GameObject name in ThreadHaven_Showroom
);

-- ---------------------------------------------------------------------
-- 3. Visitors and their activity in the showroom
-- ---------------------------------------------------------------------
CREATE TABLE visitors (
    visitor_id    INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    full_name     VARCHAR(100) NOT NULL,
    email         VARCHAR(255) NOT NULL UNIQUE
                  CHECK (email ~* '^[^@\s]+@[^@\s]+\.[^@\s]+$'),
    company       VARCHAR(120),
    city          VARCHAR(60),
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
);

-- Written by the backend every time a user presses E on a product in VR
CREATE TABLE product_views (
    view_id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id       VARCHAR(10)  NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    visitor_id       INTEGER      REFERENCES visitors(visitor_id) ON DELETE SET NULL,
    session_id       UUID         NOT NULL,
    viewed_at        TIMESTAMPTZ  NOT NULL DEFAULT now(),
    duration_seconds NUMERIC(6,1) CHECK (duration_seconds >= 0)
);

-- Bulk-order / quotation requests from buyers
CREATE TABLE enquiries (
    enquiry_id    INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    visitor_id    INTEGER      NOT NULL REFERENCES visitors(visitor_id) ON DELETE CASCADE,
    product_id    VARCHAR(10)  NOT NULL REFERENCES products(product_id) ON DELETE RESTRICT,
    size_id       SMALLINT     REFERENCES sizes(size_id) ON DELETE SET NULL,
    quantity      INTEGER      NOT NULL CHECK (quantity > 0),
    message       TEXT,
    status        VARCHAR(10)  NOT NULL DEFAULT 'Open'
                  CHECK (status IN ('Open', 'Quoted', 'Closed')),
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- 4. Indexes (primary keys and UNIQUE columns are indexed automatically)
-- ---------------------------------------------------------------------
CREATE INDEX idx_products_category    ON products (category_id);
CREATE INDEX idx_products_collection  ON products (collection_id);
CREATE INDEX idx_products_name_lower  ON products (lower(name));
CREATE INDEX idx_product_sizes_size   ON product_sizes (size_id);
CREATE INDEX idx_views_product_time   ON product_views (product_id, viewed_at);
CREATE INDEX idx_views_visitor        ON product_views (visitor_id);
CREATE INDEX idx_enquiries_visitor    ON enquiries (visitor_id);
CREATE INDEX idx_enquiries_product    ON enquiries (product_id);
CREATE INDEX idx_enquiries_open       ON enquiries (created_at) WHERE status = 'Open';

-- ---------------------------------------------------------------------
-- 5. Keep products.updated_at current on every UPDATE
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION set_updated_at() RETURNS trigger AS $$
BEGIN
    NEW.updated_at := now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_products_updated_at
    BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ---------------------------------------------------------------------
-- 6. Read model used by the API (joins the normalized tables back together)
-- ---------------------------------------------------------------------
CREATE VIEW v_product_catalogue AS
SELECT p.product_id,
       p.name,
       c.name                                        AS category,
       col.name                                      AS collection,
       p.price,
       p.description,
       s.material,
       s.fit,
       s.care,
       string_agg(sz.label, ', ' ORDER BY sz.sort_order)
           FILTER (WHERE ps.stock_qty > 0)           AS sizes_in_stock,
       COALESCE(sum(ps.stock_qty), 0)                AS total_stock,
       d.zone                                        AS showroom_zone,
       p.image_path
FROM products p
JOIN categories c                   ON c.category_id   = p.category_id
LEFT JOIN collections col           ON col.collection_id = p.collection_id
LEFT JOIN product_specifications s  ON s.product_id    = p.product_id
LEFT JOIN product_sizes ps          ON ps.product_id   = p.product_id
LEFT JOIN sizes sz                  ON sz.size_id      = ps.size_id
LEFT JOIN showroom_displays d       ON d.product_id    = p.product_id
WHERE p.is_active
GROUP BY p.product_id, c.name, col.name, s.material, s.fit, s.care, d.zone;
