using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Extractors
{
    public class CsvExtractor<T> : IExtractor<T> where T : class
    {
        private readonly string _filePath;
        private readonly ILogger<CsvExtractor<T>> _logger;

        public string SourceName => $"CSV:{Path.GetFileName(_filePath)}";

        public CsvExtractor(string filePath, ILogger<CsvExtractor<T>> logger)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<T>> ExtractAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"CSV file not found at path '{_filePath}'.");
            }

            _logger.LogInformation("Starting CSV extraction from {FilePath}", _filePath);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim,
                    IgnoreBlankLines = true
                });

                var records = await Task.Run(() => csv.GetRecords<T>().ToList(), cancellationToken);
                stopwatch.Stop();

                _logger.LogInformation("Finished CSV extraction. Records: {Count}. Duration: {Elapsed} ms", records.Count, stopwatch.ElapsedMilliseconds);
                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting data from CSV at {FilePath}", _filePath);
                throw;
            }
        }
    }
}
