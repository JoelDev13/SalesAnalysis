using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SalesAnalysis.Domain.Configuration;
using SalesAnalysis.Domain.Entities.Api;
using SalesAnalysis.Domain.Entities.Csv;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Factories;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Persistence.Loaders;
using SalesAnalysis.Persistence.Transformers;

namespace SalesAnalysis.Persistence.Services
{
    public class EtlService : IEtlService
    {
        private readonly IExtractorFactory _extractorFactory;
        private readonly ITransformer<CustomerCsv, Customer> _csvTransformer;
        private readonly ITransformer<CustomerApiResponse, Customer> _apiTransformer;
        private readonly IDataLoader<Customer> _dataLoader;
        private readonly ILoggerService _loggerService;
        private readonly IStagingWriter _stagingWriter;
        private readonly CustomerEtlOptions _options;

        public EtlService(
            IExtractorFactory extractorFactory,
            ITransformer<CustomerCsv, Customer> csvTransformer,
            ITransformer<CustomerApiResponse, Customer> apiTransformer,
            IDataLoader<Customer> dataLoader,
            ILoggerService loggerService,
            IStagingWriter stagingWriter,
            IOptions<CustomerEtlOptions> options)
        {
            _extractorFactory = extractorFactory ?? throw new ArgumentNullException(nameof(extractorFactory));
            _csvTransformer = csvTransformer ?? throw new ArgumentNullException(nameof(csvTransformer));
            _apiTransformer = apiTransformer ?? throw new ArgumentNullException(nameof(apiTransformer));
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            _stagingWriter = stagingWriter ?? throw new ArgumentNullException(nameof(stagingWriter));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            await _loggerService.LogInformationAsync("Starting ETL process.", cancellationToken);

            var customers = new List<Customer>();

            try
            {
                if (_options.EnableCsvSource && !string.IsNullOrWhiteSpace(_options.CsvFilePath))
                {
                    var csvExtractor = _extractorFactory.CreateCsvExtractor<CustomerCsv>(_options.CsvFilePath);
                    var csvData = await csvExtractor.ExtractAsync(cancellationToken);
                    var transformedCsv = await _csvTransformer.TransformAsync(csvData, cancellationToken);
                    customers.AddRange(transformedCsv);
                }

                if (_options.EnableDatabaseSource &&
                    !string.IsNullOrWhiteSpace(_options.DatabaseConnectionString) &&
                    !string.IsNullOrWhiteSpace(_options.DatabaseQuery))
                {
                    var dbExtractor = _extractorFactory.CreateDatabaseExtractor(
                        _options.DatabaseConnectionString,
                        _options.DatabaseQuery,
                        record => new Customer
                        {
                            FirstName = record["FirstName"].ToString() ?? string.Empty,
                            LastName = record["LastName"].ToString() ?? string.Empty,
                            Email = record["Email"].ToString() ?? string.Empty,
                            Phone = record["Phone"].ToString() ?? string.Empty,
                            City = record["City"].ToString() ?? string.Empty,
                            Country = record["Country"].ToString() ?? string.Empty
                        });

                    var dbData = await dbExtractor.ExtractAsync(cancellationToken);
                    customers.AddRange(dbData);
                }

                if (_options.EnableApiSource &&
                    !string.IsNullOrWhiteSpace(_options.ApiClientName) &&
                    !string.IsNullOrWhiteSpace(_options.ApiEndpoint))
                {
                    var apiExtractor = _extractorFactory.CreateApiExtractor<CustomerApiResponse>(
                        _options.ApiClientName,
                        _options.ApiEndpoint);

                    var apiData = await apiExtractor.ExtractAsync(cancellationToken);
                    var transformedApi = await _apiTransformer.TransformAsync(apiData, cancellationToken);
                    customers.AddRange(transformedApi);
                }

                if (!customers.Any())
                {
                    await _loggerService.LogWarningAsync("No data extracted from any source.", cancellationToken);
                    return 0;
                }

                await _stagingWriter.WriteAsync(customers, "customers", cancellationToken);

                var inserted = await _dataLoader.LoadAsync(customers, cancellationToken);
                stopwatch.Stop();

                await _loggerService.LogInformationAsync($"ETL process completed. Rows inserted: {inserted}.", cancellationToken);
                await _loggerService.LogPerformanceAsync("ETL", stopwatch.Elapsed, cancellationToken);

                return inserted;
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("ETL process failed.", ex, cancellationToken);
                throw;
            }
        }
    }
}
