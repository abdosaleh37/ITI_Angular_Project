using ITI_Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.BLL.Interfaces
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetTagsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    }
}
