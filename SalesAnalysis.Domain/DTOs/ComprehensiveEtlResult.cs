using System;

namespace SalesAnalysis.Domain.DTOs
{
    public class ComprehensiveEtlResult
    {
        public int CustomersProcessed { get; set; }
        public int ProductsProcessed { get; set; }
        public int OrdersProcessed { get; set; }
        public int OrderDetailsProcessed { get; set; }
        public int DimCustomersProcessed { get; set; }
        public int DimProductsProcessed { get; set; }
        public int DimDatesProcessed { get; set; }
        public string[] Errors { get; set; } = new string[0];
        public TimeSpan TotalTime { get; set; }
        public bool IsSuccess => Errors.Length == 0;
    }
}
