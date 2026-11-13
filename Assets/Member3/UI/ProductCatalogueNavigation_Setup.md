# Commit 10 – Product Catalogue Navigation

`ProductCatalogueNavigation` manages the home, category selection, and product catalogue panels. The **Explore Products** button in `MainMenuButtons` opens category selection when this navigation component is present.

## Unity setup

1. Add `ProductCatalogueNavigation` to an active scene object.
2. Assign the home, category selection, and product catalogue panel objects.
3. Assign the category panel's **Home** and **View Products** buttons.
4. Assign the catalogue panel's **Home** and **Back to Categories** buttons.
5. Keep `MainMenuButtons` in the scene and connect its **Explore Products** button as before.

The controller exposes `ShowHome`, `ShowCategories`, and `ShowCatalogue` as public methods, so other UI buttons can call the same navigation flow. Missing panel references are reported in the Unity Console and leave the current panel unchanged.
