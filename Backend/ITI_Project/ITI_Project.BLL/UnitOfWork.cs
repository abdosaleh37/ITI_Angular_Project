using ITI_Project.BLL.Interfaces;
using ITI_Project.BLL.Repositories;
using ITI_Project.DAL.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.BLL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IProductRepository? _productRepository;
        private ITagRepository? _tagRepository;
        private IOrderRepository? _orderRepository;
        private IOrderItemRepository? _orderItemRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products => 
            _productRepository ??= new ProductRepository(_context);

        public ITagRepository Tags => 
            _tagRepository ??= new TagRepository(_context);

        public IOrderRepository Orders => 
            _orderRepository ??= new OrderRepository(_context);

        public IOrderItemRepository OrderItems => 
            _orderItemRepository ??= new OrderItemRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
