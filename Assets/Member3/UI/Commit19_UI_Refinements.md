# Commit 19 – UI Refinements and Fixes

- Product-card selection now stays highlighted after the pointer leaves and clears when another card is selected.
- Product cards handle missing names, categories, IDs, and thumbnail sprites without stale or blank-looking content.
- Main-menu hover feedback now brightens and restores only the hovered button, preserving the other buttons' colors.

`ProductCard.OnProductCardSelected` supplies the selected card to the catalogue. The catalogue can highlight one card and use its product ID to show the corresponding information panel.
