# Commit 14 – Product Search

`ProductCatalogue` searches product names without regard to letter case. Search results combine with the currently selected category, so a search inside **Shirts** returns only matching shirts.

## Unity setup

1. Add a TextMeshPro input field to the catalogue panel and assign it to **Search Input** on `ProductCatalogue`.
2. Optionally assign a clear button to **Clear Search Button**.
3. Optionally assign a TextMeshPro label to **Result Count Text** and a message object such as “No products found” to **No Results Message**.
4. Confirm the cards have product IDs matching the JSON and product names in either their card setup or the product data file.

Typing filters the visible cards immediately. Clearing the query restores results for the active category.
