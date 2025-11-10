namespace SalesAnalysis.Domain.Abstractions;

public interface ITransformer<TSource, TDestination>
{
    Task<IEnumerable<TDestination>> TransformAsync(IEnumerable<TSource> source, CancellationToken cancellationToken = default);
}
