using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesAnalysis.Application.Interfaces;
using SalesAnalysis.Domain.Configuration;

namespace SalesAnalysis.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;
        private readonly CustomerEtlOptions _options;

        public Worker(
            IServiceProvider serviceProvider,
            ILogger<Worker> logger,
            IOptions<CustomerEtlOptions> options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.RunIntervalMinutes));
            _logger.LogInformation("Worker started. Interval: {Interval} minutes", interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                try
                {
                    var inserted = await customerService.RunEtlAsync(stoppingToken);
                    _logger.LogInformation("ETL cycle completed. Records inserted: {Records}", inserted);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ETL cycle cancelled.");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during ETL cycle.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Worker stopping.");
        }
    }
}
