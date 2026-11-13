using UnityEngine;

public class Product : MonoBehaviour
{
    public string productName = "New Product";
    public string category = "Garments";
    public string price;
    [TextArea(2, 5)] public string description;
    [TextArea(2, 5)] public string specifications;
}