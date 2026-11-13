using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// ProductCard.cs
/// Reusable UI component representing a single product item card in the showroom catalog.
/// Displays the product thumbnail, item title, category badge, and handles selection clicks.
/// Attach this script to the ProductCard prefab root GameObject.
/// </summary>
public class ProductCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR REFERENCES
    // ─────────────────────────────────────────────────────────────────────────

    [Header("--- UI Elements ---")]
    [Tooltip("Image component displaying the product thumbnail")]
    [SerializeField] private Image productImage;

    [Tooltip("Text component displaying the product title/name")]
    [SerializeField] private TMP_Text productNameText;

    [Tooltip("Text component displaying category or brief specification badge")]
    [SerializeField] private TMP_Text categoryBadgeText;

    [Tooltip("Text component displaying product price or SKU")]
    [SerializeField] private TMP_Text priceOrSkuText;

    [Tooltip("Main clickable button representing the card")]
    [SerializeField] private Button cardButton;

    [Header("--- Visual Styling & Feedback ---")]
    [Tooltip("Card background frame or container image")]
    [SerializeField] private Image cardBackground;

    [SerializeField] private Color defaultCardColor = new Color(0.12f, 0.14f, 0.22f, 0.95f);
    [SerializeField] private Color hoverCardColor   = new Color(0.18f, 0.22f, 0.35f, 1f);
    [SerializeField] private Color selectedCardColor= new Color(0.15f, 0.45f, 0.85f, 1f);

    [Header("--- Product Data Binding ---")]
    [SerializeField] private string productId;
    [SerializeField] private string productName;
    [SerializeField] private string productCategory;

    // Action/callback triggered when this card is clicked
    public static event Action<string> OnProductCardSelected;

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(HandleCardClick);
        }

        ResetVisualState();
    }

    private void OnDestroy()
    {
        if (cardButton != null)
        {
            cardButton.onClick.RemoveListener(HandleCardClick);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC SETUP METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Configures the card with dynamic product information.
    /// </summary>
    public void SetupCard(string id, string name, string category, Sprite imageSprite, string priceOrTag = "")
    {
        productId       = id;
        productName     = name;
        productCategory = category;

        if (productNameText != null)
            productNameText.text = name;

        if (categoryBadgeText != null)
            categoryBadgeText.text = category.ToUpper();

        if (productImage != null && imageSprite != null)
        {
            productImage.sprite = imageSprite;
            productImage.preserveAspect = true;
        }

        if (priceOrSkuText != null)
        {
            priceOrSkuText.text = string.IsNullOrEmpty(priceOrTag) ? $"ID: {id}" : priceOrTag;
        }

        ResetVisualState();
    }

    /// <summary>
    /// Highlights or unhighlights the card when selected in the catalogue.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (cardBackground != null)
        {
            cardBackground.color = isSelected ? selectedCardColor : defaultCardColor;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EVENT & INTERACTION HANDLERS
    // ─────────────────────────────────────────────────────────────────────────

    private void HandleCardClick()
    {
        Debug.Log($"ProductCard: Selected item [{productId}] - {productName}");
        SetSelected(true);
        OnProductCardSelected?.Invoke(productId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardBackground != null)
        {
            cardBackground.color = hoverCardColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetVisualState();
    }

    private void ResetVisualState()
    {
        if (cardBackground != null)
        {
            cardBackground.color = defaultCardColor;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GETTERS
    // ─────────────────────────────────────────────────────────────────────────
    public string ProductId => productId;
    public string ProductName => productName;
    public string ProductCategory => productCategory;
}
