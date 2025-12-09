using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Facts;

namespace SalesAnalysis.Domain.Interfaces.Repositories
{
    public interface IFactSalesRepository
    {
        Task<int> BulkInsertAsync(IEnumerable<FactSales> facts, CancellationToken cancellationToken = default);
        Task<int> BulkUpsertAsync(IEnumerable<FactSales> facts, CancellationToken cancellationToken = default);
        Task<bool> TruncateTableAsync(CancellationToken cancellationToken = default);
        Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default);
        Task<bool> DeleteByDateRangeAsync(int startDateId, int endDateId, CancellationToken cancellationToken = default);
        Task<IEnumerable<FactSales>> GetByDateRangeAsync(int startDateId, int endDateId, CancellationToken cancellationToken = default);
        Task<FactSales> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<FactSales>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
