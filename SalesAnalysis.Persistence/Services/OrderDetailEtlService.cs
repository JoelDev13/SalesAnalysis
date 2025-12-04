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
    public class OrderDetailEtlService : IOrderDetailEtlService
    {
        private readonly IDataLoader<OrderDetail> _dataLoader;
        private readonly ILogger<OrderDetailEtlService> _logger;

        public OrderDetailEtlService(IDataLoader<OrderDetail> dataLoader, ILogger<OrderDetailEtlService> logger)
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDetailEtlResult> ProcessOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails, CancellationToken cancellationToken = default)
        {
            var result = new OrderDetailEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting order detail ETL process");

                var orderDetailList = orderDetails?.ToList() ?? new List<OrderDetailCsv>();
                result.ProcessedCount = orderDetailList.Count;

                if (!orderDetailList.Any())
                {
                    _logger.LogWarning("No order details provided for ETL processing");
                    return result;
                }

                var validOrderDetails = ValidateAndTransformOrderDetails(orderDetailList, result);
                if (!validOrderDetails.Any())
                {
                    return result;
                }

                var affectedRows = await _dataLoader.LoadAsync(validOrderDetails, cancellationToken);
                result.InsertedCount = affectedRows;

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Order detail ETL completed successfully. Processed: {Processed}, Affected: {Affected}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.InsertedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing order details: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during order detail ETL process");
                throw;
            }
        }

        private IEnumerable<OrderDetail> ValidateAndTransformOrderDetails(IEnumerable<OrderDetailCsv> csvOrderDetails, OrderDetailEtlResult result)
        {
            var validOrderDetails = new List<OrderDetail>();

            foreach (var csvOrderDetail in csvOrderDetails)
            {
                if (csvOrderDetail.OrderId <= 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"OrderDetail {csvOrderDetail.OrderId}-{csvOrderDetail.ProductId}: OrderId must be greater than 0");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (csvOrderDetail.ProductId <= 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"OrderDetail {csvOrderDetail.OrderId}-{csvOrderDetail.ProductId}: ProductId must be greater than 0");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (csvOrderDetail.Quantity <= 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"OrderDetail {csvOrderDetail.OrderId}: Quantity must be greater than 0");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (csvOrderDetail.TotalPrice <= 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"OrderDetail {csvOrderDetail.OrderId}: TotalPrice must be greater than 0");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                validOrderDetails.Add(new OrderDetail
                {
                    OrderId = csvOrderDetail.OrderId,
                    ProductId = csvOrderDetail.ProductId,
                    Quantity = csvOrderDetail.Quantity,
                    TotalPrice = csvOrderDetail.TotalPrice
                });
            }

            return validOrderDetails;
        }
    }
}
