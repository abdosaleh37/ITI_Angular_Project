using ITI_Project.BLL.Interfaces;
using ITI_Project.BLL.DTOs;
using ITI_Project.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace ITI_Project.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
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

            var response = _mapper.Map<ProductResponseDto>(product);
            return Ok(response);
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

            var existingProduct = await _unitOfWork.Products.GetProductBySkuAsync(dto.Sku, ct);
            if (existingProduct != null)
                return Conflict(new { Message = $"Product with SKU '{dto.Sku}' already exists." });

            var product = _mapper.Map<Product>(dto);

            await AttachTagsToProduct(product, dto.Tags, ct);

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

            // Validate SKUs
            var incomingSkus = dtos.Select(d => d.Sku).ToList();
            var duplicateSkus = incomingSkus.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateSkus.Any())
                return BadRequest(new { Message = "Duplicate SKUs in request.", DuplicateSkus = duplicateSkus });

            var existingProducts = await _unitOfWork.Products.FindAsync(p => incomingSkus.Contains(p.Sku), ct);
            var existingSkus = existingProducts.Select(p => p.Sku).ToList();
            if (existingSkus.Any())
                return Conflict(new { Message = "Some SKUs already exist.", ExistingSkus = existingSkus });

            // Prepare all tags once
            var allTagNames = dtos
                .SelectMany(d => d.Tags ?? new())
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var tagLookup = await PrepareTagLookup(allTagNames, ct);

            // Map all DTOs to products using AutoMapper
            var products = dtos.Select(dto =>
            {
                var product = _mapper.Map<Product>(dto);
                AttachTagsToProductFromLookup(product, dto.Tags, tagLookup);
                return product;
            }).ToList();

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

            if (product.Sku != dto.Sku)
            {
                var existingProduct = await _unitOfWork.Products.GetProductBySkuAsync(dto.Sku, ct);
                if (existingProduct != null)
                    return Conflict(new { Message = $"Product with SKU '{dto.Sku}' already exists." });
            }

            // Update properties using manual mapping for better control
            UpdateProductFromDto(product, dto);

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

        #region Private Helper Methods

        private async Task AttachTagsToProduct(Product product, List<string>? tagNames, CancellationToken ct)
        {
            if (tagNames == null || tagNames.Count == 0) return;

            var cleanedNames = tagNames
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleanedNames.Count == 0) return;

            var existing = await _unitOfWork.Tags.FindAsync(t => cleanedNames.Contains(t.Name), ct);
            var existingLookup = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var name in cleanedNames)
            {
                if (!existingLookup.TryGetValue(name, out var tag))
                {
                    tag = new Tag { Name = name };
                    await _unitOfWork.Tags.AddAsync(tag, ct);
                    existingLookup[name] = tag;
                }
                product.ProductTags.Add(new ProductTag { Tag = tag });
            }
        }

        private async Task<Dictionary<string, Tag>> PrepareTagLookup(HashSet<string> tagNames, CancellationToken ct)
        {
            var existingTags = await _unitOfWork.Tags.FindAsync(t => tagNames.Contains(t.Name), ct);
            var tagLookup = existingTags.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var tagName in tagNames)
            {
                if (!tagLookup.ContainsKey(tagName))
                {
                    var newTag = new Tag { Name = tagName };
                    await _unitOfWork.Tags.AddAsync(newTag, ct);
                    tagLookup[tagName] = newTag;
                }
            }

            return tagLookup;
        }

        private void AttachTagsToProductFromLookup(Product product, List<string>? tagNames, Dictionary<string, Tag> tagLookup)
        {
            if (tagNames == null) return;

            foreach (var tagName in tagNames)
            {
                var trimmedTag = tagName.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedTag) && tagLookup.TryGetValue(trimmedTag, out var tag))
                {
                    product.ProductTags.Add(new ProductTag { Tag = tag });
                }
            }
        }

        private void UpdateProductFromDto(Product product, ProductCreateDto dto)
        {
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

            if (dto.Dimensions != null)
            {
                product.Dimensions.Width = dto.Dimensions.Width;
                product.Dimensions.Height = dto.Dimensions.Height;
                product.Dimensions.Depth = dto.Dimensions.Depth;
            }

            product.Meta.UpdatedAt = DateTime.UtcNow;
            if (dto.Meta?.Barcode != null) product.Meta.Barcode = dto.Meta.Barcode;
            if (dto.Meta?.QrCode != null) product.Meta.QrCode = dto.Meta.QrCode;
        }

        #endregion
    }
}