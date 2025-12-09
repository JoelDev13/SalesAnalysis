using SalesAnalysis.Application.Extensions;
using SalesAnalysis.Domain.Extensions;
using SalesAnalysis.Persistence.Extensions;
using SalesAnalysis.Worker;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .AddDomainLayer()
            .AddPersistenceLayer(context.Configuration)
            .AddApplicationLayer();

        services.AddHostedService<Worker>();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();
