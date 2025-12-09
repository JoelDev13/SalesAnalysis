using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Facts;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IFactLoader<T> where T : class
    {
        Task<int> LoadFactsAsync(IEnumerable<T> sourceData, CancellationToken cancellationToken = default);
        Task<int> UpsertFactsAsync(IEnumerable<T> sourceData, CancellationToken cancellationToken = default);
        Task<bool> CleanFactTableAsync(CancellationToken cancellationToken = default);
        Task<bool> CleanFactTableByDateRangeAsync(int startDateId, int endDateId, CancellationToken cancellationToken = default);
        Task<bool> ValidateFactDataAsync(IEnumerable<T> data, CancellationToken cancellationToken = default);
    }
}
