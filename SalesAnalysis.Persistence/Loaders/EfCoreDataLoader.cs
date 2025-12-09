using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Persistence.Data;

namespace SalesAnalysis.Persistence.Loaders
{
    public class EfCoreDataLoader<T> : IDataLoader<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EfCoreDataLoader<T>> _logger;

        public EfCoreDataLoader(AppDbContext context, ILogger<EfCoreDataLoader<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> LoadAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
        {
            if (data is null)
            {
                return 0;
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting data load using EF Core for entity {EntityName}", typeof(T).Name);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var dataList = data.ToList();
                var totalAffected = 0;
                
                // PASO 3: Procesar en LOTES (batches) de 1000 registros
                const int batchSize = 1000;
                var batches = dataList.Chunk(batchSize);
                
                _logger.LogInformation("Procesando {Total} registros en {BatchCount} lotes de {BatchSize}", 
                    dataList.Count, batches.Count(), batchSize);
                
                foreach (var batch in batches)
                {
                    // Handle upserts for entities with potential duplicates
                    if (typeof(T) == typeof(OrderDetail))
                    {
                        var affected = await UpsertOrderDetailsAsync(batch.Cast<OrderDetail>(), cancellationToken);
                        totalAffected += affected;
                    }
                    else if (typeof(T) == typeof(Customer))
                    {
                        var affected = await UpsertCustomersAsync(batch.Cast<Customer>(), cancellationToken);
                        totalAffected += affected;
                    }
                    else if (typeof(T) == typeof(Product))
                    {
                        var affected = await UpsertProductsAsync(batch.Cast<Product>(), cancellationToken);
                        totalAffected += affected;
                    }
                    else if (typeof(T) == typeof(Order))
                    {
                        var affected = await UpsertOrdersAsync(batch.Cast<Order>(), cancellationToken);
                        totalAffected += affected;
                    }
                    else
                    {
                        // Para otras entidades, usar el método normal
                        await _context.Set<T>().AddRangeAsync(batch, cancellationToken);
                        var affected = await _context.SaveChangesAsync(cancellationToken);
                        totalAffected += affected;
                    }
                    
                    // PASO 4: Limpiar el ChangeTracker entre lotes
                    _context.ChangeTracker.Clear();
                    
                    _logger.LogInformation("Lote procesado: {BatchSize} registros, {Affected} afectados", batch.Count(), totalAffected);
                }
                
                await transaction.CommitAsync(cancellationToken);
                
                stopwatch.Stop();
                _logger.LogInformation("Data load completed. Total rows affected: {Affected}. Duration: {Elapsed} ms", totalAffected, stopwatch.ElapsedMilliseconds);
                return totalAffected;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error loading data for entity {EntityName}", typeof(T).Name);
                throw;
            }
        }

        private async Task<int> UpsertOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails, CancellationToken cancellationToken)
        {
            // PASO 1: Limpiar duplicados ANTES de insertar
            var uniqueOrderDetails = orderDetails
                .GroupBy(od => new { od.OrderId, od.ProductId })
                .Select(g => g.First())
                .ToList();
            
            _logger.LogInformation("OrderDetails: {Total} registros, {Unique} únicos después de limpiar duplicados", 
                orderDetails.Count(), uniqueOrderDetails.Count);

            var entitiesToAdd = new List<OrderDetail>();
            var entitiesToUpdate = new List<OrderDetail>();
            
            foreach (var orderDetail in uniqueOrderDetails)
            {
                try
                {
                    var existing = await _context.Set<OrderDetail>()
                        .FindAsync(new object[] { orderDetail.OrderId, orderDetail.ProductId }, cancellationToken);
                    
                    if (existing == null)
                    {
                        entitiesToAdd.Add(orderDetail);
                    }
                    else
                    {
                        // Update existing record
                        existing.Quantity = orderDetail.Quantity;
                        existing.TotalPrice = orderDetail.TotalPrice;
                        entitiesToUpdate.Add(existing);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing order detail {OrderId}/{ProductId}", orderDetail.OrderId, orderDetail.ProductId);
                }
            }
            
            // PASO 2: Usar AddRange y UpdateRange en batch
            if (entitiesToAdd.Any())
            {
                await _context.Set<OrderDetail>().AddRangeAsync(entitiesToAdd, cancellationToken);
            }
            
            if (entitiesToUpdate.Any())
            {
                _context.Set<OrderDetail>().UpdateRange(entitiesToUpdate);
            }
            
            var affected = await _context.SaveChangesAsync(cancellationToken);
            
            // PASO 4: Limpiar el ChangeTracker entre operaciones
            _context.ChangeTracker.Clear();
            
            return affected;
        }

        private async Task<int> UpsertCustomersAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken)
        {
            // PASO 1: Limpiar duplicados ANTES de insertar
            var uniqueCustomers = customers
                .GroupBy(c => c.CustomerId)
                .Select(g => g.First())
                .ToList();
            
            _logger.LogInformation("Customers: {Total} registros, {Unique} únicos después de limpiar duplicados", 
                customers.Count(), uniqueCustomers.Count);

            var entitiesToAdd = new List<Customer>();
            var entitiesToUpdate = new List<Customer>();
            
            foreach (var customer in uniqueCustomers)
            {
                try
                {
                    var existing = await _context.Set<Customer>()
                        .FindAsync(new object[] { customer.CustomerId }, cancellationToken);
                    
                    if (existing == null)
                    {
                        entitiesToAdd.Add(customer);
                    }
                    else
                    {
                        // Update existing record
                        existing.FirstName = customer.FirstName;
                        existing.LastName = customer.LastName;
                        existing.Email = customer.Email;
                        existing.Phone = customer.Phone;
                        existing.City = customer.City;
                        existing.Country = customer.Country;
                        entitiesToUpdate.Add(existing);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing customer {CustomerId}", customer.CustomerId);
                }
            }
            
            // PASO 2: Usar AddRange y UpdateRange en batch
            if (entitiesToAdd.Any())
            {
                await _context.Set<Customer>().AddRangeAsync(entitiesToAdd, cancellationToken);
            }
            
            if (entitiesToUpdate.Any())
            {
                _context.Set<Customer>().UpdateRange(entitiesToUpdate);
            }
            
            var affected = await _context.SaveChangesAsync(cancellationToken);
            
            // PASO 4: Limpiar el ChangeTracker entre operaciones
            _context.ChangeTracker.Clear();
            
            return affected;
        }

        private async Task<int> UpsertProductsAsync(IEnumerable<Product> products, CancellationToken cancellationToken)
        {
            // PASO 1: Limpiar duplicados ANTES de insertar
            var uniqueProducts = products
                .GroupBy(p => p.ProductId)
                .Select(g => g.First())
                .ToList();
            
            _logger.LogInformation("Products: {Total} registros, {Unique} únicos después de limpiar duplicados", 
                products.Count(), uniqueProducts.Count);

            var entitiesToAdd = new List<Product>();
            var entitiesToUpdate = new List<Product>();
            
            foreach (var product in uniqueProducts)
            {
                try
                {
                    var existing = await _context.Set<Product>()
                        .FindAsync(new object[] { product.ProductId }, cancellationToken);
                    
                    if (existing == null)
                    {
                        entitiesToAdd.Add(product);
                    }
                    else
                    {
                        // Update existing record
                        existing.ProductName = product.ProductName;
                        existing.Category = product.Category;
                        existing.Price = product.Price;
                        existing.Stock = product.Stock;
                        entitiesToUpdate.Add(existing);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing product {ProductId}", product.ProductId);
                }
            }
            
            // PASO 2: Usar AddRange y UpdateRange en batch
            if (entitiesToAdd.Any())
            {
                await _context.Set<Product>().AddRangeAsync(entitiesToAdd, cancellationToken);
            }
            
            if (entitiesToUpdate.Any())
            {
                _context.Set<Product>().UpdateRange(entitiesToUpdate);
            }
            
            var affected = await _context.SaveChangesAsync(cancellationToken);
            
            // PASO 4: Limpiar el ChangeTracker entre operaciones
            _context.ChangeTracker.Clear();
            
            return affected;
        }

        private async Task<int> UpsertOrdersAsync(IEnumerable<Order> orders, CancellationToken cancellationToken)
        {
            // PASO 1: Limpiar duplicados ANTES de insertar
            var uniqueOrders = orders
                .GroupBy(o => o.OrderId)
                .Select(g => g.First())
                .ToList();
            
            _logger.LogInformation("Orders: {Total} registros, {Unique} únicos después de limpiar duplicados", 
                orders.Count(), uniqueOrders.Count);

            var entitiesToAdd = new List<Order>();
            var entitiesToUpdate = new List<Order>();
            
            foreach (var order in uniqueOrders)
            {
                try
                {
                    var existing = await _context.Set<Order>()
                        .FindAsync(new object[] { order.OrderId }, cancellationToken);
                    
                    if (existing == null)
                    {
                        entitiesToAdd.Add(order);
                    }
                    else
                    {
                        // Update existing record
                        existing.CustomerId = order.CustomerId;
                        existing.OrderDate = order.OrderDate;
                        existing.Status = order.Status;
                        entitiesToUpdate.Add(existing);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing order {OrderId}", order.OrderId);
                }
            }
            
            // PASO 2: Usar AddRange y UpdateRange en batch
            if (entitiesToAdd.Any())
            {
                await _context.Set<Order>().AddRangeAsync(entitiesToAdd, cancellationToken);
            }
            
            if (entitiesToUpdate.Any())
            {
                _context.Set<Order>().UpdateRange(entitiesToUpdate);
            }
            
            var affected = await _context.SaveChangesAsync(cancellationToken);
            
            // PASO 4: Limpiar el ChangeTracker entre operaciones
            _context.ChangeTracker.Clear();
            
            return affected;
        }
    }
}
