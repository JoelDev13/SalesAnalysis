using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IComprehensiveEtlService
    {
        Task<ComprehensiveEtlResult> RunCompleteEtlAsync(CancellationToken cancellationToken = default);
    }
}
