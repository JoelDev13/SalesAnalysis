using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Extractors
{
    public class DatabaseExtractor<T> : IExtractor<T> where T : class
    {
        private readonly string _connectionString;
        private readonly string _query;
        private readonly Func<IDataRecord, T> _mapper;
        private readonly ILogger<DatabaseExtractor<T>> _logger;

        public string SourceName => "RelationalDatabase";

        public DatabaseExtractor(
            string connectionString,
            string query,
            Func<IDataRecord, T> mapper,
            ILogger<DatabaseExtractor<T>> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<T>> ExtractAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<T>();
            _logger.LogInformation("Starting database extraction.");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = new SqlCommand(_query, connection);
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 120;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    results.Add(_mapper(reader));
                }

                stopwatch.Stop();
                _logger.LogInformation("Database extraction completed. Records: {Count}. Duration: {Elapsed} ms", results.Count, stopwatch.ElapsedMilliseconds);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting data from database.");
                throw;
            }
        }
    }
}
