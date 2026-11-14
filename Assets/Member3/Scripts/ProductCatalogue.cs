using System;
using System.Collections.Generic;
using Member3.Data;
using UnityEngine;

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

    private readonly Dictionary<string, Member3.Data.Product> productsById =
        new Dictionary<string, Member3.Data.Product>(StringComparer.OrdinalIgnoreCase);

    public event Action<CategorySelectionPanel.ProductCategory, int> ProductsFiltered;

    public CategorySelectionPanel.ProductCategory CurrentCategory { get; private set; }
    public int VisibleProductCount { get; private set; }

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
    }

    private void OnDisable()
    {
        ProductCard.OnProductCardSelected -= ShowProductInformation;
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
        VisibleProductCount = 0;

        for (int i = productCards.Count - 1; i >= 0; i--)
        {
            ProductCard card = productCards[i];
            if (card == null)
            {
                productCards.RemoveAt(i);
                continue;
            }

            bool shouldShow = category == CategorySelectionPanel.ProductCategory.All ||
                string.Equals(card.ProductCategory, category.ToString(), StringComparison.OrdinalIgnoreCase);

            card.gameObject.SetActive(shouldShow);
            if (shouldShow)
                VisibleProductCount++;
        }

        Debug.Log($"ProductCatalogue: Showing {VisibleProductCount} product(s) in {category}.");
        ProductsFiltered?.Invoke(category, VisibleProductCount);
    }

    /// <summary>Registers a card created after the catalogue has initialized.</summary>
    public void RegisterCard(ProductCard card)
    {
        if (card == null || productCards.Contains(card))
            return;

        productCards.Add(card);
        bool shouldShow = CurrentCategory == CategorySelectionPanel.ProductCategory.All ||
            string.Equals(card.ProductCategory, CurrentCategory.ToString(), StringComparison.OrdinalIgnoreCase);
        card.gameObject.SetActive(shouldShow);
        if (shouldShow)
            VisibleProductCount++;
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
                productsById.Add(product.Id, product);
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
}
