using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IStagingWriter
    {
        Task WriteAsync<T>(IEnumerable<T> data, string artifactName, CancellationToken cancellationToken = default);
    }
}
