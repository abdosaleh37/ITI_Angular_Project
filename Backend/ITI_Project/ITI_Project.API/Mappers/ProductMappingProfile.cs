using AutoMapper;
using ITI_Project.BLL.DTOs;
using ITI_Project.DAL.Entities;

namespace ITI_Project.API.Mappers
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // ProductCreateDto -> Product (for Create operations)
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand ?? "Unknown"))
                .ForMember(dest => dest.WarrantyInformation, opt => opt.MapFrom(src => src.WarrantyInformation ?? "No warranty"))
                .ForMember(dest => dest.ShippingInformation, opt => opt.MapFrom(src => src.ShippingInformation ?? "Standard shipping"))
                .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom(src => src.AvailabilityStatus ?? "In Stock"))
                .ForMember(dest => dest.ReturnPolicy, opt => opt.MapFrom(src => src.ReturnPolicy ?? "No returns"))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail ?? string.Empty))
                .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => MapDimensions(src.Dimensions)))
                .ForMember(dest => dest.Meta, opt => opt.MapFrom(src => MapMeta(src.Meta)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom((src, dest) => MapImages(src.Images, src.Thumbnail)))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => MapReviews(src.Reviews)))
                .ForMember(dest => dest.ProductTags, opt => opt.Ignore()); // Handle separately

            // For response mapping - flatten tags
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.ProductTags.Select(pt => pt.Tag.Name).ToList()))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.Select(i => new ImageResponseDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsPrimary = i.IsPrimary,
                    SortOrder = i.SortOrder
                }).ToList()))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews.Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.Date,
                    ReviewerName = r.ReviewerName,
                    ReviewerEmail = r.ReviewerEmail
                }).ToList()));
        }

        private static ProductDimensions MapDimensions(ProductDimensionsDto? dto)
        {
            if (dto == null)
                return new ProductDimensions { Width = 0, Height = 0, Depth = 0 };

            return new ProductDimensions
            {
                Width = (double)dto.Width,
                Height = (double)dto.Height,
                Depth = (double)dto.Depth
            };
        }

        private static ProductMeta MapMeta(ProductMetaDto? dto)
        {
            if (dto == null)
            {
                return new ProductMeta
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Barcode = string.Empty,
                    QrCode = string.Empty
                };
            }

            return new ProductMeta
            {
                CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt == default ? DateTime.UtcNow : dto.UpdatedAt,
                Barcode = dto.Barcode ?? string.Empty,
                QrCode = dto.QrCode ?? string.Empty
            };
        }

        private static List<ProductImage> MapImages(List<string>? imageUrls, string? thumbnail)
        {
            var images = new List<ProductImage>();

            if (imageUrls == null || imageUrls.Count == 0)
                return images;

            for (int i = 0; i < imageUrls.Count; i++)
            {
                images.Add(new ProductImage
                {
                    Url = imageUrls[i],
                    SortOrder = i,
                    IsPrimary = string.Equals(imageUrls[i], thumbnail, StringComparison.OrdinalIgnoreCase)
                });
            }

            return images;
        }

        private static List<ProductReview> MapReviews(List<ReviewCreateDto>? reviewDtos)
        {
            var reviews = new List<ProductReview>();

            if (reviewDtos == null || reviewDtos.Count == 0)
                return reviews;

            foreach (var dto in reviewDtos)
            {
                reviews.Add(new ProductReview
                {
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    Date = dto.Date,
                    ReviewerName = dto.ReviewerName,
                    ReviewerEmail = dto.ReviewerEmail
                });
            }

            return reviews;
        }
    }
}