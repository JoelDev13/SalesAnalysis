using Microsoft.Extensions.DependencyInjection;

namespace SalesAnalysis.Domain.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddDomainLayer(this IServiceCollection services)
        {
            return services;
        }
    }
}
