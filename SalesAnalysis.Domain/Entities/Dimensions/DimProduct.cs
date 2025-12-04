using System;

namespace SalesAnalysis.Domain.Entities.Dimensions
{
    public class DimProduct
    {
        public int ProductKey { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Brand { get; set; }
        public string SKU { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
