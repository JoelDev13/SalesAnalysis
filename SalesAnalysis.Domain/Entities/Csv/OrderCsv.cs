using CsvHelper.Configuration.Attributes;

namespace SalesAnalysis.Domain.Entities.Csv
{
    public class OrderCsv
    {
        [Name("OrderID")]
        public int OrderId { get; set; }
        [Name("CustomerID")]
        public int CustomerId { get; set; }
        [Name("OrderDate")]
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
