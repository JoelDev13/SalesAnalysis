using SalesAnalysis.Domain.Entities.Dimensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesAnalysis.Domain.Interfaces.Repositories
{
    public interface IDimDateRepository : IDimensionRepository<DimDate>
    {
        Task<DimDate> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimDate>> GetByYearAsync(int year, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimDate>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimDate>> GetByQuarterAsync(int year, int quarter, CancellationToken cancellationToken = default);
        Task<IEnumerable<DimDate>> GetHolidaysAsync(CancellationToken cancellationToken = default);
        Task<int> BulkInsertDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
