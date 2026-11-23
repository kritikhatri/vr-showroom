#!/usr/bin/env bash
# Database <-> backend integration demo for the DE submission.
#
# Calls the running Express API (create, read, update, delete) and after each
# write checks PostgreSQL directly with psql, proving that data sent through
# the application is stored in, and removed from, the database.
#
# Requirements: the API running (`cd backend && npm run dev`), curl, psql.
# On Windows run this in Git Bash or WSL.
#
# Usage (from the repository root):
#   bash DE/integration/demo.sh | tee DE/outputs/integration_demo_output.txt

set -u

API_URL="${API_URL:-http://127.0.0.1:3000}"

# Read DATABASE_URL from backend/.env unless it is already set.
if [ -z "${DATABASE_URL:-}" ] && [ -f backend/.env ]; then
  DATABASE_URL="$(grep -E '^DATABASE_URL=' backend/.env | head -1 | cut -d= -f2- | tr -d '"')"
fi
if [ -z "${DATABASE_URL:-}" ]; then
  echo "DATABASE_URL is not set and backend/.env was not found. Run this from the repository root." >&2
  exit 1
fi
# psql does not understand Prisma's ?schema=public parameter.
PSQL_URL="${DATABASE_URL%%\?*}"

pretty() {
  if command -v node >/dev/null 2>&1; then
    node -e 'let s="";process.stdin.on("data",d=>s+=d).on("end",()=>{try{console.log(JSON.stringify(JSON.parse(s),null,2))}catch{console.log(s)}})'
  else
    cat; echo
  fi
}

step() { printf '\n==================== %s ====================\n' "$1"; }

# call METHOD PATH [JSON_BODY]  - prints the HTTP status and the JSON response
call() {
  local method="$1" path="$2" body="${3:-}" response status
  echo "> $method $path"
  [ -n "$body" ] && echo "  body: $body"
  if [ -n "$body" ]; then
    response="$(curl -s -w '\n%{http_code}' -X "$method" -H 'Content-Type: application/json' -d "$body" "$API_URL$path")"
  else
    response="$(curl -s -w '\n%{http_code}' -X "$method" "$API_URL$path")"
  fi
  status="${response##*$'\n'}"
  echo "< HTTP $status"
  printf '%s' "${response%$'\n'*}" | pretty
}

db() {
  echo "> psql: $1"
  psql "$PSQL_URL" -P pager=off -c "$1"
}

echo "ThreadHaven VR Showroom - database integration demo"
echo "API: $API_URL"
date

step "1. API can reach PostgreSQL"
call GET /api/health

step "2. READ: categories with product counts (SELECT + COUNT)"
call GET /api/categories

step "3. CREATE: add a new product through the API (INSERT into 3 tables)"
call POST /api/products '{
  "id": "SH-004",
  "name": "Denim Overshirt",
  "category": "Shirts",
  "description": "A heavyweight denim overshirt with chest pockets.",
  "specifications": { "material": "Cotton denim", "fit": "Relaxed", "care": "Machine wash cold" },
  "sizes": [ { "label": "M", "stockQuantity": 6 }, { "label": "L", "stockQuantity": 4 }, { "label": "S", "stockQuantity": 2 } ]
}'

step "4. Check the database directly: the new rows exist"
db "SELECT \"id\", \"name\", \"categoryId\", \"createdAt\" FROM \"Product\" WHERE \"id\" = 'SH-004';"
db "SELECT * FROM \"ProductSpecifications\" WHERE \"productId\" = 'SH-004';"
db "SELECT ps.\"productId\", s.\"label\", ps.\"stockQuantity\" FROM \"ProductSize\" ps JOIN \"Size\" s ON s.\"id\" = ps.\"sizeId\" WHERE ps.\"productId\" = 'SH-004' ORDER BY s.\"sortOrder\";"

step "5. READ: the product as Unity receives it (availableSizes rebuilt from ProductSize)"
call GET /api/products/SH-004

step "6. UPDATE: change care instructions (PATCH)"
call PATCH /api/products/SH-004 '{ "specifications": { "care": "Wash inside out, cold" } }'
db "SELECT \"productId\", \"care\" FROM \"ProductSpecifications\" WHERE \"productId\" = 'SH-004';"

step "7. UPDATE: size S sells out (stock = 0)"
call PUT /api/products/SH-004/sizes/S '{ "stockQuantity": 0 }'
db "SELECT s.\"label\", ps.\"stockQuantity\" FROM \"ProductSize\" ps JOIN \"Size\" s ON s.\"id\" = ps.\"sizeId\" WHERE ps.\"productId\" = 'SH-004' ORDER BY s.\"sortOrder\";"

step "8. READ: filtered catalogue used by the Unity category page"
call GET '/api/products?category=Shirts'

step "9. READ: stock report (raw SQL with JOIN + GROUP BY in the API)"
call GET /api/reports/stock-by-category

step "10. VALIDATION: invalid data is rejected with HTTP 400"
call POST /api/products '{ "id": "bad", "name": "", "category": "Shirts", "description": "x", "sizes": [ { "label": "M", "stockQuantity": -3 } ] }'

step "11. DUPLICATE: the same product id again is rejected with HTTP 409 (primary key)"
call POST /api/products '{ "id": "SH-004", "name": "Copy", "category": "Shirts", "description": "Duplicate id." }'

step "12. REFERENTIAL INTEGRITY: a category with products cannot be deleted (409, ON DELETE RESTRICT)"
call DELETE /api/categories/shirts

step "13. DELETE: remove the product (child rows go with it: ON DELETE CASCADE)"
call DELETE /api/products/SH-004
db "SELECT (SELECT COUNT(*) FROM \"Product\" WHERE \"id\" = 'SH-004') AS product_rows, (SELECT COUNT(*) FROM \"ProductSpecifications\" WHERE \"productId\" = 'SH-004') AS spec_rows, (SELECT COUNT(*) FROM \"ProductSize\" WHERE \"productId\" = 'SH-004') AS size_rows;"

step "14. READ: deleted product now returns 404"
call GET /api/products/SH-004

echo
echo "Demo finished. The database is back to its starting state."
