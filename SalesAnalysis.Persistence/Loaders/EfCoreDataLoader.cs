using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Loaders
{
    public class EfCoreDataLoader<T> : IDataLoader<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EfCoreDataLoader<T>> _logger;

        public EfCoreDataLoader(AppDbContext context, ILogger<EfCoreDataLoader<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> LoadAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
        {
            if (data is null)
            {
                return 0;
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting data load using EF Core for entity {EntityName}", typeof(T).Name);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _context.Set<T>().AddRangeAsync(data, cancellationToken);
                var affected = await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Data load completed. Rows affected: {Affected}. Duration: {Elapsed} ms", affected, stopwatch.ElapsedMilliseconds);
                return affected;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error loading data for entity {EntityName}", typeof(T).Name);
                throw;
            }
        }
    }
}
