# Product catalogue data

`sample-products.json` contains eight illustrative garment entries: two each for Shirts, Jackets, Trousers, and Dresses. Each product has an ID, name, category, description, and garment specifications. Image paths are currently empty, so UI image placeholders are expected.

`ProductCatalogue` parses the JSON, indexes products by ID, and combines name search with category filtering. Selecting a product card opens `ProductInformationPanel` with its catalogue details. Selecting a 3D product displays the values from Member 2's scene `Product` component instead.

## Add or edit a product

1. Open `sample-products.json` in a text editor.
2. Add a record to the `products` array using the field names and types described in [ProductData_Schema.md](ProductData_Schema.md).
3. Keep IDs unique; the catalogue ignores duplicate or blank IDs.
4. If supplying a thumbnail, put the sprite under a Unity `Resources` folder and enter its resource path without the extension. For example, `Assets/Resources/Products/Shirts/classic-tee.png` uses `Products/Shirts/classic-tee`.
5. Assign the JSON TextAsset to `ProductCatalogue` in the Inspector and make the card ID match the record ID.

The catalogue contents are example project data and are not verified specifications for real products.
