using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.BLL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ITagRepository Tags { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
