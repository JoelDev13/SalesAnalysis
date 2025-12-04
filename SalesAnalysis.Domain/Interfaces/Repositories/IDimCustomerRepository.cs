using SalesAnalysis.Domain.Entities.Dimensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces.Repositories
{
    public interface IDimCustomerRepository : IDimensionRepository<DimCustomer>
    {
        Task<DimCustomer> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimCustomer>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimCustomer>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimCustomer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
    }
}
