using ITI_Project.BLL.Interfaces;
using ITI_Project.DAL.Data;
using ITI_Project.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.BLL.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Category == category)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Brand == brand)
                .ToListAsync(cancellationToken);
        }
    }
}
