# Member 3 – UI, Product Information, and Documentation

Member 3 supplies the UI scripts and data flow for the virtual garment showroom. The scripts support category selection, product-card search and filtering, product details, world-product selection, and navigation between UI panels.

## Current implementation

- A sample JSON catalogue with eight example garments across Shirts, Jackets, Trousers, and Dresses.
- Reusable `ProductCard` setup, selection feedback, and catalogue filtering/search.
- `ProductInformationPanel` displays data from either the JSON catalogue or Member 2's selected scene `Product`.
- `ProductCatalogueNavigation` and the product information panel provide Home, Back, and Close actions.
- The main menu button script targets Member 1's `ThreadHaven_Showroom` scene.

## Set up the scenes in Unity

The scripts and reference scene are committed, but the checked-in menu scene contains only an empty Canvas scaffold. The showroom scene contains Member 1's environment; the Member 2 Player prefab and Member 3 catalogue UI are not yet placed and linked in that scene. Finish the Inspector wiring before expecting the full click-through experience to run.

1. Open `Assets/Member1/Scenes/ThreadHaven_Showroom.unity`.
2. Add `Assets/Member2/Prefabs/Player.prefab` and confirm its camera and `ProductInteractor` input action references.
3. Add a Canvas and EventSystem with category, catalogue, and product-information panels.
4. Add `ProductCatalogueNavigation` and assign the Home, category, and catalogue panels and their buttons.
5. Add `ProductCatalogue`; assign `Assets/Member3/Products/sample-products.json`, the information panel, card list, search field, clear button, result count, and empty-results message as applicable.
6. Assign the text, image, placeholder, and navigation button fields on `ProductInformationPanel`.
7. Add Member 2's `Product` component to selectable 3D products and enter their names, categories, descriptions, specifications, and optional prices.
8. Add `Assets/Member3/UI/MainMenuScene.unity` to Build Profiles if using the menu as the app entry scene. Build Profiles should list the menu first and `ThreadHaven_Showroom` after it. Add the menu's visible controls and assign its `MainMenuButtons` references.

See [ShowroomScene_Integration.md](UI/ShowroomScene_Integration.md) for the full scene wiring checklist.

## How to use

After scene wiring is complete:

1. Select **Enter Showroom** to load `ThreadHaven_Showroom`.
2. Use the controls configured on Member 2's player prefab to move and look around.
3. Select a product card to open its JSON-backed detail panel, or target and select a 3D product to show its scene `Product` details.
4. Use the category buttons and search field together to filter the catalogue. Clearing the search keeps the selected category.
5. Use **Back**, **Home**, and **Close** to return to the appropriate panel or hide product details.

`ProductInteractor` shows a temporary crosshair and prompt while its debug UI is enabled. Its default selection accepts the configured Interact action or left mouse click.

## Product data and images

The sample data is in [sample-products.json](Products/sample-products.json). Each record has an `id`, `name`, `category`, `description`, `specifications`, and optional `image` path. If an image path is set, place the sprite in a `Resources` folder and store the path without its extension, for example `Products/Shirts/classic-tee`. Empty paths use the panel's image placeholder.

The sample garments and their specifications are illustrative. Replace them with verified project-approved data before presenting them as real product specifications.

## Documentation and visuals

- [Product data model](Products/ProductData_Schema.md)
- [Product card selection and details](Products/ProductInformation_Integration.md)
- [World product selection integration](Products/ProductSelection_Integration.md)
- [Product search](Products/ProductSearch_Setup.md)
- [Category filtering](Products/ProductFiltering_Setup.md)
- [Navigation controls](UI/NavigationControls_Setup.md)
- [Commit 19 UI fixes](UI/Commit19_UI_Refinements.md)
- [Layout reference visuals and screenshot status](Documentation/Screenshots/README.md)

The included visuals are layout references, not captures of a running Unity UI. Runtime screenshots should be captured after the menu controls and showroom UI are assembled in Unity.
