# Commit 09 – Category Filtering Setup

`CategorySelectionPanel` sends the selected category to `ProductCatalogue`. The catalogue filters its registered `ProductCard` objects by the category string set through `ProductCard.SetupCard`.

## Unity setup

1. Add `ProductCatalogue` to the parent object that contains the product cards, or assign the cards in its **Product Cards** list.
2. Keep the category panel and catalogue controller in the same loaded scene so the singleton can receive selections.
3. Assign the category buttons to the matching fields on `CategorySelectionPanel`.
4. Set each card's category by calling `SetupCard` with `Shirts`, `Jackets`, `Trousers`, or `Dresses`.
5. Select **All** to show every registered card again.

Cards that are children of the catalogue object are collected automatically, including inactive cards. Cards created at runtime can be added with `RegisterCard`.

The controller exposes `ProductsFiltered` and `VisibleProductCount` for a result count or empty-state message in a later UI pass.
