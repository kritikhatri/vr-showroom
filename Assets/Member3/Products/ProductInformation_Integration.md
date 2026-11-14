# Commit 13 – Dynamic Product Information

`ProductCatalogue` loads the assigned JSON `TextAsset` into `Member3.Data.ProductCatalogueData`, indexes records by ID, and listens for `ProductCard.OnProductCardSelected`. A card selection looks up the matching record and displays its name, category, ID, description, and non-empty specifications in `ProductInformationPanel`.

## Unity setup

1. Select the GameObject with `ProductCatalogue` and assign `Assets/Member3/Products/sample-products.json` to **Product Data Json**.
2. Assign the `ProductInformationPanel` component to **Information Panel**.
3. Ensure each card's `ProductCard.SetupCard` ID matches a record's `id` in the JSON.
4. Assign the panel's image and text components in the Inspector.

Product images are optional. If an `image` field is populated, it is treated as a `Resources` path (without the file extension), for example `Products/Shirts/classic-tee`. Empty image fields keep the image placeholder visible. Missing IDs and unassigned references produce Console messages.
