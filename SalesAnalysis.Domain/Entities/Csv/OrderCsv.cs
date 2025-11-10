namespace SalesAnalysis.Domain.Entities.Csv
{
    public class OrderCsv
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
