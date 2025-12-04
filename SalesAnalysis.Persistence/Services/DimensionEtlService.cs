using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Dimensions;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Persistence.Services
{
    public class DimensionEtlService : IDimensionEtlService
    {
        private readonly DimensionLoader _dimensionLoader;
        private readonly ILogger<DimensionEtlService> _logger;

        public DimensionEtlService(DimensionLoader dimensionLoader, ILogger<DimensionEtlService> logger)
        {
            _dimensionLoader = dimensionLoader ?? throw new ArgumentNullException(nameof(dimensionLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DimensionEtlResult> ProcessCustomerDimensionsAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken = default)
        {
            var result = new DimensionEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting customer dimension ETL process");

                var customerList = customers?.ToList() ?? new List<Customer>();
                result.ProcessedCount = customerList.Count;

                if (!customerList.Any())
                {
                    _logger.LogWarning("No customers provided for dimension processing");
                    return result;
                }

                var dimensionCustomers = new List<DimCustomer>();
                foreach (var customer in customerList)
                {
                    var dimCustomer = await _dimensionLoader.TransformCustomerToDimension(customer, cancellationToken);
                    if (dimCustomer != null)
                    {
                        dimensionCustomers.Add(dimCustomer);
                    }
                }

                var isValid = await _dimensionLoader.ValidateDimensionDataAsync(dimensionCustomers, cancellationToken);
                if (!isValid)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add("Customer dimension validation failed");
                    result.ValidationErrors = errors.ToArray();
                    return result;
                }

                var affectedRows = await _dimensionLoader.LoadDimensionsAsync(dimensionCustomers, cancellationToken);
                result.InsertedCount = affectedRows;

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Customer dimension ETL completed successfully. Processed: {Processed}, Affected: {Affected}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.InsertedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing customer dimensions: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during customer dimension ETL process");
                throw;
            }
        }

        public async Task<DimensionEtlResult> ProcessProductDimensionsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            var result = new DimensionEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting product dimension ETL process");

                var productList = products?.ToList() ?? new List<Product>();
                result.ProcessedCount = productList.Count;

                if (!productList.Any())
                {
                    _logger.LogWarning("No products provided for dimension processing");
                    return result;
                }

                var dimensionProducts = new List<DimProduct>();
                foreach (var product in productList)
                {
                    var dimProduct = await _dimensionLoader.TransformProductToDimension(product, cancellationToken);
                    if (dimProduct != null)
                    {
                        dimensionProducts.Add(dimProduct);
                    }
                }

                var isValid = await _dimensionLoader.ValidateDimensionDataAsync(dimensionProducts, cancellationToken);
                if (!isValid)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add("Product dimension validation failed");
                    result.ValidationErrors = errors.ToArray();
                    return result;
                }

                var affectedRows = await _dimensionLoader.LoadDimensionsAsync(dimensionProducts, cancellationToken);
                result.InsertedCount = affectedRows;

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Product dimension ETL completed successfully. Processed: {Processed}, Affected: {Affected}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.InsertedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing product dimensions: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during product dimension ETL process");
                throw;
            }
        }

        public async Task<DimensionEtlResult> ProcessDateDimensionsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var result = new DimensionEtlResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting date dimension ETL process from {StartDate} to {EndDate}", startDate, endDate);

                if (startDate > endDate)
                {
                    var errors = result.ValidationErrors.ToList();
                    errors.Add("Start date cannot be greater than end date");
                    result.ValidationErrors = errors.ToArray();
                    return result;
                }

                var totalDays = (endDate - startDate).Days + 1;
                result.ProcessedCount = totalDays;

                var dateDimensions = new List<DimDate>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var dimDate = CreateDimDateFromDateTime(currentDate);
                    dateDimensions.Add(dimDate);
                    currentDate = currentDate.AddDays(1);
                }

                result.InsertedCount = dateDimensions.Count;

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Date dimension ETL completed successfully. Processed: {Processed}, Duration: {Duration}ms", 
                    result.ProcessedCount, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                var errors = result.ValidationErrors.ToList();
                errors.Add($"Error processing date dimensions: {ex.Message}");
                result.ValidationErrors = errors.ToArray();
                _logger.LogError(ex, "Error during date dimension ETL process");
                throw;
            }
        }

        private DimDate CreateDimDateFromDateTime(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var dateKey = date.Year * 10000 + date.Month * 100 + date.Day;

            return new DimDate
            {
                DateKey = dateKey,
                Date = date,
                Year = date.Year,
                Quarter = (date.Month - 1) / 3 + 1,
                Month = date.Month,
                MonthName = date.ToString("MMMM"),
                WeekOfYear = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                DayOfYear = date.DayOfYear,
                DayOfMonth = date.Day,
                DayOfWeek = (int)date.DayOfWeek,
                DayName = date.DayOfWeek.ToString(),
                IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                IsHoliday = false,
                FiscalYear = $"FY{date.Year}",
                FiscalQuarter = (date.Month - 1) / 3 + 1,
                FiscalMonth = date.Month
            };
        }
    }
}
