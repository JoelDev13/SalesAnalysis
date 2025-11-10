using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerReadRepository
    {
        public CustomerRepository(DbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        }
    }
}
