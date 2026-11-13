# Commit 11 – Product Information Panel Layout

## Panel hierarchy

```text
ProductInformationPanel
├── Header
│   ├── PanelTitle                 ← “Product Details”
│   └── ProductCategoryText        ← category label / badge
├── Content (HorizontalLayoutGroup)
│   ├── ImageColumn
│   │   ├── ProductImage            ← square, preserve aspect
│   │   └── ImagePlaceholder        ← shown until a product image is assigned
│   └── DetailsColumn (VerticalLayoutGroup)
│       ├── ProductNameText
│       ├── ProductIdText
│       ├── DescriptionHeading      ← “Description”
│       ├── ProductDescriptionText
│       ├── SpecificationsHeading   ← “Specifications”
│       └── SpecificationsText     ← one specification per line
└── NavigationFooter                ← Home / Back / Close controls added later
```

## Suggested dimensions and styling

- Panel: centered, approximately 1200 × 720 px, with 32 px inner padding.
- Image column: 420 px wide; square image up to 380 × 380 px.
- Details column: remaining width with 20 px spacing between sections.
- Background: deep slate `#171B2B`; image surface `#242A3D`.
- Main text: white; supporting text: cool grey; category badge: cyan accent.
- Product name: TextMeshPro 32 pt bold; section headings: 18 pt semibold; body: 16 pt.
- Description and specifications should wrap and remain top-aligned.

## Component references

Attach `ProductInformationPanel.cs` to the panel root and assign the image, placeholder, and text references. The component initializes an empty-state prompt and exposes `ShowPanel`, `HidePanel`, and `ClearProduct`. Product data binding and selection are added in later commits.
