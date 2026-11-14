# Commit 15 – Product Category Filtering

`ProductCatalogue` now applies category and name filters to the loaded product records and the visible cards. When a card ID matches a JSON record, the JSON category is authoritative; cards without a matching record fall back to their category set by `ProductCard.SetupCard`.

The category buttons continue to call `FilterByCategory`. The active category and search query are combined, so changing a category preserves the current name search. **All** clears the category restriction while keeping the search query.

Scripts and other UI can read `ProductCatalogue.FilteredProducts` for the current data records, and `VisibleProductCount` for the cards currently shown. Assign the product JSON TextAsset for data-level filtering; without it, card filtering still uses each card's category.
