using UnityEngine;

/// <summary>
/// InspectModeUIBridge.cs  — Member 3 Integration Bridge
///
/// Listens to Member 2's ProductInspector events and drives Member 3's UI:
///   ProductInspector.OnInspectStarted  → shows the product information panel
///   ProductInspector.OnInspectEnded    → hides the product information panel
///
/// Place this component on any persistent GameObject in the Showroom scene
/// (e.g. the Player or a dedicated UIManager object).
/// No Inspector wiring is required as long as ProductCatalogue and
/// ProductInformationPanel singletons are active in the same scene.
/// </summary>
public class InspectModeUIBridge : MonoBehaviour
{
    // Optional: assign directly in Inspector to bypass singleton lookup
    [SerializeField] private ProductInformationPanel informationPanel;

    private void OnEnable()
    {
        ProductInspector.OnInspectStarted += HandleInspectStarted;
        ProductInspector.OnInspectEnded   += HandleInspectEnded;
    }

    private void OnDisable()
    {
        ProductInspector.OnInspectStarted -= HandleInspectStarted;
        ProductInspector.OnInspectEnded   -= HandleInspectEnded;
    }

    // Called by Member 2's ProductInspector when player selects a 3D product
    private void HandleInspectStarted(Product worldProduct)
    {
        if (worldProduct == null) return;

        ProductInformationPanel panel = ResolvePanel();
        if (panel == null)
        {
            Debug.LogWarning("InspectModeUIBridge: No ProductInformationPanel found in scene.");
            return;
        }

        // Display the world-space product data in Member 3's information panel
        panel.DisplayProduct(worldProduct);
        panel.ShowPanel();

        Debug.Log($"InspectModeUIBridge: Showing info panel for '{worldProduct.productName}'");
    }

    // Called by Member 2's ProductInspector when player exits inspect mode (Q / Esc)
    private void HandleInspectEnded()
    {
        ProductInformationPanel panel = ResolvePanel();
        if (panel != null)
            panel.HidePanel();

        Debug.Log("InspectModeUIBridge: Hiding info panel (inspect mode ended).");
    }

    // Resolves panel via Inspector slot first, then falls back to scene search
    private ProductInformationPanel ResolvePanel()
    {
        if (informationPanel != null) return informationPanel;
        informationPanel = FindObjectOfType<ProductInformationPanel>();
        return informationPanel;
    }
}
