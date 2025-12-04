using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces.Repositories
{
    public interface IDimensionRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<T> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkUpsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    }
}
