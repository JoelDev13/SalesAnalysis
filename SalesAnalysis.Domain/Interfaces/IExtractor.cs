using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IExtractor<T>
    {
        Task<IEnumerable<T>> ExtractAsync(CancellationToken cancellationToken = default);
        string SourceName { get; }
    }
}
