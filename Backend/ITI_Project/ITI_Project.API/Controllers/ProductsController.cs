using ITI_Project.BLL.Interfaces;
using ITI_Project.DAL.Entities;
using ITI_Project.API.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITI_Project.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var products = await _unitOfWork.Products.GetAllAsync(ct);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id, ct);

            if (product is null) 
                return NotFound(new { Message = $"Product with ID {id} not found." });

            return Ok(product);
        }

        [HttpGet("sku/{sku}")]
        public async Task<IActionResult> GetBySku([FromRoute] string sku, CancellationToken ct)
        {
            var product = await _unitOfWork.Products.GetProductBySkuAsync(sku, ct);

            if (product is null) 
                return NotFound(new { Message = $"Product with SKU '{sku}' not found." });

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            // Check if SKU already exists
            var existingProduct = await _unitOfWork.Products.GetProductBySkuAsync(dto.Sku, ct);
            if (existingProduct != null)
                return Conflict(new { Message = $"Product with SKU '{dto.Sku}' already exists." });

            // Map scalar and owned types
            var product = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                DiscountPercentage = dto.DiscountPercentage,
                Rating = dto.Rating,
                Stock = dto.Stock,
                Brand = dto.Brand ?? "Unknown",
                Sku = dto.Sku,
                Weight = dto.Weight,
                WarrantyInformation = dto.WarrantyInformation ?? "No warranty",
                ShippingInformation = dto.ShippingInformation ?? "Standard shipping",
                AvailabilityStatus = dto.AvailabilityStatus ?? "In Stock",
                ReturnPolicy = dto.ReturnPolicy ?? "No returns",
                MinimumOrderQuantity = dto.MinimumOrderQuantity,
                Thumbnail = dto.Thumbnail ?? string.Empty,
                Dimensions = new ProductDimensions
                {
                    Width = dto.Dimensions?.Width ?? 0,
                    Height = dto.Dimensions?.Height ?? 0,
                    Depth = dto.Dimensions?.Depth ?? 0
                },
                Meta = new ProductMeta
                {
                    CreatedAt = dto.Meta?.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = dto.Meta?.UpdatedAt ?? DateTime.UtcNow,
                    Barcode = dto.Meta?.Barcode ?? string.Empty,
                    QrCode = dto.Meta?.QrCode ?? string.Empty
                }
            };

            // Images
            if (dto.Images != null && dto.Images.Count > 0)
            {
                for (int i = 0; i < dto.Images.Count; i++)
                {
                    var url = dto.Images[i];
                    product.Images.Add(new ProductImage
                    {
                        Url = url,
                        SortOrder = i,
                        IsPrimary = string.Equals(url, dto.Thumbnail, StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            // Reviews
            if (dto.Reviews != null)
            {
                foreach (var r in dto.Reviews)
                {
                    product.Reviews.Add(new ProductReview
                    {
                        Rating = r.Rating,
                        Comment = r.Comment,
                        Date = r.Date,
                        ReviewerName = r.ReviewerName,
                        ReviewerEmail = r.ReviewerEmail
                    });
                }
            }

            // Tags: reuse existing, create missing
            var incomingTagNames = (dto.Tags ?? new())
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (incomingTagNames.Count > 0)
            {
                var existing = await _unitOfWork.Tags
                    .FindAsync(t => incomingTagNames.Contains(t.Name), ct);
                var existingLookup = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var name in incomingTagNames)
                {
                    Tag tag;
                    if (!existingLookup.TryGetValue(name, out tag!))
                    {
                        tag = new Tag { Name = name };
                        await _unitOfWork.Tags.AddAsync(tag, ct);
                    }
                    product.ProductTags.Add(new ProductTag { Tag = tag });
                }
            }

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { product.Id, product.Title, product.Sku });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<ProductCreateDto> dtos, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dtos == null || dtos.Count == 0)
                return BadRequest(new { Message = "No products provided." });

            var errors = new List<string>();
            var products = new List<Product>();
            var allTagNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Validate all SKUs first
            var incomingSkus = dtos.Select(d => d.Sku).ToList();
            var duplicateSkus = incomingSkus.GroupBy(s => s)
                                            .Where(g => g.Count() > 1)
                                            .Select(g => g.Key)
                                            .ToList();

            if (duplicateSkus.Any())
            {
                return BadRequest(new { Message = "Duplicate SKUs in request.", DuplicateSkus = duplicateSkus });
            }

            // Check for existing SKUs in database
            var existingProducts = await _unitOfWork.Products
                .FindAsync(p => incomingSkus.Contains(p.Sku), ct);
            var existingSkus = existingProducts.Select(p => p.Sku).ToList();

            if (existingSkus.Any())
            {
                return Conflict(new { Message = "Some SKUs already exist.", ExistingSkus = existingSkus });
            }

            // Collect all unique tag names
            foreach (var dto in dtos)
            {
                if (dto.Tags != null)
                {
                    foreach (var tag in dto.Tags)
                    {
                        var trimmedTag = tag.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedTag))
                        {
                            allTagNames.Add(trimmedTag);
                        }
                    }
                }
            }

            // Fetch all existing tags at once
            var existingTags = await _unitOfWork.Tags
                .FindAsync(t => allTagNames.Contains(t.Name), ct);
            var tagLookup = existingTags.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            // Create missing tags
            foreach (var tagName in allTagNames)
            {
                if (!tagLookup.ContainsKey(tagName))
                {
                    var newTag = new Tag { Name = tagName };
                    await _unitOfWork.Tags.AddAsync(newTag, ct);
                    tagLookup[tagName] = newTag;
                }
            }

            // Map all DTOs to products
            foreach (var dto in dtos)
            {
                var product = new Product
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Category = dto.Category,
                    Price = dto.Price,
                    DiscountPercentage = dto.DiscountPercentage,
                    Rating = dto.Rating,
                    Stock = dto.Stock,
                    Brand = dto.Brand ?? "Unknown",
                    Sku = dto.Sku,
                    Weight = dto.Weight,
                    WarrantyInformation = dto.WarrantyInformation ?? "No warranty",
                    ShippingInformation = dto.ShippingInformation ?? "Standard shipping",
                    AvailabilityStatus = dto.AvailabilityStatus ?? "In Stock",
                    ReturnPolicy = dto.ReturnPolicy ?? "No returns",
                    MinimumOrderQuantity = dto.MinimumOrderQuantity,
                    Thumbnail = dto.Thumbnail ?? string.Empty,
                    Dimensions = new ProductDimensions
                    {
                        Width = dto.Dimensions?.Width ?? 0,
                        Height = dto.Dimensions?.Height ?? 0,
                        Depth = dto.Dimensions?.Depth ?? 0
                    },
                    Meta = new ProductMeta
                    {
                        CreatedAt = dto.Meta?.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = dto.Meta?.UpdatedAt ?? DateTime.UtcNow,
                        Barcode = dto.Meta?.Barcode ?? string.Empty,
                        QrCode = dto.Meta?.QrCode ?? string.Empty
                    }
                };

                // Images
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    for (int i = 0; i < dto.Images.Count; i++)
                    {
                        var url = dto.Images[i];
                        product.Images.Add(new ProductImage
                        {
                            Url = url,
                            SortOrder = i,
                            IsPrimary = string.Equals(url, dto.Thumbnail, StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }

                // Reviews
                if (dto.Reviews != null)
                {
                    foreach (var r in dto.Reviews)
                    {
                        product.Reviews.Add(new ProductReview
                        {
                            Rating = r.Rating,
                            Comment = r.Comment,
                            Date = r.Date,
                            ReviewerName = r.ReviewerName,
                            ReviewerEmail = r.ReviewerEmail
                        });
                    }
                }

                // Tags
                if (dto.Tags != null)
                {
                    foreach (var tagName in dto.Tags)
                    {
                        var trimmedTag = tagName.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedTag) && tagLookup.TryGetValue(trimmedTag, out var tag))
                        {
                            product.ProductTags.Add(new ProductTag { Tag = tag });
                        }
                    }
                }

                products.Add(product);
            }

            // Add all products
            await _unitOfWork.Products.AddRangeAsync(products, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Ok(new 
            { 
                Message = $"{products.Count} products created successfully.", 
                Count = products.Count,
                ProductIds = products.Select(p => new { p.Id, p.Title, p.Sku }).ToList()
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id, ct);
            if (product is null)
                return NotFound(new { Message = $"Product with ID {id} not found." });

            // Check if SKU is being changed to one that already exists
            if (product.Sku != dto.Sku)
            {
                var existingProduct = await _unitOfWork.Products.GetProductBySkuAsync(dto.Sku, ct);
                if (existingProduct != null)
                    return Conflict(new { Message = $"Product with SKU '{dto.Sku}' already exists." });
            }

            // Update scalar properties
            product.Title = dto.Title;
            product.Description = dto.Description;
            product.Category = dto.Category;
            product.Price = dto.Price;
            product.DiscountPercentage = dto.DiscountPercentage;
            product.Rating = dto.Rating;
            product.Stock = dto.Stock;
            product.Brand = dto.Brand ?? product.Brand;
            product.Sku = dto.Sku;
            product.Weight = dto.Weight;
            product.WarrantyInformation = dto.WarrantyInformation ?? product.WarrantyInformation;
            product.ShippingInformation = dto.ShippingInformation ?? product.ShippingInformation;
            product.AvailabilityStatus = dto.AvailabilityStatus ?? product.AvailabilityStatus;
            product.ReturnPolicy = dto.ReturnPolicy ?? product.ReturnPolicy;
            product.MinimumOrderQuantity = dto.MinimumOrderQuantity;
            product.Thumbnail = dto.Thumbnail ?? product.Thumbnail;

            // Update owned types
            product.Dimensions.Width = dto.Dimensions?.Width ?? 0;
            product.Dimensions.Height = dto.Dimensions?.Height ?? 0;
            product.Dimensions.Depth = dto.Dimensions?.Depth ?? 0;

            product.Meta.UpdatedAt = DateTime.UtcNow;
            product.Meta.Barcode = dto.Meta?.Barcode ?? product.Meta.Barcode;
            product.Meta.QrCode = dto.Meta?.QrCode ?? product.Meta.QrCode;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Ok(new { Message = "Product updated successfully.", product.Id });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
            if (product is null)
                return NotFound(new { Message = $"Product with ID {id} not found." });

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Ok(new { Message = "Product deleted successfully.", Id = id });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? category, [FromQuery] string? brand, CancellationToken ct)
        {
            var query = _unitOfWork.Products.GetQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            if (!string.IsNullOrWhiteSpace(brand))
                query = query.Where(p => p.Brand == brand);

            var products = await query.ToListAsync(ct);
            return Ok(products);
        }
    }
}