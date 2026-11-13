using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows only the product cards that match the selected category.
/// Place this component on a parent of the catalogue cards, or assign the
/// cards explicitly in the Inspector.
/// </summary>
public class ProductCatalogue : MonoBehaviour
{
    public static ProductCatalogue Instance { get; private set; }

    [Header("Product cards")]
    [Tooltip("Leave empty to collect ProductCard components under this object.")]
    [SerializeField] private List<ProductCard> productCards = new List<ProductCard>();

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
        CurrentCategory = CategorySelectionPanel.ProductCategory.All;
        FilterByCategory(CurrentCategory);
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
}
