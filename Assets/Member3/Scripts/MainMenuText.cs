using UnityEngine;
using UnityEngine.UI;
using TMPro;                  // TextMeshPro – best text system for Unity UI

/// <summary>
/// MainMenuText.cs
/// Manages all text elements on the Main Menu screen.
/// Handles: project title, welcome message and introductory description text.
/// Attach this script to the TextController GameObject in the MainMenu scene.
/// </summary>
public class MainMenuText : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("--- Project Title ---")]
    // The big heading at the top  e.g. "VR GARMENT SHOWROOM"
    [SerializeField] private TMP_Text projectTitleText;
    [SerializeField] private string   projectTitle    = "VR GARMENT SHOWROOM";
    [SerializeField] private float    titleFontSize   = 72f;
    [SerializeField] private Color    titleColor      = new Color(0.2f, 0.6f, 1f, 1f); // bright blue

    [Header("--- Welcome Message ---")]
    // Sub-heading below the title  e.g. "Welcome to the Virtual Showroom"
    [SerializeField] private TMP_Text welcomeText;
    [SerializeField] private string   welcomeMessage  = "Welcome to the Virtual Showroom";
    [SerializeField] private float    welcomeFontSize = 36f;
    [SerializeField] private Color    welcomeColor    = Color.white;

    [Header("--- Intro Description ---")]
    // Short paragraph describing the experience to the user
    [SerializeField] private TMP_Text introText;
    [SerializeField] [TextArea(3, 6)]  // allows multi-line editing in Inspector
    private string introDescription =
        "Explore our exclusive collection of garments in an immersive " +
        "Virtual Reality environment.\n" +
        "Browse Shirts, Jackets, Trousers and Dresses — all in one place.";
    [SerializeField] private float introFontSize = 22f;
    [SerializeField] private Color introColor    = new Color(0.8f, 0.8f, 0.8f, 1f); // light grey

    [Header("--- Version Info ---")]
    // Small version label at the bottom corner
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private string   versionString = "v1.0  |  MSME Major Project 2023";
    [SerializeField] private float    versionFontSize = 14f;
    [SerializeField] private Color    versionColor    = new Color(0.5f, 0.5f, 0.5f, 1f); // grey

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    // Start() runs once just before the first frame is drawn
    private void Start()
    {
        SetupTitleText();
        SetupWelcomeText();
        SetupIntroText();
        SetupVersionText();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE SETUP METHODS
    // ─────────────────────────────────────────────────────────────────────────

    // Sets the large project title at the top of the screen
    private void SetupTitleText()
    {
        if (projectTitleText == null)
        {
            Debug.LogWarning("MainMenuText: projectTitleText not assigned!");
            return;
        }

        projectTitleText.text      = projectTitle;    // set the string
        projectTitleText.fontSize  = titleFontSize;   // set size
        projectTitleText.color     = titleColor;      // set colour
        projectTitleText.fontStyle = FontStyles.Bold; // make it bold
        projectTitleText.alignment = TextAlignmentOptions.Center; // centre align
    }

    // Sets the welcome sub-heading below the title
    private void SetupWelcomeText()
    {
        if (welcomeText == null) return;

        welcomeText.text      = welcomeMessage;
        welcomeText.fontSize  = welcomeFontSize;
        welcomeText.color     = welcomeColor;
        welcomeText.fontStyle = FontStyles.Italic; // italic for a softer feel
        welcomeText.alignment = TextAlignmentOptions.Center;
    }

    // Sets the introductory paragraph describing the VR experience
    private void SetupIntroText()
    {
        if (introText == null) return;

        introText.text      = introDescription;
        introText.fontSize  = introFontSize;
        introText.color     = introColor;
        introText.alignment = TextAlignmentOptions.Center;
        // Allow text to wrap across multiple lines
        introText.enableWordWrapping = true;
    }

    // Sets the small version label at the bottom of the screen
    private void SetupVersionText()
    {
        if (versionText == null) return;

        versionText.text     = versionString;
        versionText.fontSize = versionFontSize;
        versionText.color    = versionColor;
        versionText.alignment = TextAlignmentOptions.BottomRight;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC METHODS  (called by other scripts if needed)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Update the title text at runtime.</summary>
    public void SetTitle(string newTitle)
    {
        projectTitle = newTitle;
        if (projectTitleText != null)
            projectTitleText.text = newTitle;
    }

    /// <summary>Update the welcome message at runtime.</summary>
    public void SetWelcomeMessage(string message)
    {
        welcomeMessage = message;
        if (welcomeText != null)
            welcomeText.text = message;
    }

#if UNITY_EDITOR
    // Preview text changes live in the Editor
    private void OnValidate()
    {
        if (projectTitleText != null) projectTitleText.text = projectTitle;
        if (welcomeText      != null) welcomeText.text      = welcomeMessage;
        if (introText        != null) introText.text        = introDescription;
        if (versionText      != null) versionText.text      = versionString;
    }
#endif
}
