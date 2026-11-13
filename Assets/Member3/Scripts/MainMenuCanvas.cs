using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenuCanvas – sets up and owns the root Canvas for the Main Menu.
/// Attach to the MainMenuCanvas GameObject in the MainMenu scene.
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class MainMenuCanvas : MonoBehaviour
{
    // ── Inspector References ─────────────────────────────────────────────────
    [Header("Canvas Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        // Ensure Canvas renders in Screen Space – Overlay mode
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        canvas.renderMode          = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder        = 0;

        // Scale UI relative to 1920×1080 reference resolution
        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();

        canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight  = 0.5f;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Show this canvas.</summary>
    public void Show() => gameObject.SetActive(true);

    /// <summary>Hide this canvas.</summary>
    public void Hide() => gameObject.SetActive(false);
}
