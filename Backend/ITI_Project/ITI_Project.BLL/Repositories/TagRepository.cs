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
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        }

        public async Task<IEnumerable<Tag>> GetTagsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => names.Contains(t.Name))
                .ToListAsync(cancellationToken);
        }
    }
}
