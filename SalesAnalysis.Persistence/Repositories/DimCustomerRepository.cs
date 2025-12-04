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
    public class DimCustomerRepository : DimensionRepository<DimCustomer>, IDimCustomerRepository
    {
        public DimCustomerRepository(AppDbContext context, ILogger<DimCustomerRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<DimCustomer> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }

        public async Task<IEnumerable<DimCustomer>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimCustomer>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimCustomer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
