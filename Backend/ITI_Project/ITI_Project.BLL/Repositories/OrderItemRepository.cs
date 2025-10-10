using ITI_Project.BLL.Interfaces;
using ITI_Project.DAL.Data;
using ITI_Project.DAL.Entities;

namespace ITI_Project.BLL.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
