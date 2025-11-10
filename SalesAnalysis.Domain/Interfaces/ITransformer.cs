using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface ITransformer<TSource, TDestination>
    {
        Task<IEnumerable<TDestination>> TransformAsync(IEnumerable<TSource> source, CancellationToken cancellationToken = default);
    }
}
