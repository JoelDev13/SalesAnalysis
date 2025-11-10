using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Extractors
{
    public class ApiExtractor<T> : IExtractor<T> where T : class
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _clientName;
        private readonly string _endpoint;
        private readonly ILogger<ApiExtractor<T>> _logger;

        public string SourceName => $"Api:{_endpoint}";

        public ApiExtractor(
            IHttpClientFactory httpClientFactory,
            string clientName,
            string endpoint,
            ILogger<ApiExtractor<T>> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _clientName = string.IsNullOrWhiteSpace(clientName) ? throw new ArgumentException("Client name is required", nameof(clientName)) : clientName;
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? throw new ArgumentException("Endpoint is required", nameof(endpoint)) : endpoint;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<T>> ExtractAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting API extraction from {Endpoint}", _endpoint);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var client = _httpClientFactory.CreateClient(_clientName);
                using var response = await client.GetAsync(_endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var data = await JsonSerializer.DeserializeAsync<List<T>>(responseStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }, cancellationToken);

                stopwatch.Stop();
                var count = data?.Count ?? 0;
                _logger.LogInformation("API extraction completed. Records: {Count}. Duration: {Elapsed} ms", count, stopwatch.ElapsedMilliseconds);

                return data ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting data from API {Endpoint}", _endpoint);
                throw;
            }
        }
    }
}
