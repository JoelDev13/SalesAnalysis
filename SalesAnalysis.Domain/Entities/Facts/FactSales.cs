using System;
using SalesAnalysis.Domain.Entities.Dimensions;

namespace SalesAnalysis.Domain.Entities.Facts
{
    public class FactSales
    {
        public int FactSalesId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int DateId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual DimCustomer Customer { get; set; }
        public virtual DimProduct Product { get; set; }
        public virtual DimDate Date { get; set; }
    }
}
