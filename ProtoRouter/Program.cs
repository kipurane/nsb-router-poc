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
            var staticRouting = routerConfig.UseStaticRoutingProtocol();

//            UseAzureServiceBusTransport(config, routerConfig, staticRouting);
//            RouteFromLearningToAzure(routerConfig, config, staticRouting);
            RouteFromAzureToLearning(routerConfig, config, staticRouting);

            routerConfig.AutoCreateQueues();
            routerConfig.PoisonQueueName = $"{routerName}-poison";
            
            var router = Router.Create(routerConfig);
            await router.Start().ConfigureAwait(false);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();

            await router.Stop().ConfigureAwait(false);
        }

        private static void RouteFromAzureToLearning(RouterConfiguration routerConfig, IConfiguration config, RouteTable staticRouting)
        {
            routerConfig.AddInterface<LearningTransport>("LearningTransportA", t => { });
            var connectionStringB = config.GetConnectionString("NServiceBus:AzureServiceBusB");
            var azureServiceBusAInterface = routerConfig.AddInterface<AzureServiceBusTransport>("AzureServiceBusB", t => { t.ConnectionString(connectionStringB); });
            staticRouting.AddForwardRoute("AzureServiceBusB", "LearningTransportA");
        }

        private static void RouteFromLearningToAzure(RouterConfiguration routerConfig, IConfiguration config, RouteTable staticRouting)
        {
            routerConfig.AddInterface<LearningTransport>("LearningTransportA", t => { });
            var connectionStringB = config.GetConnectionString("NServiceBus:AzureServiceBusB");
            var azureServiceBusBInterface = routerConfig.AddInterface<AzureServiceBusTransport>("AzureServiceBusB", t => { t.ConnectionString(connectionStringB); });
            staticRouting.AddForwardRoute("LearningTransportA", "AzureServiceBusB");
        }

        private static void UseAzureServiceBusTransport(IConfiguration config, RouterConfiguration routerConfig, RouteTable staticRouting)
        {
            var connectionStringA = config.GetConnectionString("NServiceBus:AzureServiceBusA");
            var azureServiceBusAInterface = routerConfig.AddInterface<AzureServiceBusTransport>("AzureServiceBusA", t => { t.ConnectionString(connectionStringA); });

            var connectionStringB = config.GetConnectionString("NServiceBus:AzureServiceBusB");
            var azureServiceBusBInterface = routerConfig.AddInterface<AzureServiceBusTransport>("AzureServiceBusB", t => { t.ConnectionString(connectionStringB); });

            staticRouting.AddForwardRoute("AzureServiceBusA", "AzureServiceBusB");
            staticRouting.AddForwardRoute("AzureServiceBusB", "AzureServiceBusA");
        }
    }
}