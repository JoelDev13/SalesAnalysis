using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Dimensions;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Domain.Interfaces.Repositories;

namespace SalesAnalysis.Persistence.Services
{
    public class DimensionLoader : IDimensionLoader<DimCustomer>, IDimensionLoader<DimProduct>
    {
        private readonly IDimCustomerRepository _customerRepository;
        private readonly IDimProductRepository _productRepository;
        private readonly ILogger<DimensionLoader> _logger;

        public DimensionLoader(
            IDimCustomerRepository customerRepository,
            IDimProductRepository productRepository,
            ILogger<DimensionLoader> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> LoadDimensionsAsync(IEnumerable<DimCustomer> sourceData, CancellationToken cancellationToken = default)
        {
            if (sourceData == null) return 0;

            var customerList = sourceData.ToList();
            if (!customerList.Any()) return 0;

            _logger.LogInformation("Starting customer dimension load for {Count} records", customerList.Count);

            foreach (var customer in customerList)
            {
                customer.CreatedDate = DateTime.UtcNow;
                customer.IsActive = true;
            }

            return await _customerRepository.BulkUpsertAsync(customerList, cancellationToken);
        }

        public async Task<int> LoadDimensionsAsync(IEnumerable<DimProduct> sourceData, CancellationToken cancellationToken = default)
        {
            if (sourceData == null) return 0;

            var productList = sourceData.ToList();
            if (!productList.Any()) return 0;

            _logger.LogInformation("Starting product dimension load for {Count} records", productList.Count);

            foreach (var product in productList)
            {
                product.CreatedDate = DateTime.UtcNow;
                product.IsActive = true;
            }

            return await _productRepository.BulkUpsertAsync(productList, cancellationToken);
        }

        public async Task<int> UpsertDimensionsAsync(IEnumerable<DimCustomer> sourceData, CancellationToken cancellationToken = default)
        {
            return await LoadDimensionsAsync(sourceData, cancellationToken);
        }

        public async Task<int> UpsertDimensionsAsync(IEnumerable<DimProduct> sourceData, CancellationToken cancellationToken = default)
        {
            return await LoadDimensionsAsync(sourceData, cancellationToken);
        }

        public async Task<bool> ValidateDimensionDataAsync(IEnumerable<DimCustomer> data, CancellationToken cancellationToken = default)
        {
            var customerList = data.ToList();
            var validationErrors = new List<string>();

            foreach (var customer in customerList)
            {
                if (string.IsNullOrWhiteSpace(customer.FirstName))
                    validationErrors.Add($"Customer {customer.CustomerId}: FirstName is required");

                if (string.IsNullOrWhiteSpace(customer.LastName))
                    validationErrors.Add($"Customer {customer.CustomerId}: LastName is required");

                if (string.IsNullOrWhiteSpace(customer.Email))
                    validationErrors.Add($"Customer {customer.CustomerId}: Email is required");
            }

            if (validationErrors.Any())
            {
                _logger.LogWarning("Customer dimension validation failed: {Errors}", string.Join("; ", validationErrors));
                return false;
            }

            return true;
        }

        public async Task<bool> ValidateDimensionDataAsync(IEnumerable<DimProduct> data, CancellationToken cancellationToken = default)
        {
            var productList = data.ToList();
            var validationErrors = new List<string>();

            foreach (var product in productList)
            {
                if (string.IsNullOrWhiteSpace(product.ProductName))
                    validationErrors.Add($"Product {product.ProductId}: ProductName is required");

                if (product.Price <= 0)
                    validationErrors.Add($"Product {product.ProductId}: Price must be greater than 0");

                if (product.Stock < 0)
                    validationErrors.Add($"Product {product.ProductId}: Stock cannot be negative");
            }

            if (validationErrors.Any())
            {
                _logger.LogWarning("Product dimension validation failed: {Errors}", string.Join("; ", validationErrors));
                return false;
            }

            return true;
        }

        public async Task<DimCustomer> TransformCustomerToDimension(Customer customer, CancellationToken cancellationToken = default)
        {
            if (customer == null) return null;

            return new DimCustomer
            {
                FirstName = customer.FirstName?.Trim(),
                LastName = customer.LastName?.Trim(),
                Email = customer.Email?.Trim().ToLowerInvariant(),
                Phone = customer.Phone?.Trim(),
                City = customer.City?.Trim(),
                Country = customer.Country?.Trim(),
                Region = customer.Country?.Trim(),
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        public async Task<DimProduct> TransformProductToDimension(Product product, CancellationToken cancellationToken = default)
        {
            if (product == null) return null;

            return new DimProduct
            {
                ProductName = product.ProductName?.Trim(),
                Category = product.Category?.Trim(),
                Subcategory = string.Empty,
                Price = product.Price,
                Stock = product.Stock,
                Brand = string.Empty,
                SKU = string.Empty,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
        }

    }
}
