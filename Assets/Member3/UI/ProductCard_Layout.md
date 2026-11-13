# Product Card Component – UI Layout Notes

## Prefab Hierarchy Structure
```
ProductCard (RectTransform, CanvasRenderer, Image, Button, ProductCard.cs)
├── CardBackground          ← Rounded dark frame container
├── ThumbnailContainer
│   ├── PlaceholderImage    ← Fallback neutral background
│   └── ProductImage        ← Scaled product thumbnail (preserveAspect: true)
├── InfoContainer (VerticalLayoutGroup)
│   ├── CategoryBadge       ← Horizontal layout with small uppercase tag
│   │   └── BadgeText       ← "SHIRTS" / "JACKETS" (12px, accent color)
│   ├── ProductNameText     ← Item title (18px, Semi-Bold, White)
│   └── SkuOrPriceText      ← Identifier/Price (14px, muted cyan/grey)
└── SelectionHighlight      ← Active state border/outline indicator
```

## Dimensions & Layout Specs
- **Default Card Dimensions:** 220px (width) × 280px (height)
- **Image Aspect Ratio:** 1:1 square thumbnail frame (180px × 180px)
- **Margins & Padding:** 12px outer padding, 8px inner spacing
- **Typography:**
  - Product Title: TextMeshPro 18pt SemiBold
  - Category Tag: TextMeshPro 11pt Bold
  - SKU / Price: TextMeshPro 13pt Regular

## Interactive Visual States
| State    | Color (RGBA)                 | Description |
|----------|------------------------------|-------------|
| Normal   | `(0.12, 0.14, 0.22, 0.95)`   | Slate navy dark card base |
| Hover    | `(0.18, 0.22, 0.35, 1.00)`   | Subtle lighting upon mouse over |
| Selected | `(0.15, 0.45, 0.85, 1.00)`   | Vibrant blue active highlight |

## Integration
- Script: `ProductCard.cs`
- Emits event: `ProductCard.OnProductCardSelected` with the selected item's `productId`.
- Seamlessly connects to catalogue grid generators and product detail panels.
