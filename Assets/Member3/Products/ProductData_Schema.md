# Commit 12 – Product Data Structure

`Member3.Data.Product` models the UI catalogue records. Its fields match `sample-products.json`: product ID, name, category, description, specifications, and image path. `ProductSpecifications` holds material, fit, available sizes, and care details. `ProductCatalogueData` represents the JSON root object and its product array.

The `Member3.Data` namespace keeps this UI data type distinct from the scene component named `Product` in `Assets/Member2/Scripts/Product.cs`.

The types are compatible with Unity's `JsonUtility`, for example:

```csharp
Member3.Data.ProductCatalogueData data =
    JsonUtility.FromJson<Member3.Data.ProductCatalogueData>(jsonText);
```

`Product.GetSpecificationLines()` returns populated specifications as readable label/value lines for the information panel. Runtime loading and display are handled in later commits.
