using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Csv;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Persistence.Services
{
    public class OrderEtlService : IOrderEtlService
    {
        private readonly IDataLoader<Order> _dataLoader;
        private readonly ILogger<OrderEtlService> _logger;

        public OrderEtlService(IDataLoader<Order> dataLoader, ILogger<OrderEtlService> logger)
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderEtlResult> ProcessOrdersAsync(IEnumerable<OrderCsv> orders, CancellationToken cancellationToken = default)
        {
            var result = new OrderEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting order ETL process");

                var orderList = orders?.ToList() ?? new List<OrderCsv>();
                result.ProcessedCount = orderList.Count;

                if (!orderList.Any())
                {
                    _logger.LogWarning("No orders provided for ETL processing");
                    return result;
                }

                var validOrders = ValidateAndTransformOrders(orderList, result);
                if (!validOrders.Any())
                {
                    return result;
                }

                var affectedRows = await _dataLoader.LoadAsync(validOrders, cancellationToken);
                result.InsertedCount = affectedRows;

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Order ETL completed successfully. Processed: {Processed}, Affected: {Affected}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.InsertedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing orders: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during order ETL process");
                throw;
            }
        }

        private IEnumerable<Order> ValidateAndTransformOrders(IEnumerable<OrderCsv> csvOrders, OrderEtlResult result)
        {
            var validOrders = new List<Order>();

            foreach (var csvOrder in csvOrders)
            {
                if (csvOrder.CustomerId <= 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Order {csvOrder.OrderId}: CustomerId must be greater than 0");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (!DateTime.TryParse(csvOrder.OrderDate.ToString(), out var orderDate))
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Order {csvOrder.OrderId}: Invalid OrderDate format");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(csvOrder.Status))
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Order {csvOrder.OrderId}: Status is required");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                validOrders.Add(new Order
                {
                    CustomerId = csvOrder.CustomerId,
                    OrderDate = orderDate,
                    Status = csvOrder.Status?.Trim()
                });
            }

            return validOrders;
        }
    }
}
