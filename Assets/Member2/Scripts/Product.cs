using UnityEngine;

public class Product : MonoBehaviour
{
    public string productName = "New Product";
    public string category = "Garments";
    public string price;
    [TextArea(2, 5)] public string description;
    [TextArea(2, 5)] public string specifications;

    // Automatically puts one Box Collider around the whole model
    private void Awake()
    {
        Debug.Log("Product ready: " + name);

        // Remove any old collider on the parent (e.g. one added by hand earlier)
        foreach (Collider old in GetComponents<Collider>()) Destroy(old);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning(name + ": no visible parts found");
            return;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        Vector3 scale = transform.lossyScale;
        box.center = transform.InverseTransformPoint(bounds.center);
        box.size = new Vector3(bounds.size.x / Mathf.Abs(scale.x),
                               bounds.size.y / Mathf.Abs(scale.y),
                               bounds.size.z / Mathf.Abs(scale.z));

        Debug.Log(name + ": collider added, size " + bounds.size);
    }
}
