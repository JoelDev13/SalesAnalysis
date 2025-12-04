using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Dimensions;
using SalesAnalysis.Domain.Interfaces.Repositories;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Repositories
{
    public class DimDateRepository : DimensionRepository<DimDate>, IDimDateRepository
    {
        public DimDateRepository(AppDbContext context, ILogger<DimDateRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<DimDate> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var dateKey = date.Year * 10000 + date.Month * 100 + date.Day;
            return await _dbSet
                .FirstOrDefaultAsync(d => d.DateKey == dateKey, cancellationToken);
        }

        public async Task<IEnumerable<DimDate>> GetByYearAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.Year == year)
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimDate>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.Year == year && d.Month == month)
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimDate>> GetByQuarterAsync(int year, int quarter, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.Year == year && d.Quarter == quarter)
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DimDate>> GetHolidaysAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.IsHoliday)
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> BulkInsertDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var dates = new List<DimDate>();
            var current = startDate;

            while (current <= endDate)
            {
                dates.Add(CreateDimDateFromDateTime(current));
                current = current.AddDays(1);
            }

            return await BulkInsertAsync(dates, cancellationToken);
        }

        private DimDate CreateDimDateFromDateTime(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var dateKey = date.Year * 10000 + date.Month * 100 + date.Day;

            return new DimDate
            {
                DateKey = dateKey,
                Date = date,
                Year = date.Year,
                Quarter = (date.Month - 1) / 3 + 1,
                Month = date.Month,
                MonthName = date.ToString("MMMM"),
                WeekOfYear = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                DayOfYear = date.DayOfYear,
                DayOfMonth = date.Day,
                DayOfWeek = (int)date.DayOfWeek,
                DayName = date.DayOfWeek.ToString(),
                IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                IsHoliday = false
            };
        }
    }
}
