# Main Menu Text Elements – UI Layout Notes

## Text Hierarchy (in Unity Scene)
```
MainMenuCanvas
└── Text_Layer
    ├── ProjectTitleText      ← "VR GARMENT SHOWROOM"  (Bold, 72px, Blue)
    ├── WelcomeText           ← "Welcome to the Virtual Showroom" (Italic, 36px, White)
    ├── IntroDescriptionText  ← 2–3 line description paragraph (22px, Light Grey)
    └── VersionText           ← "v1.0 | MSME Major Project 2023" (14px, Grey, bottom-right)
```

## Text Specifications

| Element          | Font Size | Style  | Colour        | Alignment     |
|------------------|-----------|--------|---------------|---------------|
| Project Title    | 72px      | Bold   | Bright Blue   | Centre        |
| Welcome Message  | 36px      | Italic | White         | Centre        |
| Intro Description| 22px      | Normal | Light Grey    | Centre        |
| Version Label    | 14px      | Normal | Grey          | Bottom-Right  |

## Content
- **Title:** VR GARMENT SHOWROOM
- **Welcome:** Welcome to the Virtual Showroom
- **Intro:** Explore our exclusive collection of garments in an immersive
  Virtual Reality environment. Browse Shirts, Jackets, Trousers and Dresses.
- **Version:** v1.0 | MSME Major Project 2023

## Script
Controlled by `MainMenuText.cs` — assign all TMP_Text references in Inspector.
Uses **TextMeshPro** (TMP) for crisp, high-quality text rendering.
