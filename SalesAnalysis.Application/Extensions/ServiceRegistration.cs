using Microsoft.Extensions.DependencyInjection;
using SalesAnalysis.Application.Interfaces;
using SalesAnalysis.Application.Services;

namespace SalesAnalysis.Application.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
            return services;
        }
    }
}
