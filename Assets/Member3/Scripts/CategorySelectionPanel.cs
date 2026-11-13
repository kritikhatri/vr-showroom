using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CategorySelectionPanel.cs
/// Manages the product category selection screen.
/// Categories: Shirts, Jackets, Trousers, Dresses
/// When a category button is clicked, it filters and shows relevant products.
/// Attach to the CategoryPanel GameObject in the scene.
/// </summary>
public class CategorySelectionPanel : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // CATEGORY ENUM  (defines all available product categories)
    // ─────────────────────────────────────────────────────────────────────────

    // An enum is like a list of named constants — similar to C++ enum
    public enum ProductCategory
    {
        All,        // show every product
        Shirts,
        Jackets,
        Trousers,
        Dresses
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("--- Panel Reference ---")]
    // The root panel GameObject that contains all category buttons
    [SerializeField] private GameObject categoryPanel;

    [Header("--- Category Buttons ---")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button shirtsButton;
    [SerializeField] private Button jacketsButton;
    [SerializeField] private Button trousersButton;
    [SerializeField] private Button dressesButton;

    [Header("--- Button Labels ---")]
    [SerializeField] private TMP_Text allLabel;
    [SerializeField] private TMP_Text shirtsLabel;
    [SerializeField] private TMP_Text jacketsLabel;
    [SerializeField] private TMP_Text trousersLabel;
    [SerializeField] private TMP_Text dressesLabel;

    [Header("--- Panel Title ---")]
    [SerializeField] private TMP_Text panelTitleText;
    [SerializeField] private string   panelTitle = "Select a Category";

    [Header("--- Colours ---")]
    // Colour when a category is selected (active)
    [SerializeField] private Color activeColor   = new Color(0.2f, 0.6f, 1f, 1f);  // blue
    // Colour when a category is not selected (inactive)
    [SerializeField] private Color inactiveColor = new Color(0.2f, 0.2f, 0.3f, 1f); // dark grey

    [Header("--- Category Icons (optional) ---")]
    [SerializeField] private Image shirtsIcon;
    [SerializeField] private Image jacketsIcon;
    [SerializeField] private Image trousersIcon;
    [SerializeField] private Image dressesIcon;

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────────────────────────────────

    // Tracks which category is currently selected
    private ProductCategory currentCategory = ProductCategory.All;

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        SetupPanelTitle();
        SetupButtonLabels();
        RegisterButtonListeners();
        // Start with "All" selected by default
        SelectCategory(ProductCategory.All);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SETUP METHODS
    // ─────────────────────────────────────────────────────────────────────────

    private void SetupPanelTitle()
    {
        if (panelTitleText != null)
        {
            panelTitleText.text      = panelTitle;
            panelTitleText.fontSize  = 40f;
            panelTitleText.fontStyle = FontStyles.Bold;
            panelTitleText.color     = Color.white;
            panelTitleText.alignment = TextAlignmentOptions.Center;
        }
    }

    private void SetupButtonLabels()
    {
        // Set the text on each category button
        if (allLabel      != null) allLabel.text      = "All";
        if (shirtsLabel   != null) shirtsLabel.text   = "Shirts";
        if (jacketsLabel  != null) jacketsLabel.text  = "Jackets";
        if (trousersLabel != null) trousersLabel.text = "Trousers";
        if (dressesLabel  != null) dressesLabel.text  = "Dresses";
    }

    private void RegisterButtonListeners()
    {
        // Connect each button to its handler using a lambda (short inline function)
        if (allButton      != null) allButton.onClick.AddListener(()      => SelectCategory(ProductCategory.All));
        if (shirtsButton   != null) shirtsButton.onClick.AddListener(()   => SelectCategory(ProductCategory.Shirts));
        if (jacketsButton  != null) jacketsButton.onClick.AddListener(()  => SelectCategory(ProductCategory.Jackets));
        if (trousersButton != null) trousersButton.onClick.AddListener(() => SelectCategory(ProductCategory.Trousers));
        if (dressesButton  != null) dressesButton.onClick.AddListener(() => SelectCategory(ProductCategory.Dresses));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CATEGORY SELECTION
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Selects a category, highlights its button and fires an event
    /// so the product list can update accordingly.
    /// </summary>
    public void SelectCategory(ProductCategory category)
    {
        currentCategory = category;

        Debug.Log("CategorySelectionPanel: Selected category → " + category.ToString());

        // Update button highlight colours
        UpdateButtonHighlights();

        // Notify the product catalogue to filter by this category
        // ProductCatalogue applies this selection to the registered product cards.
        if (ProductCatalogue.Instance != null)
            ProductCatalogue.Instance.FilterByCategory(category);
    }

    // Makes the selected button bright and all others dim
    private void UpdateButtonHighlights()
    {
        // Set every button to inactive colour first
        SetButtonColor(allButton,      inactiveColor);
        SetButtonColor(shirtsButton,   inactiveColor);
        SetButtonColor(jacketsButton,  inactiveColor);
        SetButtonColor(trousersButton, inactiveColor);
        SetButtonColor(dressesButton,  inactiveColor);

        // Then highlight only the active one
        switch (currentCategory)
        {
            case ProductCategory.All:      SetButtonColor(allButton,      activeColor); break;
            case ProductCategory.Shirts:   SetButtonColor(shirtsButton,   activeColor); break;
            case ProductCategory.Jackets:  SetButtonColor(jacketsButton,  activeColor); break;
            case ProductCategory.Trousers: SetButtonColor(trousersButton, activeColor); break;
            case ProductCategory.Dresses:  SetButtonColor(dressesButton,  activeColor); break;
        }
    }

    // Helper: applies a colour to a button's Image component
    private void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Show this panel.</summary>
    public void Show()
    {
        if (categoryPanel != null) categoryPanel.SetActive(true);
    }

    /// <summary>Hide this panel.</summary>
    public void Hide()
    {
        if (categoryPanel != null) categoryPanel.SetActive(false);
    }

    /// <summary>Returns the currently selected category.</summary>
    public ProductCategory GetCurrentCategory() => currentCategory;
}
