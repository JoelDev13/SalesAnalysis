using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Db;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface ICustomerReadRepository : IRepository<Customer>
    {
        Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
