using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IDataLoader<in T>
    {
        Task<int> LoadAsync(IEnumerable<T> data, CancellationToken cancellationToken = default);
    }
}
