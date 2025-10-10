using ITI_Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.BLL.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderWithDetailsAsync(int orderId, CancellationToken cancellationToken = default);
    }
}
