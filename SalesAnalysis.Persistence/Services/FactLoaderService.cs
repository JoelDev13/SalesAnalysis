using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Entities.Dimensions;
using SalesAnalysis.Domain.Entities.Facts;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Domain.Interfaces.Repositories;

namespace SalesAnalysis.Persistence.Services
{
    public class FactLoaderService : IFactLoader<FactSales>
    {
        private readonly IFactSalesRepository _factSalesRepository;
        private readonly IDimCustomerRepository _customerRepository;
        private readonly IDimProductRepository _productRepository;
        private readonly IDimDateRepository _dateRepository;
        private readonly ITableCleaningService _cleaningService;
        private readonly ILogger<FactLoaderService> _logger;

        public FactLoaderService(
            IFactSalesRepository factSalesRepository,
            IDimCustomerRepository customerRepository,
            IDimProductRepository productRepository,
            IDimDateRepository dateRepository,
            ITableCleaningService cleaningService,
            ILogger<FactLoaderService> logger)
        {
            _factSalesRepository = factSalesRepository ?? throw new ArgumentNullException(nameof(factSalesRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _dateRepository = dateRepository ?? throw new ArgumentNullException(nameof(dateRepository));
            _cleaningService = cleaningService ?? throw new ArgumentNullException(nameof(cleaningService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> LoadFactsAsync(IEnumerable<FactSales> sourceData, CancellationToken cancellationToken = default)
        {
            if (sourceData == null) return 0;

            var factList = sourceData.ToList();
            if (!factList.Any()) return 0;

            _logger.LogInformation("Iniciando proceso de carga de hechos de ventas para {Count} registros", factList.Count);

            try
            {
                // Paso 1: Limpiar tablas de hechos antes de cargar
                _logger.LogInformation("Limpiando tablas de hechos antes de la carga");
                await _cleaningService.CleanFactTablesAsync(cancellationToken);
                _logger.LogInformation("Limpieza de tablas de hechos completada");

                // Paso 2: Procesar y cargar los hechos
                foreach (var fact in factList)
                {
                    fact.CreatedDate = DateTime.UtcNow;
                    fact.ModifiedDate = DateTime.UtcNow;
                    fact.IsActive = true;
                }

                var validFacts = new List<FactSales>();
                var invalidCount = 0;

                foreach (var fact in factList)
                {
                    try
                    {
                        var customerDim = await _customerRepository.GetByCustomerIdAsync(fact.CustomerId, cancellationToken);
                        var productDim = await _productRepository.GetByProductIdAsync(fact.ProductId, cancellationToken);
                        
                        // Convertir DateId (int) a DateTime para el repositorio
                        var dateValue = DateTime.ParseExact(fact.DateId.ToString(), "yyyyMMdd", null);
                        var dateDim = await _dateRepository.GetByDateAsync(dateValue, cancellationToken);

                        if (customerDim != null && productDim != null && dateDim != null)
                        {
                            var transformedFact = new FactSales
                            {
                                CustomerId = customerDim.CustomerKey,
                                ProductId = productDim.ProductKey,
                                DateId = dateDim.DateKey,
                                OrderId = fact.OrderId,
                                Quantity = fact.Quantity,
                                UnitPrice = fact.UnitPrice,
                                TotalAmount = fact.TotalAmount,
                                DiscountAmount = fact.DiscountAmount,
                                FinalAmount = fact.FinalAmount,
                                OrderStatus = fact.OrderStatus,
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                IsActive = true
                            };
                            validFacts.Add(transformedFact);
                        }
                        else
                        {
                            invalidCount++;
                            _logger.LogWarning("Hecho inválido: CustomerId={CustomerId}, ProductId={ProductId}, DateId={DateId}", 
                                fact.CustomerId, fact.ProductId, fact.DateId);
                        }
                    }
                    catch (Exception ex)
                    {
                        invalidCount++;
                        _logger.LogError(ex, "Error procesando hecho con CustomerId={CustomerId}, ProductId={ProductId}", 
                            fact.CustomerId, fact.ProductId);
                    }
                }

                if (validFacts.Any())
                {
                    await _factSalesRepository.BulkInsertAsync(validFacts, cancellationToken);
                    _logger.LogInformation("Se cargaron exitosamente {Count} hechos de ventas", validFacts.Count);
                }

                if (invalidCount > 0)
                {
                    _logger.LogWarning("Se ignoraron {Count} hechos inválidos", invalidCount);
                }

                return validFacts.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la carga de hechos de ventas");
                throw;
            }
        }

        public async Task<int> UpsertFactsAsync(IEnumerable<FactSales> sourceData, CancellationToken cancellationToken = default)
        {
            return await LoadFactsAsync(sourceData, cancellationToken);
        }

        public async Task<bool> CleanFactTableAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Iniciando proceso de limpieza de tabla FactSales");
            
            try
            {
                var result = await _factSalesRepository.TruncateTableAsync(cancellationToken);
                
                if (result)
                {
                    _logger.LogInformation("Tabla FactSales limpiada exitosamente");
                }
                else
                {
                    _logger.LogWarning("Falló el TRUNCATE, intentando eliminación completa");
                    result = await _factSalesRepository.DeleteAllAsync(cancellationToken);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza de la tabla FactSales");
                return false;
            }
        }

        public async Task<bool> CleanFactTableByDateRangeAsync(int startDateId, int endDateId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando limpieza completa de tabla FactSales para rango de fechas {StartDateId} a {EndDateId}", startDateId, endDateId);
            
            try
            {
                var result = await _factSalesRepository.TruncateTableAsync(cancellationToken);
                
                if (!result)
                {
                    _logger.LogWarning("Falló el TRUNCATE, intentando eliminación completa");
                    result = await _factSalesRepository.DeleteAllAsync(cancellationToken);
                }
                
                if (result)
                {
                    _logger.LogInformation("Tabla FactSales limpiada completamente para el rango de fechas {StartDateId} a {EndDateId}", startDateId, endDateId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza completa de la tabla FactSales para rango de fechas {StartDateId} a {EndDateId}", startDateId, endDateId);
                return false;
            }
        }

        public async Task<bool> ValidateFactDataAsync(IEnumerable<FactSales> data, CancellationToken cancellationToken = default)
        {
            var factList = data.ToList();
            var validationErrors = new List<string>();

            foreach (var fact in factList)
            {
                if (fact.CustomerId <= 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: CustomerId es requerido y debe ser mayor a 0");

                if (fact.ProductId <= 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: ProductId es requerido y debe ser mayor a 0");

                if (fact.DateId <= 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: DateId es requerido y debe ser mayor a 0");

                if (fact.Quantity <= 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: Quantity debe ser mayor a 0");

                if (fact.UnitPrice <= 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: UnitPrice debe ser mayor a 0");

                if (fact.TotalAmount <= 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: TotalAmount debe ser mayor a 0");

                if (fact.FinalAmount < 0)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: FinalAmount no puede ser negativo");

                var calculatedTotal = fact.Quantity * fact.UnitPrice;
                if (Math.Abs(calculatedTotal - fact.TotalAmount) > 0.01m)
                    validationErrors.Add($"Hecho {fact.FactSalesId}: TotalAmount ({fact.TotalAmount}) no coincide con Quantity * UnitPrice ({calculatedTotal})");
            }

            if (validationErrors.Any())
            {
                _logger.LogWarning("Validación de hechos de ventas falló: {Errors}", string.Join("; ", validationErrors));
                return false;
            }

            return true;
        }

        public async Task<FactSales> TransformOrderToFactSales(Order order, OrderDetail orderDetail, CancellationToken cancellationToken = default)
        {
            if (order == null || orderDetail == null) return null;

            var customerDim = await _customerRepository.GetByCustomerIdAsync(order.CustomerId, cancellationToken);
            var productDim = await _productRepository.GetByProductIdAsync(orderDetail.ProductId, cancellationToken);
            var dateDim = await _dateRepository.GetByDateAsync(order.OrderDate, cancellationToken);

            if (customerDim == null || productDim == null || dateDim == null)
            {
                _logger.LogWarning("No se encontraron dimensiones para OrderId: {OrderId}, ProductId: {ProductId}", order.OrderId, orderDetail.ProductId);
                return null;
            }

            var discountAmount = 0m;
            var finalAmount = orderDetail.TotalPrice - discountAmount;

            return new FactSales
            {
                CustomerId = customerDim.CustomerKey,
                ProductId = productDim.ProductKey,
                DateId = dateDim.DateKey,
                OrderId = order.OrderId,
                Quantity = orderDetail.Quantity,
                UnitPrice = orderDetail.TotalPrice / orderDetail.Quantity,
                TotalAmount = orderDetail.TotalPrice,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                OrderStatus = order.Status,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        public async Task<int> LoadFactsWithCleaningAsync(IEnumerable<FactSales> sourceData, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando proceso completo de carga con limpieza previa");

            var cleaningResult = await CleanFactTableAsync(cancellationToken);
            if (!cleaningResult)
            {
                _logger.LogError("Falló la limpieza de la tabla, cancelando carga");
                return 0;
            }

            var validationResult = await ValidateFactDataAsync(sourceData, cancellationToken);
            if (!validationResult)
            {
                _logger.LogError("Validación de datos fallida, cancelando carga");
                return 0;
            }

            return await LoadFactsAsync(sourceData, cancellationToken);
        }
    }
}
