using AutoMapper;
using ITI_Project.BLL.Common;
using ITI_Project.BLL.DTOs;
using ITI_Project.BLL.Interfaces;
using ITI_Project.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITI_Project.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            var productDtos = products.Select(p => _mapper.Map<ProductResponseDto>(p)).ToList();
            return Result<IEnumerable<ProductResponseDto>>.Success(productDtos);
        }

        public async Task<Result<ProductResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id, cancellationToken);

            if (product is null)
            {
                return Result<ProductResponseDto>.Failure($"Product with ID {id} not found.");
            }

            var response = _mapper.Map<ProductResponseDto>(product);
            return Result<ProductResponseDto>.Success(response);
        }

        public async Task<Result<ProductResponseDto>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetProductBySkuAsync(sku, cancellationToken);

            if (product is null)
            {
                return Result<ProductResponseDto>.Failure($"Product with SKU '{sku}' not found.");
            }

            var response = _mapper.Map<ProductResponseDto>(product);
            return Result<ProductResponseDto>.Success(response);
        }

        public async Task<Result<ProductResponseDto>> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
        {
            // Check if SKU already exists
            var existingProduct = await _unitOfWork.Products.GetProductBySkuAsync(dto.Sku, cancellationToken);
            if (existingProduct != null)
            {
                return Result<ProductResponseDto>.Failure($"Product with SKU '{dto.Sku}' already exists.");
            }

            var product = _mapper.Map<Product>(dto);

            // Attach tags
            await AttachTagsToProduct(product, dto.Tags, cancellationToken);

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload product with details
            var createdProduct = await _unitOfWork.Products.GetProductWithDetailsAsync(product.Id, cancellationToken);
            var response = _mapper.Map<ProductResponseDto>(createdProduct!);

            return Result<ProductResponseDto>.Success(response);
        }

        public async Task<Result<BulkProductResultDto>> CreateBulkAsync(List<ProductCreateDto> dtos, CancellationToken cancellationToken = default)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return Result<BulkProductResultDto>.Failure("No products provided.");
            }

            // Validate SKUs for duplicates in the request
            var incomingSkus = dtos.Select(d => d.Sku).ToList();
            var duplicateSkus = incomingSkus
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSkus.Any())
            {
                return Result<BulkProductResultDto>.Failure(
                    $"Duplicate SKUs in request: {string.Join(", ", duplicateSkus)}"
                );
            }

            // Check if any SKUs already exist in database
            var existingProducts = await _unitOfWork.Products.FindAsync(
                p => incomingSkus.Contains(p.Sku), 
                cancellationToken
            );
            var existingSkus = existingProducts.Select(p => p.Sku).ToList();

            if (existingSkus.Any())
            {
                return Result<BulkProductResultDto>.Failure(
                    $"Some SKUs already exist: {string.Join(", ", existingSkus)}"
                );
            }

            // Prepare all tags once
            var allTagNames = dtos
                .SelectMany(d => d.Tags ?? new())
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var tagLookup = await PrepareTagLookup(allTagNames, cancellationToken);

            // Map all DTOs to products
            var products = dtos.Select(dto =>
            {
                var product = _mapper.Map<Product>(dto);
                AttachTagsToProductFromLookup(product, dto.Tags, tagLookup);
                return product;
            }).ToList();

            await _unitOfWork.Products.AddRangeAsync(products, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = new BulkProductResultDto
            {
                Count = products.Count,
                Products = products.Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Sku = p.Sku
                }).ToList()
            };

            return Result<BulkProductResultDto>.Success(resultDto);
        }

        public async Task<Result<ProductResponseDto>> UpdateAsync(int id, ProductCreateDto dto, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id, cancellationToken);
            
            if (product is null)
            {
                return Result<ProductResponseDto>.Failure($"Product with ID {id} not found.");
            }

            // Check if SKU is being changed to an existing one
            if (product.Sku != dto.Sku)
            {
                var existingProduct = await _unitOfWork.Products.GetProductBySkuAsync(dto.Sku, cancellationToken);
                if (existingProduct != null)
                {
                    return Result<ProductResponseDto>.Failure($"Product with SKU '{dto.Sku}' already exists.");
                }
            }

            // Update product properties
            UpdateProductFromDto(product, dto);

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload product with details
            var updatedProduct = await _unitOfWork.Products.GetProductWithDetailsAsync(id, cancellationToken);
            var response = _mapper.Map<ProductResponseDto>(updatedProduct!);

            return Result<ProductResponseDto>.Success(response);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            
            if (product is null)
            {
                return Result.Failure($"Product with ID {id} not found.");
            }

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<ProductResponseDto>>> SearchAsync(
            string? category, 
            string? brand, 
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Products.GetQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            var products = await query.ToListAsync(cancellationToken);
            var productDtos = products.Select(p => _mapper.Map<ProductResponseDto>(p)).ToList();

            return Result<IEnumerable<ProductResponseDto>>.Success(productDtos);
        }

        #region Private Helper Methods

        private async Task AttachTagsToProduct(Product product, List<string>? tagNames, CancellationToken cancellationToken)
        {
            if (tagNames == null || tagNames.Count == 0) return;

            var cleanedNames = tagNames
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleanedNames.Count == 0) return;

            var existing = await _unitOfWork.Tags.FindAsync(
                t => cleanedNames.Contains(t.Name), 
                cancellationToken
            );
            var existingLookup = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var name in cleanedNames)
            {
                if (!existingLookup.TryGetValue(name, out var tag))
                {
                    tag = new Tag { Name = name };
                    await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
                    existingLookup[name] = tag;
                }
                product.ProductTags.Add(new ProductTag { Tag = tag });
            }
        }

        private async Task<Dictionary<string, Tag>> PrepareTagLookup(
            HashSet<string> tagNames, 
            CancellationToken cancellationToken)
        {
            var existingTags = await _unitOfWork.Tags.FindAsync(
                t => tagNames.Contains(t.Name), 
                cancellationToken
            );
            var tagLookup = existingTags.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var tagName in tagNames)
            {
                if (!tagLookup.ContainsKey(tagName))
                {
                    var newTag = new Tag { Name = tagName };
                    await _unitOfWork.Tags.AddAsync(newTag, cancellationToken);
                    tagLookup[tagName] = newTag;
                }
            }

            return tagLookup;
        }

        private void AttachTagsToProductFromLookup(
            Product product, 
            List<string>? tagNames, 
            Dictionary<string, Tag> tagLookup)
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