using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces.Repositories;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Repositories
{
    public abstract class DimensionRepository<T> : IDimensionRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly ILogger<DimensionRepository<T>> _logger;
        protected readonly DbSet<T> _dbSet;

        protected DimensionRepository(AppDbContext context, ILogger<DimensionRepository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<T> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
        {
            
            throw new NotImplementedException("Hash-based lookup is not implemented. Use CustomerId/ProductId instead.");
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) return 0;

            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) return 0;

            _dbSet.UpdateRange(entities);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> BulkUpsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) return 0;

            var entityList = entities.ToList();
            if (!entityList.Any()) return 0;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var insertedCount = 0;
                var updatedCount = 0;

                foreach (var entity in entityList)
                {
                    // Use CustomerId/ProductId for upsert logic instead of hash
                    var idProperty = typeof(T).GetProperties()
                        .FirstOrDefault(p => p.Name.Equals("CustomerId", StringComparison.OrdinalIgnoreCase) || 
                                           p.Name.Equals("ProductId", StringComparison.OrdinalIgnoreCase));
                    
                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(entity);
                        var keyProperty = typeof(T).GetProperties()
                            .FirstOrDefault(p => p.Name.EndsWith("Key", StringComparison.OrdinalIgnoreCase));
                        
                        var existing = await _dbSet.FindAsync(new object[] { idValue }, cancellationToken);
                        if (existing != null)
                        {
                            if (keyProperty != null)
                            {
                                keyProperty.SetValue(entity, keyProperty.GetValue(existing));
                                _context.Entry(existing).CurrentValues.SetValues(entity);
                                updatedCount++;
                            }
                        }
                        else
                        {
                            await _dbSet.AddAsync(entity, cancellationToken);
                            insertedCount++;
                        }
                    }
                    else
                    {
                        await _dbSet.AddAsync(entity, cancellationToken);
                        insertedCount++;
                    }
                }

                var totalAffected = await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Bulk upsert completed: {Inserted} inserted, {Updated} updated, {Total} total affected", 
                    insertedCount, updatedCount, totalAffected);

                return totalAffected;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error during bulk upsert operation for entity {EntityName}", typeof(T).Name);
                throw;
            }
        }
    }
}
