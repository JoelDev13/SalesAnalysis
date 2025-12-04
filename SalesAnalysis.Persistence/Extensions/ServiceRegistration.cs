using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SalesAnalysis.Domain.Configuration;
using SalesAnalysis.Domain.Factories;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Domain.Interfaces.Repositories;
using SalesAnalysis.Persistence.Data;
using SalesAnalysis.Persistence.Extractors;
using SalesAnalysis.Persistence.Factories;
using SalesAnalysis.Persistence.Loaders;
using SalesAnalysis.Persistence.Repositories;
using SalesAnalysis.Persistence.Services;
using SalesAnalysis.Persistence.Transformers;

namespace SalesAnalysis.Persistence.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Options
            services.Configure<CustomerEtlOptions>(configuration.GetSection("CustomerEtl"));

            services.AddHttpClient();
            services.AddHttpClient(CustomerEtlOptionsDefaults.ApiClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CustomerEtlOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.ApiBaseUrl))
                {
                    client.BaseAddress = new Uri(options.ApiBaseUrl);
                }
            });
            services.AddSingleton<IExtractorFactory, ExtractorFactory>();

            services.AddTransient(typeof(CsvExtractor<>));
            services.AddTransient(typeof(DatabaseExtractor<>));
            services.AddTransient(typeof(ApiExtractor<>));

            // Repositorio
            services.AddScoped<ICustomerReadRepository, CustomerRepository>();
            services.AddSingleton<CustomerTransformer>();
            services.AddSingleton<ITransformer<Domain.Entities.Csv.CustomerCsv, Domain.Entities.Db.Customer>>(sp => sp.GetRequiredService<CustomerTransformer>());
            services.AddSingleton<ITransformer<Domain.Entities.Api.CustomerApiResponse, Domain.Entities.Db.Customer>>(sp => sp.GetRequiredService<CustomerTransformer>());

            // Data loaders
            services.AddScoped<IDataLoader<Domain.Entities.Db.Customer>, EfCoreDataLoader<Domain.Entities.Db.Customer>>();
            services.AddScoped<IDataLoader<Domain.Entities.Db.Product>, EfCoreDataLoader<Domain.Entities.Db.Product>>();
            services.AddScoped<IDataLoader<Domain.Entities.Db.Order>, EfCoreDataLoader<Domain.Entities.Db.Order>>();
            services.AddScoped<IDataLoader<Domain.Entities.Db.OrderDetail>, EfCoreDataLoader<Domain.Entities.Db.OrderDetail>>();

            // Dimension repositories
            services.AddScoped<IDimCustomerRepository, DimCustomerRepository>();
            services.AddScoped<IDimProductRepository, DimProductRepository>();
            services.AddScoped<IDimDateRepository, DimDateRepository>();

            // Dimension loader
            services.AddScoped<DimensionLoader>();

            // Dimension ETL service
            services.AddScoped<IDimensionEtlService, DimensionEtlService>();

            // Additional ETL services
            services.AddScoped<ICustomerEtlService, CustomerEtlService>();
            services.AddScoped<IProductEtlService, ProductEtlService>();
            services.AddScoped<IOrderEtlService, OrderEtlService>();
            services.AddScoped<IOrderDetailEtlService, OrderDetailEtlService>();

            // Comprehensive ETL service
            services.AddScoped<IComprehensiveEtlService, ComprehensiveEtlService>();

            // Logger service
            services.AddSingleton<ILoggerService, StandardLoggerService>();
            services.AddSingleton<IStagingWriter, StagingFileWriter>();

            // ETL service
            services.AddScoped<IEtlService, EtlService>();

            return services;
        }
    }
}
