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
    public class CustomerEtlService : ICustomerEtlService
    {
        private readonly IDataLoader<Customer> _dataLoader;
        private readonly ILogger<CustomerEtlService> _logger;

        public CustomerEtlService(IDataLoader<Customer> dataLoader, ILogger<CustomerEtlService> logger)
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerEtlResult> ProcessCustomersAsync(IEnumerable<CustomerCsv> customers, CancellationToken cancellationToken = default)
        {
            var result = new CustomerEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting customer ETL process");

                var validCustomers = ValidateAndTransformCustomers(customers, result);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer validation failed with {ErrorCount} errors", result.ValidationErrors.Length);
                    return result;
                }

                var inserted = await _dataLoader.LoadAsync(validCustomers, cancellationToken);
                
                result.ProcessedCount = validCustomers.Count();
                result.InsertedCount = inserted;
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Customer ETL completed successfully. Processed: {Processed}, Affected: {Affected}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.InsertedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing customers: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during customer ETL process");
                throw;
            }
        }

        private IEnumerable<Customer> ValidateAndTransformCustomers(IEnumerable<CustomerCsv> csvCustomers, CustomerEtlResult result)
        {
            var validCustomers = new List<Customer>();

            foreach (var csvCustomer in csvCustomers)
            {
                if (string.IsNullOrWhiteSpace(csvCustomer.FirstName))
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Customer {csvCustomer.CustomerId}: FirstName is required");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(csvCustomer.LastName))
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Customer {csvCustomer.CustomerId}: LastName is required");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(csvCustomer.Email) && !csvCustomer.Email.Contains("@"))
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add($"Customer {csvCustomer.CustomerId}: Invalid email format");
                    result.ValidationErrors = errors.ToArray();
                    continue;
                }

                validCustomers.Add(new Customer
                {
                    FirstName = csvCustomer.FirstName?.Trim(),
                    LastName = csvCustomer.LastName?.Trim(),
                    Email = csvCustomer.Email?.Trim() ?? string.Empty,
                    Phone = csvCustomer.Phone?.Trim(),
                    City = csvCustomer.City?.Trim(),
                    Country = csvCustomer.Country?.Trim()
                });
            }

            return validCustomers;
        }
    }
}
