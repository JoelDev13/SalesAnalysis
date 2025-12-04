using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Csv;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface ICustomerEtlService
    {
        Task<CustomerEtlResult> ProcessCustomersAsync(IEnumerable<CustomerCsv> customers, CancellationToken cancellationToken = default);
    }
}
