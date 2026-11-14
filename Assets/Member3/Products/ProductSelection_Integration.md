# Commit 17 – World Product Selection Integration

`ProductCatalogue` listens to `ProductInteractor.OnProductSelected`. When a product in the 3D showroom is selected, the catalogue passes the selected scene `Product` to `ProductInformationPanel` and opens the panel. The existing `ProductInspector` listener remains independent, so Member 2's rotation and zoom interaction continues to receive the same selection event.

## Unity scene setup

1. Keep an enabled `ProductCatalogue` in the showroom scene and assign its `ProductInformationPanel` reference.
2. Ensure the player has the `ProductInteractor` component and its **Player / Interact** input action configured.
3. Add the Member 2 `Product` component to each selectable model and fill in its name, category, description, specifications, and optional price.
4. Ensure the information panel's text references are assigned. A selected 3D product uses its `Product` component data; its catalogue image is optional and the image placeholder is shown.

The event subscription is registered while `ProductCatalogue` is enabled and removed when it is disabled. UI catalogue card selection continues to use the JSON product records as before.
