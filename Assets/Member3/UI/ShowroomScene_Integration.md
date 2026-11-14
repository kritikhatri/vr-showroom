# Commit 18 – Showroom Scene Integration

`MainMenuButtons` now targets Member 1's checked-in scene, `ThreadHaven_Showroom`. That scene is included in Unity's build scene list, so the Enter Showroom action can load it by name.

## Finish the scene wiring in Unity

The checked-in showroom scene currently contains Member 1's environment, while the Member 2 player and Member 3 catalogue UI are separate assets/scripts. Add and connect them in the Unity Editor:

1. Open `Assets/Member1/Scenes/ThreadHaven_Showroom.unity`.
2. Add Member 2's `Assets/Member2/Prefabs/Player.prefab` to the scene. Confirm its `ProductInteractor` has its player camera and input actions assigned.
3. Add a screen-space Canvas and EventSystem for the catalogue and product details. Include the category, catalogue, and information panel objects in the scene.
4. Add `ProductCatalogueNavigation` and assign the home, category, and catalogue panels and their navigation buttons.
5. Add `ProductCatalogue` to the catalogue controller object. Assign `Assets/Member3/Products/sample-products.json`, the product information panel, and any catalogue card/search references in the Inspector.
6. Assign the information panel's text, image, and optional Back, Home, and Close button references.
7. Add the Member 2 `Product` component to each selectable showroom product and fill its name, category, description, specifications, and optional price. `ProductCatalogue` listens to `ProductInteractor.OnProductSelected` and opens the information panel with those details.
8. In **File > Build Profiles** (or **Build Settings** in older Unity versions), add `Assets/Member3/UI/MainMenuScene.unity` and place it before the showroom scene if the main menu should be the application entry point. Keep `ThreadHaven_Showroom` in the list so the Enter Showroom button can load it.

The main menu scene asset is currently a Canvas scaffold; the menu's visible controls must be present in that scene and referenced from `MainMenuButtons` for the click flow to be available in a build.
