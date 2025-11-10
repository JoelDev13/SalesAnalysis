using SalesAnalysis.Domain.Abstractions;

namespace SalesAnalysis.Domain.Repository;

public interface IRepository<T> where T : class
{
    Task<int> SaveAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
