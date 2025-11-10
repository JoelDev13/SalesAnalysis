using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IEtlService
    {
        Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
