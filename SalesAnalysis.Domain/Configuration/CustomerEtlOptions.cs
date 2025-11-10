using System;
using System.IO;

namespace SalesAnalysis.Domain.Configuration
{
    public class CustomerEtlOptions
    {
        public string CsvFilePath { get; set; } = string.Empty;
        public string DatabaseConnectionString { get; set; } = string.Empty;
        public string DatabaseQuery { get; set; } = string.Empty;
        public string ApiClientName { get; set; } = CustomerEtlOptionsDefaults.ApiClientName;
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public bool EnableCsvSource { get; set; } = true;
        public bool EnableDatabaseSource { get; set; } = true;
        public bool EnableApiSource { get; set; } = true;
        public string StagingDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "staging");
        public int RunIntervalMinutes { get; set; } = 60;
    }
}
