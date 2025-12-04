using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Dimensions;
using SalesAnalysis.Domain.Interfaces.Repositories;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Repositories
{
    public class DimProductRepository : DimensionRepository<DimProduct>, IDimProductRepository
    {
        public DimProductRepository(AppDbContext context, ILogger<DimProductRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<DimProduct> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        public async Task<IEnumerable<DimProduct>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimProduct>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimProduct>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimProduct>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Stock <= threshold && p.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
