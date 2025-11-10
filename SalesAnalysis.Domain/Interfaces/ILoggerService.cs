using System;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface ILoggerService
    {
        Task LogInformationAsync(string message, CancellationToken cancellationToken = default);
        Task LogWarningAsync(string message, CancellationToken cancellationToken = default);
        Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);
        Task LogPerformanceAsync(string operation, TimeSpan duration, CancellationToken cancellationToken = default);
    }
}
