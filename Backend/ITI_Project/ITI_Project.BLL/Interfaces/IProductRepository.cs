using ITI_Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.BLL.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetProductWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<Product?> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand, CancellationToken cancellationToken = default);
    }
}
