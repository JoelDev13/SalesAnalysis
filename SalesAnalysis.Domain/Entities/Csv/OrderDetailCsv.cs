namespace SalesAnalysis.Domain.Entities.Csv
{
    public class OrderDetailCsv
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
