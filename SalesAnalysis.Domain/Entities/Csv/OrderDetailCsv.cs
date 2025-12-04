using CsvHelper.Configuration.Attributes;

namespace SalesAnalysis.Domain.Entities.Csv
{
    public class OrderDetailCsv
    {
        [Name("OrderID")]
        public int OrderId { get; set; }
        [Name("ProductID")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
