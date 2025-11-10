using System;
using System.Data;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Domain.Factories
{
    public interface IExtractorFactory
    {
        IExtractor<T> CreateCsvExtractor<T>(string filePath) where T : class;
        IExtractor<T> CreateDatabaseExtractor<T>(string connectionString, string query, Func<IDataRecord, T> mapper) where T : class;
        IExtractor<T> CreateApiExtractor<T>(string clientName, string endpoint) where T : class;
    }
}
