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
    public class ProductEtlService : IProductEtlService
    {
        private readonly IDataLoader<Product> _dataLoader;
        private readonly ILogger<ProductEtlService> _logger;

        public ProductEtlService(IDataLoader<Product> dataLoader, ILogger<ProductEtlService> logger)
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProductEtlResult> ProcessProductsAsync(IEnumerable<ProductCsv> products, CancellationToken cancellationToken = default)
        {
            var result = new ProductEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting product ETL process");

                var productList = products?.ToList() ?? new List<ProductCsv>();
                result.ProcessedCount = productList.Count;

                if (!productList.Any())
                {
                    _logger.LogWarning("No products provided for ETL processing");
                    return result;
                }

                var validProducts = ValidateAndTransformProducts(productList, result);
                if (!validProducts.Any())
                {
                    return result;
                }

                var affectedRows = await _dataLoader.LoadAsync(validProducts, cancellationToken);
                result.InsertedCount = affectedRows;

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Product ETL completed successfully. Processed: {Processed}, Affected: {Affected}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.InsertedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing products: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during product ETL process");
                throw;
            }
        }

        private IEnumerable<Product> ValidateAndTransformProducts(IEnumerable<ProductCsv> csvProducts, ProductEtlResult result)
        {
            var validProducts = new List<Product>();

            foreach (var csvProduct in csvProducts)
            {
                if (string.IsNullOrWhiteSpace(csvProduct.ProductName))
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Product {csvProduct.ProductId}: ProductName is required");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (csvProduct.Price <= 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Product {csvProduct.ProductId}: Price must be greater than 0");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (csvProduct.Stock < 0)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Product {csvProduct.ProductId}: Stock cannot be negative");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                validProducts.Add(new Product
                {
                    ProductName = csvProduct.ProductName?.Trim(),
                    Category = csvProduct.Category?.Trim() ?? "Unknown",
                    Price = csvProduct.Price,
                    Stock = csvProduct.Stock
                });
            }

            return validProducts;
        }
    }
}
