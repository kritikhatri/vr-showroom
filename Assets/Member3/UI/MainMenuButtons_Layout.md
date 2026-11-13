# Main Menu Buttons – UI Layout Notes

## Button Hierarchy (in Unity Scene)
```
MainMenuCanvas
└── MainMenuPanel
    └── ButtonsContainer
        ├── EnterShowroomButton   ← Blue   | "Enter Showroom"
        ├── ExploreProductsButton ← Green  | "Explore Products"
        └── ExitButton            ← Red    | "Exit"
```

## Button Specifications

| Button            | Colour     | Action                              | Size (px)   |
|-------------------|------------|-------------------------------------|-------------|
| Enter Showroom    | Blue       | Loads Member 1's showroom scene     | 300 × 60    |
| Explore Products  | Green      | Opens product catalogue panel       | 300 × 60    |
| Exit              | Red        | Quits the application               | 300 × 60    |

## Button Flow
```
[Enter Showroom]    →  SceneManager.LoadScene("ShowroomScene")
[Explore Products]  →  productCataloguePanel.SetActive(true)
[Exit]              →  Application.Quit()
```

## Integration Notes
- "Enter Showroom" connects to **Member 1's** 3D environment scene
- "Explore Products" connects to **Member 3's** product catalogue panel
- Scene name must match exactly what is set in Unity Build Settings

## Script
Controlled by `MainMenuButtons.cs`
Assign Button references and panel references in Inspector.
