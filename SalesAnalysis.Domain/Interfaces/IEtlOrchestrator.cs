using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IEtlOrchestrator
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
