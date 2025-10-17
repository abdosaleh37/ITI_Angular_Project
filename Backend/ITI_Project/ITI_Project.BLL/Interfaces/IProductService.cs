using ITI_Project.BLL.Common;
using ITI_Project.BLL.DTOs;

namespace ITI_Project.BLL.Interfaces
{
    public interface IProductService
    {
        Task<Result<IEnumerable<ProductResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<ProductResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<ProductResponseDto>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<Result<ProductResponseDto>> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default);
        Task<Result<BulkProductResultDto>> CreateBulkAsync(List<ProductCreateDto> dtos, CancellationToken cancellationToken = default);
        Task<Result<ProductResponseDto>> UpdateAsync(int id, ProductCreateDto dto, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<ProductResponseDto>>> SearchAsync(string? category, string? brand, CancellationToken cancellationToken = default);
    }
}