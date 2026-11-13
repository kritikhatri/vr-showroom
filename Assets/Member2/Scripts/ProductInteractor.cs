using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProductInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float reach = 3f;
    [SerializeField] private bool showDebug = true;

    public static event Action<Product> OnProductSelected;
    public Product CurrentTarget { get; private set; }

    private InputAction interactAction;
    private string debugText = "";

    private void Awake()
    {
        interactAction = inputActions.FindActionMap("Player", true).FindAction("Interact", true);
    }

    private void Update()
    {
        CurrentTarget = null;
        debugText = "Ray hits: nothing";

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, 20f);   // look further for debugging
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (hits.Length > 0)
        {
            debugText = "Ray hits:";
            foreach (RaycastHit hit in hits)
            {
                Product p = hit.collider.GetComponentInParent<Product>();
                debugText += $"\n  {hit.collider.name}  ({hit.distance:0.0} m){(p != null ? "  <- PRODUCT" : "")}";
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

    private void OnGUI()
    {
        var big = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height / 2 - 15, Screen.width, 30), "+", big);

        if (CurrentTarget != null)
            GUI.Label(new Rect(0, Screen.height / 2 + 30, Screen.width, 30),
                      "Press E to inspect " + CurrentTarget.productName, big);

        if (showDebug)
            GUI.Label(new Rect(10, 10, 600, 300), debugText,
                      new GUIStyle(GUI.skin.label) { fontSize = 16 });
    }
}