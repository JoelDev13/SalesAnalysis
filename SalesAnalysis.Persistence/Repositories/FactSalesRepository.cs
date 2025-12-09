using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Facts;
using SalesAnalysis.Domain.Interfaces.Repositories;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Repositories
{
    public class FactSalesRepository : IFactSalesRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FactSalesRepository> _logger;

        public FactSalesRepository(AppDbContext context, ILogger<FactSalesRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> BulkInsertAsync(IEnumerable<FactSales> facts, CancellationToken cancellationToken = default)
        {
            if (facts == null) return 0;

            var factList = facts.ToList();
            if (!factList.Any()) return 0;

            _logger.LogInformation("Starting bulk insert of {Count} fact sales records", factList.Count);

            foreach (var fact in factList)
            {
                fact.CreatedDate = DateTime.UtcNow;
                fact.ModifiedDate = DateTime.UtcNow;
                fact.IsActive = true;
            }

            _context.FactSales.AddRange(factList);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> BulkUpsertAsync(IEnumerable<FactSales> facts, CancellationToken cancellationToken = default)
        {
            if (facts == null) return 0;

            var factList = facts.ToList();
            if (!factList.Any()) return 0;

            _logger.LogInformation("Starting bulk upsert of {Count} fact sales records", factList.Count);

            var existingIds = await _context.FactSales
                .Where(f => factList.Select(fl => fl.OrderId).Contains(f.OrderId))
                .Select(f => f.OrderId)
                .ToListAsync(cancellationToken);

            var toUpdate = factList.Where(f => existingIds.Contains(f.OrderId)).ToList();
            var toInsert = factList.Where(f => !existingIds.Contains(f.OrderId)).ToList();

            foreach (var fact in toUpdate)
            {
                fact.ModifiedDate = DateTime.UtcNow;
                _context.FactSales.Update(fact);
            }

            foreach (var fact in toInsert)
            {
                fact.CreatedDate = DateTime.UtcNow;
                fact.ModifiedDate = DateTime.UtcNow;
                fact.IsActive = true;
                _context.FactSales.Add(fact);
            }

            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> TruncateTableAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Truncating FactSales table");
            
            try
            {
                await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE FactSales", cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error truncating FactSales table");
                return false;
            }
        }

        public async Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Deleting all records from FactSales table");
            
            try
            {
                var allFacts = await _context.FactSales.ToListAsync(cancellationToken);
                _context.FactSales.RemoveRange(allFacts);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all records from FactSales table");
                return false;
            }
        }

        public async Task<bool> DeleteByDateRangeAsync(int startDateId, int endDateId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting FactSales records for date range {StartDateId} to {EndDateId}", startDateId, endDateId);
            
            try
            {
                var factsToDelete = await _context.FactSales
                    .Where(f => f.DateId >= startDateId && f.DateId <= endDateId)
                    .ToListAsync(cancellationToken);

                _context.FactSales.RemoveRange(factsToDelete);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FactSales records for date range {StartDateId} to {EndDateId}", startDateId, endDateId);
                return false;
            }
        }

        public async Task<IEnumerable<FactSales>> GetByDateRangeAsync(int startDateId, int endDateId, CancellationToken cancellationToken = default)
        {
            return await _context.FactSales
                .Include(f => f.Customer)
                .Include(f => f.Product)
                .Include(f => f.Date)
                .Where(f => f.DateId >= startDateId && f.DateId <= endDateId && f.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<FactSales> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.FactSales
                .Include(f => f.Customer)
                .Include(f => f.Product)
                .Include(f => f.Date)
                .FirstOrDefaultAsync(f => f.FactSalesId == id && f.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<FactSales>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.FactSales
                .Include(f => f.Customer)
                .Include(f => f.Product)
                .Include(f => f.Date)
                .Where(f => f.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
