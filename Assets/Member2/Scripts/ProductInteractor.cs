using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProductInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float reach = 3f;
    [SerializeField] private bool showDebug = true;

    // Member 3's UI listens to this
    public static event Action<Product> OnProductSelected;
    public Product CurrentTarget { get; private set; }

    private InputAction interactAction;
    private string debugText = "";

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        interactAction = inputActions.FindActionMap("Player", true).FindAction("Interact", true);
    }

    private void Update()
    {
        CurrentTarget = null;
        debugText = "Ray hits: nothing";

        // Shoot a ray from the centre of the screen, through hangers/racks
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, 20f);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (hits.Length > 0)
        {
            debugText = "Ray hits:";
            foreach (RaycastHit hit in hits)
            {
                Product p = hit.collider.GetComponentInParent<Product>();
                debugText += "\n  " + hit.collider.name + "  (" + hit.distance.ToString("0.0") + " m)";
                if (p != null) debugText += "  <- PRODUCT";
                if (p != null && CurrentTarget == null && hit.distance <= reach)
                    CurrentTarget = p;
            }
        }

        bool pressed = interactAction.WasPressedThisFrame()
                    || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (CurrentTarget != null && pressed)
        {
            Debug.Log("Selected: " + CurrentTarget.productName);
            OnProductSelected?.Invoke(CurrentTarget);
        }
    }

    // Temporary crosshair + prompt (Member 3 can replace with proper UI)
    private void OnGUI()
    {
        GUIStyle big = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height / 2 - 15, Screen.width, 30), "+", big);

        if (CurrentTarget != null)
            GUI.Label(new Rect(0, Screen.height / 2 + 30, Screen.width, 30),
                      "Press E to inspect " + CurrentTarget.productName, big);

        if (showDebug)
            GUI.Label(new Rect(10, 10, 600, 300), debugText,
                      new GUIStyle(GUI.skin.label) { fontSize = 16 });
    }
}
