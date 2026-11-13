# Category Selection Panel – UI Layout Notes

## Hierarchy Structure (in Unity Scene)
```
MainMenuCanvas
└── CategoryPanel
    ├── PanelTitle              ← "Select a Category" (Bold, 40px, White)
    ├── CategoryButtonsGrid
    │   ├── AllButton           ← "All"      (Blue when active)
    │   ├── ShirtsButton        ← "Shirts"   (Blue when active)
    │   ├── JacketsButton       ← "Jackets"  (Blue when active)
    │   ├── TrousersButton      ← "Trousers" (Blue when active)
    │   └── DressesButton       ← "Dresses"  (Blue when active)
    └── CategoryIconRow         ← Optional icons per category
```

## Category Definitions

| Category | Products Shown         | Button Colour (Active)  |
|----------|------------------------|-------------------------|
| All      | Every product          | Blue (0.2, 0.6, 1.0)    |
| Shirts   | Shirt products only    | Blue (0.2, 0.6, 1.0)    |
| Jackets  | Jacket products only   | Blue (0.2, 0.6, 1.0)    |
| Trousers | Trouser products only  | Blue (0.2, 0.6, 1.0)    |
| Dresses  | Dress products only    | Blue (0.2, 0.6, 1.0)    |

## Colour States
- **Active (selected):** Bright blue `(0.2, 0.6, 1.0)`
- **Inactive:** Dark grey `(0.2, 0.2, 0.3)`

## Script
Controlled by `CategorySelectionPanel.cs`
Communicates with `ProductCatalogue.cs` via singleton instance.
