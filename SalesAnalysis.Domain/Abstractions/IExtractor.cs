namespace SalesAnalysis.Domain.Abstractions;

public interface IExtractor<T>
{
    Task<IEnumerable<T>> ExtractAsync(CancellationToken cancellationToken = default);
    string SourceName { get; }
}
