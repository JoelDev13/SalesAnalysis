namespace SalesAnalysis.Domain.Abstractions;

public interface IDataLoader<T>
{
    Task<int> LoadAsync(IEnumerable<T> data, CancellationToken cancellationToken = default);
}
