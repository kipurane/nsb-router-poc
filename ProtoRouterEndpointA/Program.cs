namespace ProtoRouterEndpointA
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NServiceBus;
    using NServiceBus.Logging;
    using NServiceBus.Serilog;
    using Serilog;
    using Shared.Commands.V1;

    internal static class Program
    {
        private static EndpointConfiguration endpointConfiguration;
        private static IConfiguration configuration;

        private static async Task Main(string[] args)
        {
            configuration = ConfigurationCreator.GetConfiguration();
            endpointConfiguration = new EndpointConfiguration(configuration["NServiceBus:EndpointName"]);

            ConfigureTransportAndRouting();
//            UserLearningTransport();

            ConfigureSerialization();
            ConfigureRecoverability();
            ConfigureLogging();
            
            endpointConfiguration.EnableInstallers();
            
            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Press <enter> to exit.");
            Console.ReadLine();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }

        private static void UserLearningTransport()
        {
            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            var bridge = transport.Routing().ConnectToRouter("proto-router");
            bridge.RouteToEndpoint(typeof(SendCommandToEndpointB), "proto-router-endpoint-b");
        }

        private static void ConfigureTransportAndRouting(
            Action<TransportExtensions<AzureServiceBusTransport>> messageEndpointMappings = null)
        {
            var connectionString = configuration.GetConnectionString("NServiceBus:AzureServiceBusTransport");
            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(connectionString);
            messageEndpointMappings?.Invoke(transport);

            var bridge = transport.Routing().ConnectToRouter("proto-router");
            bridge.RouteToEndpoint(typeof(SendCommandToEndpointB), "proto-router-endpoint-b");

            TransactionMode();
            void TransactionMode()
            {
                var transportTransactionMode =
                    configuration.GetValue("NServiceBus:Transport:TransactionMode", "default");
                if (transportTransactionMode.ToLower() == "receiveonly")
                    transport.Transactions(TransportTransactionMode.ReceiveOnly);
            }
        }

        private static void ConfigureSerialization()
        {
            var serialization = endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            serialization.WriterCreator(stream =>
            {
                var streamWriter = new StreamWriter(stream, new UTF8Encoding(false));
                return new JsonTextWriter(streamWriter)
                {
                    Formatting = Formatting.Indented
                };
            });
        }
        
        private static void ConfigureRecoverability()
        {
            var auditQueue = configuration["NServiceBus:AuditQueue"];
            var errorQueue = configuration["NServiceBus:ErrorQueue"];
            endpointConfiguration.AuditProcessedMessagesTo(auditQueue);
            endpointConfiguration.SendFailedMessagesTo(errorQueue);

            var recoverability = endpointConfiguration.Recoverability();

            var immediateRetries = int.Parse(configuration["NServiceBus:Retries:ImmediateRetriesAmount"]);
            recoverability.Immediate(immediate => { immediate.NumberOfRetries(immediateRetries); });

            var delayedRetries = int.Parse(configuration["NServiceBus:Retries:DelayedRetriesAmount"]);
            var delayedRetriesTimeIncrease =
                int.Parse(configuration["NServiceBus:Retries:DelayedRetriesTimeIncreaseInSeconds"]);
            recoverability.Delayed(
                delayed =>
                {
                    delayed.NumberOfRetries(delayedRetries);
                    delayed.TimeIncrease(TimeSpan.FromSeconds(delayedRetriesTimeIncrease));
                });
        }
        
        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            LogManager.Use<SerilogFactory>();
        }
    }
}