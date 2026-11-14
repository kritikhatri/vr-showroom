using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// ProductCard.cs
/// Reusable UI component for a single product card in the catalogue grid.
/// Displays thumbnail, product name, and category badge.
/// Fires OnProductCardSelected(ProductCard) when clicked so ProductCatalogue
/// can deselect other cards and open the information panel.
/// </summary>
public class ProductCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("--- UI Elements ---")]
    [SerializeField] private Image    productImage;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text categoryBadgeText;
    [SerializeField] private TMP_Text priceOrSkuText;
    [SerializeField] private Button   cardButton;

    [Header("--- Visual Styling ---")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Color defaultCardColor  = new Color(0.12f, 0.14f, 0.22f, 0.95f);
    [SerializeField] private Color hoverCardColor    = new Color(0.18f, 0.22f, 0.35f, 1f);
    [SerializeField] private Color selectedCardColor = new Color(0.15f, 0.45f, 0.85f, 1f);

    [Header("--- Product Data ---")]
    [SerializeField] private string productId;
    [SerializeField] private string productName;
    [SerializeField] private string productCategory;

    // ─────────────────────────────────────────────────────────────────────────
    // EVENT  — passes the entire card so ProductCatalogue can compare references
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired when a card is clicked.
    /// ProductCatalogue.HandleProductCardSelected subscribes to this.
    /// </summary>
    public static event Action<ProductCard> OnProductCardSelected;

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (cardButton != null)
            cardButton.onClick.AddListener(HandleCardClick);

        ResetVisualState();
    }

    private void OnDestroy()
    {
        if (cardButton != null)
            cardButton.onClick.RemoveListener(HandleCardClick);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC SETUP
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Populates the card with product data at runtime.</summary>
    public void SetupCard(string id, string name, string category,
                          Sprite imageSprite, string priceOrTag = "")
    {
        productId       = id;
        productName     = name;
        productCategory = category;

        if (productNameText     != null) productNameText.text     = name;
        if (categoryBadgeText   != null) categoryBadgeText.text   = category.ToUpper();
        if (priceOrSkuText      != null)
            priceOrSkuText.text = string.IsNullOrEmpty(priceOrTag) ? $"ID: {id}" : priceOrTag;

        if (productImage != null && imageSprite != null)
        {
            productImage.sprite          = imageSprite;
            productImage.preserveAspect  = true;
        }

        ResetVisualState();
    }

    /// <summary>Highlights or un-highlights the card border.</summary>
    public void SetSelected(bool isSelected)
    {
        if (cardBackground != null)
            cardBackground.color = isSelected ? selectedCardColor : defaultCardColor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INTERACTION
    // ─────────────────────────────────────────────────────────────────────────

    private void HandleCardClick()
    {
        Debug.Log($"ProductCard: clicked [{productId}] {productName}");
        SetSelected(true);
        OnProductCardSelected?.Invoke(this);   // pass the whole card — ProductCatalogue uses ProductId
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardBackground != null)
            cardBackground.color = hoverCardColor;
    }

    public void OnPointerExit(PointerEventData eventData) => ResetVisualState();

    private void ResetVisualState()
    {
        if (cardBackground != null)
            cardBackground.color = defaultCardColor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GETTERS
    // ─────────────────────────────────────────────────────────────────────────

    public string ProductId       => productId;
    public string ProductName     => productName;
    public string ProductCategory => productCategory;
}
