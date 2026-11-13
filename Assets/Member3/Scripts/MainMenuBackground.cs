using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenuBackground.cs
/// Controls the visual styling of the Main Menu scene.
/// Handles: background panel colour, project logo, gradient overlay and accent line.
/// Attach this script to the BackgroundController GameObject in the MainMenu scene.
/// </summary>
public class MainMenuBackground : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS  (set these inside Unity Editor)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("--- Background Panel ---")]
    // The full-screen dark background image
    [SerializeField] private Image backgroundPanel;
    // Dark navy colour for the background
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.15f, 1f);

    [Header("--- Gradient Overlay ---")]
    // Semi-transparent overlay on top of background for depth effect
    [SerializeField] private Image gradientOverlay;
    [SerializeField] private Color overlayColor = new Color(0.08f, 0.08f, 0.25f, 0.75f);

    [Header("--- Project Logo ---")]
    // The VR Showroom project logo image
    [SerializeField] private Image logoImage;
    // Drag your logo sprite here in the Inspector
    [SerializeField] private Sprite logoSprite;
    // Size of the logo on screen (width x height in pixels)
    [SerializeField] private Vector2 logoSize = new Vector2(320f, 160f);
    // Position of logo (0,0 = centre of screen)
    [SerializeField] private Vector2 logoPosition = new Vector2(0f, 200f);

    [Header("--- Accent / Divider Line ---")]
    // Thin horizontal line below the logo for visual separation
    [SerializeField] private Image accentLine;
    // Bright blue accent colour matching the VR theme
    [SerializeField] private Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    // Awake() runs once when the scene loads — before Start()
    private void Awake()
    {
        ApplyBackgroundColor();
        ApplyGradientOverlay();
        ApplyLogo();
        ApplyAccentLine();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE HELPER METHODS
    // ─────────────────────────────────────────────────────────────────────────

    // Sets the background panel to our dark navy colour
    private void ApplyBackgroundColor()
    {
        // Check the reference is set before using it (safety check)
        if (backgroundPanel == null)
        {
            Debug.LogWarning("MainMenuBackground: backgroundPanel not assigned!");
            return;
        }
        backgroundPanel.color = backgroundColor;
    }

    // Applies semi-transparent overlay for a gradient-like depth effect
    private void ApplyGradientOverlay()
    {
        if (gradientOverlay == null) return;
        gradientOverlay.color = overlayColor;
    }

    // Positions and sizes the project logo, then assigns the sprite
    private void ApplyLogo()
    {
        if (logoImage == null) return;

        // Assign sprite if one has been provided in Inspector
        if (logoSprite != null)
            logoImage.sprite = logoSprite;

        // Get the RectTransform (controls position and size for UI elements)
        RectTransform rt = logoImage.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta        = logoSize;       // set width and height
            rt.anchoredPosition = logoPosition;   // set position on canvas
        }
    }

    // Sets the colour of the thin accent line below the logo
    private void ApplyAccentLine()
    {
        if (accentLine == null) return;
        accentLine.color = accentColor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EDITOR-ONLY  (lets you preview changes in the Unity Editor without play)
    // ─────────────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically refresh visuals when you change values in Inspector
        ApplyBackgroundColor();
        ApplyGradientOverlay();
        ApplyAccentLine();
    }
#endif
}
