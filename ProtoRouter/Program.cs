namespace ProtoRouter
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Extensions.Configuration;
    using NServiceBus;
    using NServiceBus.Router;

    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var config = ConfigurationCreator.GetConfiguration();
            var routerName = config["NServiceBus:EndpointName"];
            
            Console.Title = routerName;
            
            var routerConfig = new RouterConfiguration(routerName);

            var connectionStringA = config.GetConnectionString("NServiceBus:AzureServiceBusA");
            var connectionStringB = config.GetConnectionString("NServiceBus:AzureServiceBusB");
            
            var azureServiceBusAInterface = routerConfig.AddInterface<AzureServiceBusTransport>("AzureServiceBusA", t =>
            {
                t.ConnectionString(connectionStringA);
            });
            var azureServiceBusBInterface = routerConfig.AddInterface<AzureServiceBusTransport>("AzureServiceBusB", t =>
            {
                t.ConnectionString(connectionStringB);
            });
            
            var staticRouting = routerConfig.UseStaticRoutingProtocol();
            staticRouting.AddForwardRoute("AzureServiceBusA", "AzureServiceBusB");
            staticRouting.AddForwardRoute("AzureServiceBusB", "AzureServiceBusA");
            
            routerConfig.AutoCreateQueues();
            routerConfig.PoisonQueueName = $"{routerName}-poison";
            
            var router = Router.Create(routerConfig);
            await router.Start().ConfigureAwait(false);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();

            await router.Stop().ConfigureAwait(false);
        }
    }
}