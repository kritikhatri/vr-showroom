using System;
using System.Collections.Generic;

namespace Member3.Data
{
    /// <summary>Serializable wrapper matching the sample-products.json root object.</summary>
    [Serializable]
    public class ProductCatalogueData
    {
        public string catalogueName;
        public string industry;
        public Product[] products;
    }

    /// <summary>Member 3's UI-facing product record.</summary>
    [Serializable]
    public class Product
    {
        public string id;
        public string name;
        public string category;
        public string description;
        public ProductSpecifications specifications;
        public string image;

        public string Id => id;
        public string Name => name;
        public string Category => category;
        public string Description => description;
        public ProductSpecifications Specifications => specifications;
        public string ImagePath => image;

        public string[] GetSpecificationLines()
        {
            return specifications == null ? Array.Empty<string>() : specifications.ToDisplayLines();
        }
    }

    /// <summary>Garment specification fields used by the sample catalogue.</summary>
    [Serializable]
    public class ProductSpecifications
    {
        public string material;
        public string fit;
        public string availableSizes;
        public string care;

        public string Material => material;
        public string Fit => fit;
        public string AvailableSizes => availableSizes;
        public string Care => care;

        public string[] ToDisplayLines()
        {
            List<string> lines = new List<string>();
            AddLine(lines, "Material", material);
            AddLine(lines, "Fit", fit);
            AddLine(lines, "Available sizes", availableSizes);
            AddLine(lines, "Care", care);
            return lines.ToArray();
        }

        private static void AddLine(List<string> lines, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                lines.Add($"{label}: {value}");
        }
    }
}
