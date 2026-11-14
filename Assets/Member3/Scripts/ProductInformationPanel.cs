using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays selected product details and provides the panel empty state.</summary>
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

    [Header("Navigation controls")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button closeButton;

    [Header("Empty state")]
    [SerializeField] private string emptyTitle = "Select a product";
    [SerializeField] private string emptyDescription = "Choose a product from the catalogue to view its details.";
    [SerializeField] private string emptySpecifications = "Product specifications will appear here.";

    private bool hasProduct;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (productImage != null)
            productImage.preserveAspect = true;

        RegisterButtonListeners();
        if (!hasProduct)
            ClearProduct();
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    /// <summary>Populates the panel with the selected product record.</summary>
    public void DisplayProduct(Member3.Data.Product product, Sprite image)
    {
        if (product == null)
        {
            ClearProduct();
            return;
        }

        hasProduct = true;

        if (productNameText != null)
            productNameText.text = string.IsNullOrWhiteSpace(product.Name) ? "Untitled product" : product.Name;
        if (categoryText != null)
            categoryText.text = "Category: " + (string.IsNullOrWhiteSpace(product.Category) ? "—" : product.Category);
        if (productIdText != null)
            productIdText.text = "Product ID: " + (string.IsNullOrWhiteSpace(product.Id) ? "—" : product.Id);
        if (descriptionText != null)
            descriptionText.text = string.IsNullOrWhiteSpace(product.Description) ? "No description available." : product.Description;

        string[] specificationLines = product.GetSpecificationLines();
        if (specificationsText != null)
            specificationsText.text = specificationLines.Length == 0
                ? "No specifications available."
                : string.Join("\n", specificationLines);

        if (productImage != null)
        {
            productImage.sprite = image;
            productImage.enabled = image != null;
            productImage.preserveAspect = true;
        }

        if (imagePlaceholder != null)
            imagePlaceholder.SetActive(image == null);
    }

    /// <summary>Restores the panel's selection prompt.</summary>
    public void ClearProduct()
    {
        hasProduct = false;
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
        GameObject root = panelRoot != null ? panelRoot : gameObject;
        root.SetActive(true);
    }

    public void HidePanel()
    {
        GameObject root = panelRoot != null ? panelRoot : gameObject;
        root.SetActive(false);
    }

    private void RegisterButtonListeners()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
        if (homeButton != null)
            homeButton.onClick.AddListener(GoHome);
        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);
    }

    private void RemoveButtonListeners()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(GoBack);
        if (homeButton != null)
            homeButton.onClick.RemoveListener(GoHome);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HidePanel);
    }

    private void GoBack()
    {
        HidePanel();
        if (ProductCatalogueNavigation.Instance != null)
            ProductCatalogueNavigation.Instance.ShowCatalogue();
    }

    private void GoHome()
    {
        HidePanel();
        if (ProductCatalogueNavigation.Instance != null)
            ProductCatalogueNavigation.Instance.ShowHome();
    }
}
