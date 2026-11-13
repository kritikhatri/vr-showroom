using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls navigation between the home, category, and product catalogue panels.
/// Assign the panels and optional navigation buttons in the Inspector.
/// </summary>
public class ProductCatalogueNavigation : MonoBehaviour
{
    public static ProductCatalogueNavigation Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject categoryPanel;
    [SerializeField] private GameObject cataloguePanel;

    [Header("Navigation buttons")]
    [SerializeField] private Button categoryHomeButton;
    [SerializeField] private Button categoryViewProductsButton;
    [SerializeField] private Button catalogueHomeButton;
    [SerializeField] private Button catalogueBackButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("ProductCatalogueNavigation: More than one navigation controller is active.", this);
            enabled = false;
            return;
        }

        Instance = this;
        RegisterButtonListeners();
    }

    private void Start()
    {
        ShowHome();
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
        if (Instance == this)
            Instance = null;
    }

    public void ShowHome() => ShowPanel(homePanel, "home");

    public void ShowCategories() => ShowPanel(categoryPanel, "category selection");

    public void ShowCatalogue() => ShowPanel(cataloguePanel, "product catalogue");

    private void ShowPanel(GameObject panelToShow, string panelDescription)
    {
        if (panelToShow == null)
        {
            Debug.LogWarning($"ProductCatalogueNavigation: Assign the {panelDescription} panel in the Inspector.", this);
            return;
        }

        if (homePanel != null)
            homePanel.SetActive(panelToShow == homePanel);
        if (categoryPanel != null)
            categoryPanel.SetActive(panelToShow == categoryPanel);
        if (cataloguePanel != null)
            cataloguePanel.SetActive(panelToShow == cataloguePanel);
    }

    private void RegisterButtonListeners()
    {
        if (categoryHomeButton != null)
            categoryHomeButton.onClick.AddListener(ShowHome);
        if (categoryViewProductsButton != null)
            categoryViewProductsButton.onClick.AddListener(ShowCatalogue);
        if (catalogueHomeButton != null)
            catalogueHomeButton.onClick.AddListener(ShowHome);
        if (catalogueBackButton != null)
            catalogueBackButton.onClick.AddListener(ShowCategories);
    }

    private void RemoveButtonListeners()
    {
        if (categoryHomeButton != null)
            categoryHomeButton.onClick.RemoveListener(ShowHome);
        if (categoryViewProductsButton != null)
            categoryViewProductsButton.onClick.RemoveListener(ShowCatalogue);
        if (catalogueHomeButton != null)
            catalogueHomeButton.onClick.RemoveListener(ShowHome);
        if (catalogueBackButton != null)
            catalogueBackButton.onClick.RemoveListener(ShowCategories);
    }
}
