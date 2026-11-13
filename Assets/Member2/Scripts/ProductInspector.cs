using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProductInspector : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FirstPersonController movement;
    [SerializeField] private ProductInteractor interactor;

    [Header("Controls")]
    [SerializeField] private float rotateSpeed = 0.3f;
    [SerializeField] private float zoomStep = 0.05f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float maxDistance = 3f;

    // Member 3's UI can listen to these to show/hide the info panel
    public static event Action<Product> OnInspectStarted;
    public static event Action OnInspectEnded;

    public bool IsInspecting => current != null;

    private Product current;
    private Transform pivot;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float distance;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (movement == null) movement = GetComponent<FirstPersonController>();
        if (interactor == null) interactor = GetComponent<ProductInteractor>();
    }

    private void OnEnable()  { ProductInteractor.OnProductSelected += StartInspect; }
    private void OnDisable() { ProductInteractor.OnProductSelected -= StartInspect; }

    private void StartInspect(Product product)
    {
        if (current != null) return;
        current = product;

        // Remember where it was so we can put it back
        Transform t = product.transform;
        originalParent = t.parent;
        originalPosition = t.position;
        originalRotation = t.rotation;

        // Place it in front of the camera, distance based on its size
        Bounds bounds = GetBounds(product);
        distance = Mathf.Clamp(bounds.size.magnitude * 1.2f, minDistance, maxDistance);

        pivot = new GameObject("InspectPivot").transform;
        pivot.position = playerCamera.transform.position + playerCamera.transform.forward * distance;
        t.position += pivot.position - bounds.center;   // centre the model on the pivot
        t.SetParent(pivot, true);

        // Pause walking and selecting (this also frees the cursor)
        movement.enabled = false;
        interactor.enabled = false;

        if (OnInspectStarted != null) OnInspectStarted(product);
    }

    private void Update()
    {
        if (current == null) return;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            // Rotate while left button is held
            if (mouse.leftButton.isPressed)
            {
                Vector2 d = mouse.delta.ReadValue() * rotateSpeed;
                pivot.Rotate(playerCamera.transform.up, -d.x, Space.World);
                pivot.Rotate(playerCamera.transform.right, d.y, Space.World);
            }

            // Zoom with scroll wheel / trackpad
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * zoomStep, minDistance, maxDistance);
                pivot.position = playerCamera.transform.position + playerCamera.transform.forward * distance;
            }
        }

        Keyboard kb = Keyboard.current;
        if (kb != null && (kb.qKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
            EndInspect();
    }

    public void EndInspect()
    {
        if (current == null) return;

        // Put the product back exactly where it was
        Transform t = current.transform;
        t.SetParent(originalParent, true);
        t.SetPositionAndRotation(originalPosition, originalRotation);
        Destroy(pivot.gameObject);
        current = null;

        movement.enabled = true;
        interactor.enabled = true;

        if (OnInspectEnded != null) OnInspectEnded();
    }

    private static Bounds GetBounds(Product product)
    {
        Renderer[] renderers = product.GetComponentsInChildren<Renderer>();
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    // Temporary on-screen help (Member 3 can replace with proper UI)
    private void OnGUI()
    {
        if (current == null) return;
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height - 60, Screen.width, 30),
                  current.productName + "  -  Drag to rotate, Scroll to zoom, Q to exit", style);
    }
}
