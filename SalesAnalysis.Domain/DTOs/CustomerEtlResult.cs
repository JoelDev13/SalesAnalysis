using System;

namespace SalesAnalysis.Domain.DTOs
{
    public class CustomerEtlResult
    {
        public int ProcessedCount { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public string[] ValidationErrors { get; set; } = new string[0];
        public TimeSpan ProcessingTime { get; set; }
        public bool IsSuccess => ValidationErrors.Length == 0;
    }
}
