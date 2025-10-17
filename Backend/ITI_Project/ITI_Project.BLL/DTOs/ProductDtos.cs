using ITI_Project.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.BLL.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public double DiscountPercentage { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public List<string>? Tags { get; set; }

        public string? Brand { get; set; }

        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public double Weight { get; set; }

        public ProductDimensionsDto? Dimensions { get; set; }

        public string? WarrantyInformation { get; set; }

        public string? ShippingInformation { get; set; }

        public string? AvailabilityStatus { get; set; }

        public List<ReviewCreateDto>? Reviews { get; set; }

        public string? ReturnPolicy { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int MinimumOrderQuantity { get; set; }

        public ProductMetaDto? Meta { get; set; }

        public List<string>? Images { get; set; }

        public string? Thumbnail { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double DiscountPercentage { get; set; }
        public double Rating { get; set; }
        public int Stock { get; set; }
        public List<string> Tags { get; set; } = new();
        public string Brand { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public double Weight { get; set; }
        public ProductDimensions Dimensions { get; set; } = null!;
        public string WarrantyInformation { get; set; } = string.Empty;
        public string ShippingInformation { get; set; } = string.Empty;
        public string AvailabilityStatus { get; set; } = string.Empty;
        public List<ReviewResponseDto> Reviews { get; set; } = new();
        public string ReturnPolicy { get; set; } = string.Empty;
        public int MinimumOrderQuantity { get; set; }
        public ProductMeta Meta { get; set; } = null!;
        public List<ImageResponseDto> Images { get; set; } = new();
        public string Thumbnail { get; set; } = string.Empty;
    }

    public class ProductDimensionsDto
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
    }

    public class ReviewCreateDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public string ReviewerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ReviewerEmail { get; set; } = string.Empty;
    }

    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
    }

    public class ProductMetaDto
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Barcode { get; set; }
        public string? QrCode { get; set; }
    }

    public class ImageResponseDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class BulkProductResultDto
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ProductSummaryDto> Products { get; set; } = new();
    }

    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
    }
}