using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Services
{
    public class StandardLoggerService : ILoggerService
    {
        private readonly ILogger<StandardLoggerService> _logger;

        public StandardLoggerService(ILogger<StandardLoggerService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task LogInformationAsync(string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(message);
            return Task.CompletedTask;
        }

        public Task LogWarningAsync(string message, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(message);
            return Task.CompletedTask;
        }

        public Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
        {
            if (exception is null)
            {
                _logger.LogError(message);
            }
            else
            {
                _logger.LogError(exception, message);
            }

            return Task.CompletedTask;
        }

        public Task LogPerformanceAsync(string operation, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("{Operation} completed in {Elapsed} ms", operation, duration.TotalMilliseconds);
            return Task.CompletedTask;
        }
    }
}
