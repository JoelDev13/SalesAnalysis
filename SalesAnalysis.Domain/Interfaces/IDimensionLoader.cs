using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IDimensionLoader<T> where T : class
    {
        Task<int> LoadDimensionsAsync(IEnumerable<T> sourceData, CancellationToken cancellationToken = default);
        Task<int> UpsertDimensionsAsync(IEnumerable<T> sourceData, CancellationToken cancellationToken = default);
        Task<bool> ValidateDimensionDataAsync(IEnumerable<T> data, CancellationToken cancellationToken = default);
    }
}
