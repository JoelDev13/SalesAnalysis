using Microsoft.Extensions.DependencyInjection;
using SalesAnalysis.Domain.Interfaces;
using SalesAnalysis.Application.Services;

namespace SalesAnalysis.Application.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
         
            services.AddScoped<DimensionLoader>();

          
            services.AddScoped<IDimensionEtlService, DimensionEtlService>();

          
            services.AddScoped<ICustomerEtlService, CustomerEtlService>();
            services.AddScoped<IProductEtlService, ProductEtlService>();
            services.AddScoped<IOrderEtlService, OrderEtlService>();
            services.AddScoped<IOrderDetailEtlService, OrderDetailEtlService>();

            
            services.AddScoped<IComprehensiveEtlService, ComprehensiveEtlService>();

            services.AddSingleton<ILoggerService, StandardLoggerService>();
            services.AddSingleton<IStagingWriter, StagingFileWriter>();

            services.AddScoped<IEtlService, EtlService>();

            return services;
        }
    }
}
