using System;
using System.Data;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Factories;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Persistence.Extractors;
using Microsoft.Extensions.Http;
using System.Net.Http;

namespace SalesAnalysis.Persistence.Factories
{
    public class ExtractorFactory : IExtractorFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public ExtractorFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public IExtractor<T> CreateCsvExtractor<T>(string filePath) where T : class
        {
            var logger = _loggerFactory.CreateLogger<CsvExtractor<T>>();
            return new CsvExtractor<T>(filePath, logger);
        }

        public IExtractor<T> CreateDatabaseExtractor<T>(string connectionString, string query, Func<IDataRecord, T> mapper) where T : class
        {
            var logger = _loggerFactory.CreateLogger<DatabaseExtractor<T>>();
            return new DatabaseExtractor<T>(connectionString, query, mapper, logger);
        }

        public IExtractor<T> CreateApiExtractor<T>(string clientName, string endpoint) where T : class
        {
            var logger = _loggerFactory.CreateLogger<ApiExtractor<T>>();
            return new ApiExtractor<T>(_httpClientFactory, clientName, endpoint, logger);
        }
    }
}
