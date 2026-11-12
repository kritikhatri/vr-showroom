# Main Menu Background – UI Layout Notes

## Hierarchy Structure (in Unity Scene)
```
MainMenuCanvas
└── Background_Layer
    ├── BackgroundPanel          ← Full-screen dark navy Image
    ├── GradientOverlay          ← Semi-transparent overlay Image
    ├── LogoImage                ← VR Showroom project logo (top-centre)
    └── AccentLine               ← Thin blue horizontal divider line
```

## Colour Theme
| Element          | Colour (RGBA)              | Purpose                        |
|------------------|----------------------------|--------------------------------|
| Background       | (0.05, 0.05, 0.15, 1.0)   | Dark navy — VR/tech feel       |
| Gradient Overlay | (0.08, 0.08, 0.25, 0.75)  | Depth / atmosphere             |
| Accent Line      | (0.20, 0.60, 1.00, 1.0)   | Bright blue — modern highlight |

## Logo Placement
- **Size:** 320 × 160 px
- **Anchor:** Top-centre
- **Position:** (0, 200) from canvas centre

## Script
Controlled by `MainMenuBackground.cs` — assign all Image references in Inspector.
