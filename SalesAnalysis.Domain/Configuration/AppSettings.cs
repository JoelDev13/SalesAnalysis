namespace SalesAnalysis.Domain.Configuration
{
    public class AppSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string CsvBasePath { get; set; } = "Data/CSV";
    }
}
