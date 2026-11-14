# Commit 16 – Back, Home, and Close Controls

`ProductCatalogueNavigation` supplies the existing panel flow: category **Home** returns to the main menu, catalogue **Back** returns to categories, and catalogue **Home** returns to the main menu. `ProductInformationPanel` now has optional **Back**, **Home**, and **Close** button references.

## Unity setup

1. Assign the category panel's Home button and the catalogue panel's Back and Home buttons to `ProductCatalogueNavigation`.
2. Assign the product detail panel's Back, Home, and Close buttons to `ProductInformationPanel`.
3. Back from product details returns to the catalogue; Home returns to the main menu; Close hides only the detail panel.

If no `ProductCatalogueNavigation` exists in the scene, Back and Home hide the detail panel as a fallback. The detail component preserves selected product data when first activated, so opening an initially inactive details panel does not reset its content.
