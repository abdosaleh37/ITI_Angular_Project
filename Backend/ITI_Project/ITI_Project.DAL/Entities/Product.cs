using System;
using System.Collections.Generic;

namespace ITI_Project.DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public double DiscountPercentage { get; set; }
        public double Rating { get; set; }
        public int Stock { get; set; }
        public string Brand { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public double Weight { get; set; }
        public ProductDimensions Dimensions { get; set; } = new();
        public string WarrantyInformation { get; set; } = null!;
        public string ShippingInformation { get; set; } = null!;
        public string AvailabilityStatus { get; set; } = null!;
        public string ReturnPolicy { get; set; } = null!;
        public int MinimumOrderQuantity { get; set; }
        public ProductMeta Meta { get; set; } = new();
        public string Thumbnail { get; set; } = null!;

        // Normalized navigations
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }

    // Value objects (owned types)
    public class ProductDimensions
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
    }

    public class ProductMeta
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Barcode { get; set; } = null!;
        public string QrCode { get; set; } = null!;
    }

    // Entities for normalized collections
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }

        public Product Product { get; set; } = null!;
    }

    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime Date { get; set; }
        public string ReviewerName { get; set; } = null!;
        public string ReviewerEmail { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }

    public class ProductTag
    {
        public int ProductId { get; set; }
        public int TagId { get; set; }

        public Product Product { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
