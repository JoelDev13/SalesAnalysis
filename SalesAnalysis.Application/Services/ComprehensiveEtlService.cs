using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Csv;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Factories;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Application.Services
{
    public class ComprehensiveEtlService : IComprehensiveEtlService
    {
        private readonly IExtractorFactory _extractorFactory;
        private readonly ICustomerEtlService _customerEtlService;
        private readonly IProductEtlService _productEtlService;
        private readonly IOrderEtlService _orderEtlService;
        private readonly IOrderDetailEtlService _orderDetailEtlService;
        private readonly IDimensionEtlService _dimensionEtlService;
        private readonly ILogger<ComprehensiveEtlService> _logger;

        public ComprehensiveEtlService(
            IExtractorFactory extractorFactory,
            ICustomerEtlService customerEtlService,
            IProductEtlService productEtlService,
            IOrderEtlService orderEtlService,
            IOrderDetailEtlService orderDetailEtlService,
            IDimensionEtlService dimensionEtlService,
            ILogger<ComprehensiveEtlService> logger)
        {
            _extractorFactory = extractorFactory ?? throw new ArgumentNullException(nameof(extractorFactory));
            _customerEtlService = customerEtlService ?? throw new ArgumentNullException(nameof(customerEtlService));
            _productEtlService = productEtlService ?? throw new ArgumentNullException(nameof(productEtlService));
            _orderEtlService = orderEtlService ?? throw new ArgumentNullException(nameof(orderEtlService));
            _orderDetailEtlService = orderDetailEtlService ?? throw new ArgumentNullException(nameof(orderDetailEtlService));
            _dimensionEtlService = dimensionEtlService ?? throw new ArgumentNullException(nameof(dimensionEtlService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ComprehensiveEtlResult> RunCompleteEtlAsync(CancellationToken cancellationToken = default)
        {
            var result = new ComprehensiveEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting comprehensive ETL process");

                await ProcessCustomersAsync(result, cancellationToken);
                await ProcessProductsAsync(result, cancellationToken);
                await ProcessOrdersAsync(result, cancellationToken);
                await ProcessOrderDetailsAsync(result, cancellationToken);
                await ProcessDimensionsAsync(result, cancellationToken);

                stopwatch.Stop();
                result.TotalTime = stopwatch.Elapsed;

                _logger.LogInformation("Comprehensive ETL completed successfully. Total time: {Time}ms", result.TotalTime.TotalMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.TotalTime = stopwatch.Elapsed;
                var errors = result.Errors.ToList();
                errors.Add($"ETL process failed: {ex.Message}");
                result.Errors = errors.ToArray();
                _logger.LogError(ex, "Error during comprehensive ETL process");
                throw;
            }
        }

        private async Task ProcessCustomersAsync(ComprehensiveEtlResult result, CancellationToken cancellationToken)
        {
            try
            {
                var csvExtractor = _extractorFactory.CreateCsvExtractor<CustomerCsv>("..\\Csv\\customers.csv");
                var customers = await csvExtractor.ExtractAsync(cancellationToken);
                
                var customerResult = await _customerEtlService.ProcessCustomersAsync(customers, cancellationToken);
                result.CustomersProcessed = customerResult.ProcessedCount;
                
                if (!customerResult.IsSuccess)
                {
                    var errors = result.Errors.ToList();
                    errors.AddRange(customerResult.ValidationErrors);
                    result.Errors = errors.ToArray();
                }
            }
            catch (Exception ex)
            {
                var errors = result.Errors.ToList();
                errors.Add($"Customer processing failed: {ex.Message}");
                result.Errors = errors.ToArray();
                _logger.LogError(ex, "Error processing customers");
            }
        }

        private async Task ProcessProductsAsync(ComprehensiveEtlResult result, CancellationToken cancellationToken)
        {
            try
            {
                var csvExtractor = _extractorFactory.CreateCsvExtractor<ProductCsv>("..\\Csv\\products.csv");
                var products = await csvExtractor.ExtractAsync(cancellationToken);
                
                var productResult = await _productEtlService.ProcessProductsAsync(products, cancellationToken);
                result.ProductsProcessed = productResult.ProcessedCount;
                
                if (!productResult.IsSuccess)
                {
                    var errors = result.Errors.ToList();
                    errors.AddRange(productResult.ValidationErrors);
                    result.Errors = errors.ToArray();
                }
            }
            catch (Exception ex)
            {
                var errors = result.Errors.ToList();
                errors.Add($"Product processing failed: {ex.Message}");
                result.Errors = errors.ToArray();
                _logger.LogError(ex, "Error processing products");
            }
        }

        private async Task ProcessOrdersAsync(ComprehensiveEtlResult result, CancellationToken cancellationToken)
        {
            try
            {
                var csvExtractor = _extractorFactory.CreateCsvExtractor<OrderCsv>("..\\Csv\\orders.csv");
                var orders = await csvExtractor.ExtractAsync(cancellationToken);
                
                var orderResult = await _orderEtlService.ProcessOrdersAsync(orders, cancellationToken);
                result.OrdersProcessed = orderResult.ProcessedCount;
                
                if (!orderResult.IsSuccess)
                {
                    var errors = result.Errors.ToList();
                    errors.AddRange(orderResult.ValidationErrors);
                    result.Errors = errors.ToArray();
                }
            }
            catch (Exception ex)
            {
                var errors = result.Errors.ToList();
                errors.Add($"Order processing failed: {ex.Message}");
                result.Errors = errors.ToArray();
                _logger.LogError(ex, "Error processing orders");
            }
        }

        private async Task ProcessOrderDetailsAsync(ComprehensiveEtlResult result, CancellationToken cancellationToken)
        {
            try
            {
                var csvExtractor = _extractorFactory.CreateCsvExtractor<OrderDetailCsv>("..\\Csv\\order_details.csv");
                var orderDetails = await csvExtractor.ExtractAsync(cancellationToken);
                
                var orderDetailResult = await _orderDetailEtlService.ProcessOrderDetailsAsync(orderDetails, cancellationToken);
                result.OrderDetailsProcessed = orderDetailResult.ProcessedCount;
                
                if (!orderDetailResult.IsSuccess)
                {
                    var errors = result.Errors.ToList();
                    errors.AddRange(orderDetailResult.ValidationErrors);
                    result.Errors = errors.ToArray();
                }
            }
            catch (Exception ex)
            {
                var errors = result.Errors.ToList();
                errors.Add($"OrderDetail processing failed: {ex.Message}");
                result.Errors = errors.ToArray();
                _logger.LogError(ex, "Error processing order details");
            }
        }

        private async Task ProcessDimensionsAsync(ComprehensiveEtlResult result, CancellationToken cancellationToken)
        {
            try
            {
                var customerExtractor = _extractorFactory.CreateDatabaseExtractor(
                    "Server=Joel;Database=AnalisisDeVentasEtl;Trusted_Connection=true;TrustServerCertificate=true;",
                    "SELECT CustomerId, FirstName, LastName, Email, Phone, City, Country FROM dbo.Customers",
                    record => new Customer
                    {
                        CustomerId = Convert.ToInt32(record["CustomerId"]),
                        FirstName = record["FirstName"].ToString(),
                        LastName = record["LastName"].ToString(),
                        Email = record["Email"].ToString(),
                        Phone = record["Phone"].ToString(),
                        City = record["City"].ToString(),
                        Country = record["Country"].ToString()
                    });

                var customers = await customerExtractor.ExtractAsync(cancellationToken);
                var customerDimResult = await _dimensionEtlService.ProcessCustomerDimensionsAsync(customers, cancellationToken);
                result.DimCustomersProcessed = customerDimResult.ProcessedCount;

                var productExtractor = _extractorFactory.CreateDatabaseExtractor(
                    "Server=Joel;Database=AnalisisDeVentasEtl;Trusted_Connection=true;TrustServerCertificate=true;",
                    "SELECT ProductId, ProductName, Category, Price, Stock FROM dbo.Products",
                    record => new Product
                    {
                        ProductId = Convert.ToInt32(record["ProductId"]),
                        ProductName = record["ProductName"].ToString(),
                        Category = record["Category"].ToString(),
                        Price = Convert.ToDecimal(record["Price"]),
                        Stock = Convert.ToInt32(record["Stock"])
                    });

                var products = await productExtractor.ExtractAsync(cancellationToken);
                var productDimResult = await _dimensionEtlService.ProcessProductDimensionsAsync(products, cancellationToken);
                result.DimProductsProcessed = productDimResult.ProcessedCount;

                var dateDimResult = await _dimensionEtlService.ProcessDateDimensionsAsync(
                    DateTime.Now.AddYears(-2), DateTime.Now.AddYears(1), cancellationToken);
                result.DimDatesProcessed = dateDimResult.ProcessedCount;

                if (!customerDimResult.IsSuccess || !productDimResult.IsSuccess || !dateDimResult.IsSuccess)
                {
                    var errors = result.Errors.ToList();
                    errors.AddRange(customerDimResult.ValidationErrors);
                    errors.AddRange(productDimResult.ValidationErrors);
                    errors.AddRange(dateDimResult.ValidationErrors);
                    result.Errors = errors.ToArray();
                }
            }
            catch (Exception ex)
            {
                var errors = result.Errors.ToList();
                errors.Add($"Dimension processing failed: {ex.Message}");
                result.Errors = errors.ToArray();
                _logger.LogError(ex, "Error processing dimensions");
            }
        }
    }
}
