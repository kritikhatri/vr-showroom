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

        string[] specificationLines = product.GetSpecificationLines();
        SetProductContent(product.Name, product.Category, product.Id, product.Description,
            string.Join("\n", specificationLines), image);
    }

    /// <summary>Displays a scene product selected through Member 2's world interaction.</summary>
    public void DisplayProduct(global::Product product)
    {
        if (product == null)
        {
            ClearProduct();
            return;
        }

        string specifications = product.specifications ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(product.price))
        {
            string priceLine = "Price: " + product.price.Trim();
            specifications = string.IsNullOrWhiteSpace(specifications)
                ? priceLine
                : priceLine + "\n" + specifications;
        }

        SetProductContent(product.productName, product.category, string.Empty,
            product.description, specifications, null);
    }

    private void SetProductContent(string productName, string category, string productId,
        string description, string specifications, Sprite image)
    {
        hasProduct = true;

        if (productNameText != null)
            productNameText.text = string.IsNullOrWhiteSpace(productName) ? "Untitled product" : productName;
        if (categoryText != null)
            categoryText.text = "Category: " + (string.IsNullOrWhiteSpace(category) ? "—" : category);
        if (productIdText != null)
            productIdText.text = "Product ID: " + (string.IsNullOrWhiteSpace(productId) ? "—" : productId);
        if (descriptionText != null)
            descriptionText.text = string.IsNullOrWhiteSpace(description) ? "No description available." : description;
        if (specificationsText != null)
            specificationsText.text = string.IsNullOrWhiteSpace(specifications)
                ? "No specifications available."
                : specifications;

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
