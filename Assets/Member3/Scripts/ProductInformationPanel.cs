using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// References the product detail panel UI and provides its empty state.
/// Product-specific values will be supplied by the product selection flow.
/// </summary>
public class ProductInformationPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Product image")]
    [SerializeField] private Image productImage;
    [SerializeField] private GameObject imagePlaceholder;

    [Header("Product text")]
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private TMP_Text productIdText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text specificationsText;

    [Header("Empty state")]
    [SerializeField] private string emptyTitle = "Select a product";
    [SerializeField] private string emptyDescription = "Choose a product from the catalogue to view its details.";
    [SerializeField] private string emptySpecifications = "Product specifications will appear here.";

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (productImage != null)
            productImage.preserveAspect = true;

        ClearProduct();
    }

    /// <summary>Restores the panel's selection prompt.</summary>
    public void ClearProduct()
    {
        if (productNameText != null) productNameText.text = emptyTitle;
        if (categoryText != null) categoryText.text = "Category: —";
        if (productIdText != null) productIdText.text = "Product ID: —";
        if (descriptionText != null) descriptionText.text = emptyDescription;
        if (specificationsText != null) specificationsText.text = emptySpecifications;

        if (productImage != null)
        {
            productImage.sprite = null;
            productImage.enabled = false;
        }

        if (imagePlaceholder != null)
            imagePlaceholder.SetActive(true);
    }

    public void ShowPanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}
