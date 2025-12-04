using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesAnalysis.Domain.Interfaces;
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
            var interval = TimeSpan.FromMinutes(1); // cada minuto
            _logger.LogInformation("Worker started. Interval: {Interval} minutes", interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var comprehensiveEtlService = scope.ServiceProvider.GetRequiredService<IComprehensiveEtlService>();

                try
                {
                    var result = await comprehensiveEtlService.RunCompleteEtlAsync(stoppingToken);
                    _logger.LogInformation("Complete ETL cycle completed. Customers: {Customers}, Products: {Products}, Orders: {Orders}, OrderDetails: {OrderDetails}, DimCustomers: {DimCustomers}, DimProducts: {DimProducts}, DimDates: {DimDates}", 
                        result.CustomersProcessed, result.ProductsProcessed, result.OrdersProcessed, result.OrderDetailsProcessed, 
                        result.DimCustomersProcessed, result.DimProductsProcessed, result.DimDatesProcessed);
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
