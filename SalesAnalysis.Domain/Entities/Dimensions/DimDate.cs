using System;

namespace SalesAnalysis.Domain.Entities.Dimensions
{
    public class DimDate
    {
        public int DateKey { get; set; }
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int WeekOfYear { get; set; }
        public int DayOfYear { get; set; }
        public int DayOfMonth { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
        public string FiscalYear { get; set; }
        public int FiscalQuarter { get; set; }
        public int FiscalMonth { get; set; }
    }
}
