namespace ProtoRouterEndpointA.Configuration
{
    using System;

    public static class CurrentEnvironment
    {
        public static string HostingEnvironment => Environment.GetEnvironmentVariable("ENVIRONMENT");
        
        public static bool IsDevelopmentEnvironment => HostingEnvironment?.ToLower() == "development";
    }
}