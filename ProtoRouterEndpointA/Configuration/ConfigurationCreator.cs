namespace ProtoRouterEndpointA.Configuration
{
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public static class ConfigurationCreator
    {
        public static IConfiguration GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("commonappsettings.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{CurrentEnvironment.HostingEnvironment ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            if (CurrentEnvironment.IsDevelopmentEnvironment) 
                return configuration;

            // Suppressing Console Logging by Azure KeyVault
            LoggerCallbackHandler.UseDefaultLogging = false;

            configurationBuilder
                .AddAzureKeyVault(
                    $"{configuration["KeyVault:BaseUrl"]}",
                    $"{configuration["KeyVault:ClientId"]}",
                    $"{configuration["KeyVault:ClientSecret"]}")
                .AddJsonFile($"appsettings.override.json", optional: true)
                .Build();

            configuration = configurationBuilder.Build();

            return configuration;
        }
    }
}