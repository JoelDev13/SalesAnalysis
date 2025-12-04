using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.DTOs;

namespace SalesAnalysis.Domain.Interfaces
{
    public interface IDimensionEtlService
    {
        Task<DimensionEtlResult> ProcessCustomerDimensionsAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken = default);
        Task<DimensionEtlResult> ProcessProductDimensionsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
        Task<DimensionEtlResult> ProcessDateDimensionsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
