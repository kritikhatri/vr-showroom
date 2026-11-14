using System;
using System.Collections.Generic;
using Member3.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Filters product cards and connects card selection to product information.
/// Place this component on a parent of the catalogue cards, or assign the
/// cards explicitly in the Inspector.
/// </summary>
public class ProductCatalogue : MonoBehaviour
{
    public static ProductCatalogue Instance { get; private set; }

    [Header("Product cards")]
    [Tooltip("Leave empty to collect ProductCard components under this object.")]
    [SerializeField] private List<ProductCard> productCards = new List<ProductCard>();

    [Header("Product information")]
    [SerializeField] private TextAsset productDataJson;
    [SerializeField] private ProductInformationPanel informationPanel;

    [Header("Product search")]
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Button clearSearchButton;
    [SerializeField] private TMP_Text resultCountText;
    [SerializeField] private GameObject noResultsMessage;

    private readonly Dictionary<string, Member3.Data.Product> productsById =
        new Dictionary<string, Member3.Data.Product>(StringComparer.OrdinalIgnoreCase);
    private readonly List<Member3.Data.Product> productRecords = new List<Member3.Data.Product>();
    private readonly List<Member3.Data.Product> filteredProductRecords = new List<Member3.Data.Product>();

    public event Action<CategorySelectionPanel.ProductCategory, int> ProductsFiltered;

    public CategorySelectionPanel.ProductCategory CurrentCategory { get; private set; }
    public int VisibleProductCount { get; private set; }
    public string CurrentSearchQuery { get; private set; } = string.Empty;
    public IReadOnlyList<Member3.Data.Product> FilteredProducts => filteredProductRecords;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("ProductCatalogue: More than one catalogue controller is active.", this);
            enabled = false;
            return;
        }

        Instance = this;
        CollectChildCards();
        LoadProductData();
        CurrentCategory = CategorySelectionPanel.ProductCategory.All;
        FilterByCategory(CurrentCategory);
    }

    private void OnEnable()
    {
        ProductCard.OnProductCardSelected += ShowProductInformation;
        ProductInteractor.OnProductSelected += ShowWorldProductInformation;
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(SearchByName);
        if (clearSearchButton != null)
            clearSearchButton.onClick.AddListener(ClearSearch);
    }

    private void OnDisable()
    {
        ProductCard.OnProductCardSelected -= ShowProductInformation;
        ProductInteractor.OnProductSelected -= ShowWorldProductInformation;
        if (searchInput != null)
            searchInput.onValueChanged.RemoveListener(SearchByName);
        if (clearSearchButton != null)
            clearSearchButton.onClick.RemoveListener(ClearSearch);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Filters the registered cards by category. All shows every card.</summary>
    public void FilterByCategory(CategorySelectionPanel.ProductCategory category)
    {
        CurrentCategory = category;
        ApplyFilters();

        Debug.Log($"ProductCatalogue: Showing {VisibleProductCount} product(s) in {category}.");
        ProductsFiltered?.Invoke(category, VisibleProductCount);
    }

    /// <summary>Filters the current category by a case-insensitive product name query.</summary>
    public void SearchByName(string query)
    {
        CurrentSearchQuery = string.IsNullOrWhiteSpace(query) ? string.Empty : query.Trim();
        ApplyFilters();
        ProductsFiltered?.Invoke(CurrentCategory, VisibleProductCount);
    }

    /// <summary>Clears the search field and restores the current category results.</summary>
    public void ClearSearch()
    {
        if (searchInput != null)
            searchInput.SetTextWithoutNotify(string.Empty);

        SearchByName(string.Empty);
    }

    private void ApplyFilters()
    {
        filteredProductRecords.Clear();
        foreach (Member3.Data.Product product in productRecords)
        {
            if (MatchesCategory(product.Category) && MatchesSearch(product.Name))
                filteredProductRecords.Add(product);
        }

        VisibleProductCount = 0;

        for (int i = productCards.Count - 1; i >= 0; i--)
        {
            ProductCard card = productCards[i];
            if (card == null)
            {
                productCards.RemoveAt(i);
                continue;
            }

            string category = card.ProductCategory;
            string productName = card.ProductName;
            if (productsById.TryGetValue(card.ProductId, out Member3.Data.Product product) &&
                product != null)
            {
                if (!string.IsNullOrWhiteSpace(product.Category))
                    category = product.Category;
                if (!string.IsNullOrWhiteSpace(product.Name))
                    productName = product.Name;
            }

            bool shouldShow = MatchesCategory(category) && MatchesSearch(productName);

            card.gameObject.SetActive(shouldShow);
            if (shouldShow)
                VisibleProductCount++;
        }

        if (resultCountText != null)
            resultCountText.text = VisibleProductCount == 1 ? "1 product" : $"{VisibleProductCount} products";
        if (noResultsMessage != null)
            noResultsMessage.SetActive(VisibleProductCount == 0);
    }

    private bool MatchesCategory(string category)
    {
        return CurrentCategory == CategorySelectionPanel.ProductCategory.All ||
            string.Equals(category, CurrentCategory.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesSearch(string productName)
    {
        return string.IsNullOrEmpty(CurrentSearchQuery) ||
            (!string.IsNullOrEmpty(productName) &&
             productName.IndexOf(CurrentSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>Registers a card created after the catalogue has initialized.</summary>
    public void RegisterCard(ProductCard card)
    {
        if (card == null || productCards.Contains(card))
            return;

        productCards.Add(card);
        ApplyFilters();
    }

    private void CollectChildCards()
    {
        ProductCard[] childCards = GetComponentsInChildren<ProductCard>(true);
        foreach (ProductCard card in childCards)
        {
            if (card != null && !productCards.Contains(card))
                productCards.Add(card);
        }
    }

    private void LoadProductData()
    {
        productsById.Clear();
        productRecords.Clear();
        filteredProductRecords.Clear();

        if (productDataJson == null)
        {
            Debug.LogWarning("ProductCatalogue: Assign the sample product JSON TextAsset in the Inspector.", this);
            return;
        }

        ProductCatalogueData catalogueData;
        try
        {
            catalogueData = JsonUtility.FromJson<ProductCatalogueData>(productDataJson.text);
        }
        catch (Exception exception)
        {
            Debug.LogError($"ProductCatalogue: Could not parse product JSON. {exception.Message}", this);
            return;
        }
        if (catalogueData == null || catalogueData.products == null)
        {
            Debug.LogError("ProductCatalogue: The assigned product JSON has no products array.", this);
            return;
        }

        foreach (Member3.Data.Product product in catalogueData.products)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.Id))
                continue;

            if (!productsById.ContainsKey(product.Id))
            {
                productsById.Add(product.Id, product);
                productRecords.Add(product);
            }
            else
                Debug.LogWarning($"ProductCatalogue: Duplicate product ID '{product.Id}' was ignored.", this);
        }

        Debug.Log($"ProductCatalogue: Loaded {productsById.Count} product record(s).", this);
    }

    private void ShowProductInformation(string productId)
    {
        if (informationPanel == null)
        {
            Debug.LogWarning("ProductCatalogue: Assign a ProductInformationPanel in the Inspector.", this);
            return;
        }

        if (!productsById.TryGetValue(productId, out Member3.Data.Product product))
        {
            Debug.LogWarning($"ProductCatalogue: No product data found for ID '{productId}'.", this);
            return;
        }

        Sprite image = null;
        if (!string.IsNullOrWhiteSpace(product.ImagePath))
        {
            image = Resources.Load<Sprite>(product.ImagePath);
            if (image == null)
                Debug.LogWarning($"ProductCatalogue: Could not load image '{product.ImagePath}' from a Resources folder.", this);
        }

        informationPanel.DisplayProduct(product, image);
        informationPanel.ShowPanel();
    }

    /// <summary>Receives a product selected in the 3D showroom by Member 2.</summary>
    private void ShowWorldProductInformation(global::Product selectedProduct)
    {
        if (selectedProduct == null)
            return;

        if (informationPanel == null)
        {
            Debug.LogWarning("ProductCatalogue: Assign a ProductInformationPanel to show selected showroom products.", this);
            return;
        }

        informationPanel.DisplayProduct(selectedProduct);
        informationPanel.ShowPanel();
    }
}
