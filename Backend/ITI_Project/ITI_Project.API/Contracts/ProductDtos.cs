using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ITI_Project.API.Contracts
{
    public class ProductCreateDto
    {
        [JsonIgnore] // This will ignore the id field during deserialization if present
        public int? Id { get; set; }
        
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public double DiscountPercentage { get; set; }
        public double Rating { get; set; }
        public int Stock { get; set; }
        public List<string>? Tags { get; set; }
        public string? Brand { get; set; }
        public string Sku { get; set; } = null!;
        public double Weight { get; set; }
        public ProductDimensionsDto? Dimensions { get; set; }
        public string? WarrantyInformation { get; set; }
        public string? ShippingInformation { get; set; }
        public string? AvailabilityStatus { get; set; }
        public List<ProductReviewDto>? Reviews { get; set; }
        public string? ReturnPolicy { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public ProductMetaDto? Meta { get; set; }
        public List<string>? Images { get; set; }
        public string? Thumbnail { get; set; }
    }

    public class ProductUpsertDto
    {
        public int? Id { get; set; } // Null or 0 for create, positive value for update
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public double DiscountPercentage { get; set; }
        public double Rating { get; set; }
        public int Stock { get; set; }
        public List<string>? Tags { get; set; }
        public string? Brand { get; set; }
        public string Sku { get; set; } = null!;
        public double Weight { get; set; }
        public ProductDimensionsDto? Dimensions { get; set; }
        public string? WarrantyInformation { get; set; }
        public string? ShippingInformation { get; set; }
        public string? AvailabilityStatus { get; set; }
        public List<ProductReviewDto>? Reviews { get; set; }
        public string? ReturnPolicy { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public ProductMetaDto? Meta { get; set; }
        public List<string>? Images { get; set; }
        public string? Thumbnail { get; set; }
    }

    public class ProductDimensionsDto
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
    }

    public class ProductReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime Date { get; set; }
        public string ReviewerName { get; set; } = null!;
        public string ReviewerEmail { get; set; } = null!;
    }

    public class ProductMetaDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Barcode { get; set; } = null!;
        public string QrCode { get; set; } = null!;
    }
}