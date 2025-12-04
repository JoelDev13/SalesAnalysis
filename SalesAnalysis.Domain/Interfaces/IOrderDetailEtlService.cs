using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Csv;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IOrderDetailEtlService
    {
        Task<OrderDetailEtlResult> ProcessOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails, CancellationToken cancellationToken = default);
    }
}
