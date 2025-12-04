using SalesAnalysis.Domain.Entities.Dimensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces.Repositories
{
    public interface IDimProductRepository : IDimensionRepository<DimProduct>
    {
        Task<DimProduct> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimProduct>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimProduct>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimProduct>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<DimProduct>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default);
    }
}
