using CsvHelper.Configuration.Attributes;

namespace SalesAnalysis.Domain.Entities.Csv
{
    public class CustomerCsv
    {
        [Name("CustomerID")]
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
