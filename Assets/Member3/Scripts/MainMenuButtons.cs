using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;   // needed to load different scenes
using TMPro;

/// <summary>
/// MainMenuButtons.cs
/// Handles all three main menu buttons:
///   1. Enter Showroom  → loads the 3D showroom scene (Member 1's scene)
///   2. Explore Products → opens the product catalogue panel
///   3. Exit            → quits the application
/// Attach this script to the ButtonController GameObject in the MainMenu scene.
/// </summary>
public class MainMenuButtons : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("--- Buttons ---")]
    // Drag each Button from the Hierarchy into these slots in the Inspector
    [SerializeField] private Button enterShowroomButton;
    [SerializeField] private Button exploreProductsButton;
    [SerializeField] private Button exitButton;

    [Header("--- Button Labels ---")]
    // TextMeshPro text components inside each button
    [SerializeField] private TMP_Text enterShowroomLabel;
    [SerializeField] private TMP_Text exploreProductsLabel;
    [SerializeField] private TMP_Text exitLabel;

    [Header("--- Button Colours ---")]
    // Normal (idle) colour for each button
    [SerializeField] private Color enterButtonColor   = new Color(0.2f, 0.6f, 1f, 1f);  // blue
    [SerializeField] private Color exploreButtonColor = new Color(0.1f, 0.7f, 0.4f, 1f); // green
    [SerializeField] private Color exitButtonColor    = new Color(0.8f, 0.2f, 0.2f, 1f); // red

    [Header("--- Scene Names ---")]
    // Name of the showroom scene built by Member 1 (must match Build Settings)
    [SerializeField] private string showroomSceneName    = "ThreadHaven_Showroom";
    // Name of the product catalogue scene / panel
    [SerializeField] private string productCatalogueScene = "ProductCatalogue";

    [Header("--- Panels ---")]
    // If using panels instead of scenes, assign the product panel here
    [SerializeField] private GameObject productCataloguePanel;
    [SerializeField] private GameObject mainMenuPanel;

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        SetupButtonLabels();
        SetupButtonColours();
        RegisterButtonListeners();   // connect buttons to their functions
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SETUP METHODS
    // ─────────────────────────────────────────────────────────────────────────

    // Sets the text displayed on each button
    private void SetupButtonLabels()
    {
        if (enterShowroomLabel   != null) enterShowroomLabel.text   = "Enter Showroom";
        if (exploreProductsLabel != null) exploreProductsLabel.text = "Explore Products";
        if (exitLabel            != null) exitLabel.text            = "Exit";
    }

    // Sets the background colour of each button's Image component
    private void SetupButtonColours()
    {
        SetButtonColor(enterShowroomButton,   enterButtonColor);
        SetButtonColor(exploreProductsButton, exploreButtonColor);
        SetButtonColor(exitButton,            exitButtonColor);
    }

    // Applies a colour to a button's background image
    private void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();  // get the Image on the button
        if (img != null) img.color = color;
    }

    // Connects each button's onClick event to the correct method
    private void RegisterButtonListeners()
    {
        // AddListener is like binding a function to a click event
        if (enterShowroomButton   != null)
            enterShowroomButton.onClick.AddListener(OnEnterShowroomClicked);

        if (exploreProductsButton != null)
            exploreProductsButton.onClick.AddListener(OnExploreProductsClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BUTTON CLICK HANDLERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called when "Enter Showroom" is clicked.
    /// Loads Member 1's 3D showroom scene.
    /// </summary>
    public void OnEnterShowroomClicked()
    {
        Debug.Log("MainMenuButtons: Entering Showroom...");

        // Check if the scene exists in Build Settings before loading
        if (Application.CanStreamedLevelBeLoaded(showroomSceneName))
        {
            SceneManager.LoadScene(showroomSceneName);
        }
        else
        {
            // If scene not yet set up, show product catalogue as fallback
            Debug.LogWarning("ShowroomScene not found in Build Settings. Opening catalogue.");
            OnExploreProductsClicked();
        }
    }

    /// <summary>
    /// Called when "Explore Products" is clicked.
    /// Shows the product catalogue panel.
    /// </summary>
    public void OnExploreProductsClicked()
    {
        Debug.Log("MainMenuButtons: Opening Product Catalogue...");

        // Use the shared navigation flow when it is present in the scene.
        if (ProductCatalogueNavigation.Instance != null)
        {
            ProductCatalogueNavigation.Instance.ShowCategories();
            return;
        }

        // If we have a panel reference, show it and hide main menu
        if (productCataloguePanel != null)
        {
            productCataloguePanel.SetActive(true);   // show catalogue
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);       // hide main menu
        }
        else
        {
            // Otherwise load a separate scene
            SceneManager.LoadScene(productCatalogueScene);
        }
    }

    /// <summary>
    /// Called when "Exit" is clicked.
    /// Quits the application (works in built game, not in Editor).
    /// </summary>
    public void OnExitClicked()
    {
        Debug.Log("MainMenuButtons: Exiting application...");

#if UNITY_EDITOR
        // Stop play mode if running inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HOVER EFFECTS  (visual feedback when mouse is over a button)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Slightly brightens the button when hovered.</summary>
    public void OnButtonHoverEnter(Button button)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            Color baseColor = GetButtonBaseColor(button);
            img.color = Color.Lerp(baseColor, Color.white, 0.2f);
        }
    }

    /// <summary>Restores the button colour when mouse leaves.</summary>
    public void OnButtonHoverExit(Button button)
    {
        if (button == null) return;
        SetButtonColor(button, GetButtonBaseColor(button));
    }

    private Color GetButtonBaseColor(Button button)
    {
        if (button == enterShowroomButton) return enterButtonColor;
        if (button == exploreProductsButton) return exploreButtonColor;
        if (button == exitButton) return exitButtonColor;
        return Color.white;
    }
}
