using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Services
{
    public interface ITableCleaningService
    {
        Task CleanFactTablesAsync(CancellationToken cancellationToken = default);
        Task CleanDimensionTablesAsync(CancellationToken cancellationToken = default);
        Task CleanAllTablesAsync(CancellationToken cancellationToken = default);
    }

    public class TableCleaningService : ITableCleaningService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TableCleaningService> _logger;

        public TableCleaningService(AppDbContext context, ILogger<TableCleaningService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CleanFactTablesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting fact tables cleaning process");

            try
            {
                // Limpiar FactSales
                await CleanTableAsync("FactSales", cancellationToken);
                
                _logger.LogInformation("Fact tables cleaning completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during fact tables cleaning");
                throw;
            }
        }

        public async Task CleanDimensionTablesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting dimension tables cleaning process");

            try
            {
                // Limpiar tablas de dimensiones
                await CleanTableAsync("DimCustomers", cancellationToken);
                await CleanTableAsync("DimProducts", cancellationToken);
                await CleanTableAsync("DimDates", cancellationToken);
                
                _logger.LogInformation("Dimension tables cleaning completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during dimension tables cleaning");
                throw;
            }
        }

        public async Task CleanAllTablesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting complete tables cleaning process");

            try
            {
                // Limpiar tablas de hechos primero (por dependencias)
                await CleanFactTablesAsync(cancellationToken);
                
                // Luego limpiar tablas de dimensiones
                await CleanDimensionTablesAsync(cancellationToken);
                
                // Finalmente limpiar tablas de origen
                await CleanSourceTablesAsync(cancellationToken);
                
                _logger.LogInformation("Complete tables cleaning completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during complete tables cleaning");
                throw;
            }
        }

        private async Task CleanTableAsync(string tableName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cleaning table: {TableName}", tableName);

            try
            {
                // Usar TRUNCATE si es posible, sino DELETE
                var canTruncate = await CanTruncateTableAsync(tableName, cancellationToken);
                
                if (canTruncate)
                {
                    await _context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [{tableName}]", cancellationToken);
                    _logger.LogInformation("Table {TableName} truncated successfully", tableName);
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]", cancellationToken);
                    _logger.LogInformation("Table {TableName} cleaned with DELETE successfully", tableName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning table {TableName}", tableName);
                throw;
            }
        }

        private async Task CleanSourceTablesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cleaning source tables");

            try
            {
                // Limpiar OrderDetails primero (sin dependencias)
                await CleanTableAsync("OrderDetails", cancellationToken);
                
                // Luego limpiar Orders
                await CleanTableAsync("Orders", cancellationToken);
                
                // Finalmente limpiar Products y Customers
                await CleanTableAsync("Products", cancellationToken);
                await CleanTableAsync("Customers", cancellationToken);
                
                _logger.LogInformation("Source tables cleaning completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during source tables cleaning");
                throw;
            }
        }

        private async Task<bool> CanTruncateTableAsync(string tableName, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si la tabla tiene foreign keys
                var hasForeignKeys = await _context.Database.SqlQueryRaw<int>(
                    @"SELECT COUNT(*) FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID(@tableName)",
                    new { tableName }
                ).FirstOrDefaultAsync(cancellationToken);

                return hasForeignKeys == 0;
            }
            catch
            {
                // Si no podemos verificar, asumimos que no se puede truncar
                return false;
            }
        }
    }
}
