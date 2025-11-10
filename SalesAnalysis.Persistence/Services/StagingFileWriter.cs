using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesAnalysis.Domain.Configuration;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Services
{
    public class StagingFileWriter : IStagingWriter
    {
        private readonly ILogger<StagingFileWriter> _logger;
        private readonly CustomerEtlOptions _options;

        public StagingFileWriter(ILogger<StagingFileWriter> logger, IOptions<CustomerEtlOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task WriteAsync<T>(IEnumerable<T> data, string artifactName, CancellationToken cancellationToken = default)
        {
            if (data is null)
            {
                _logger.LogWarning("Staging write skipped for {ArtifactName} because data was null.", artifactName);
                return;
            }

            var directory = string.IsNullOrWhiteSpace(_options.StagingDirectory)
                ? Path.Combine(AppContext.BaseDirectory, "staging")
                : _options.StagingDirectory;
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"{artifactName}-{DateTime.UtcNow:yyyyMMddHHmmss}.json");

            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions
            {
                WriteIndented = true
            }, cancellationToken);

            _logger.LogInformation("Data staged at {FilePath}", filePath);
        }
    }
}
