namespace ProtoRouterEndpointB.Handlers
{
    using System.Threading.Tasks;
    using NServiceBus;
    using Serilog;
    using Shared.Events.V1;

    public class EventFromEndpointAHandler : IHandleMessages<EventFromEndpointA>
    {
        public Task Handle(EventFromEndpointA message, IMessageHandlerContext context)
        {
            Log.Information($"Got {message.GetType().Name} event from ENDPOINT A with message: {message.Message}.");
            return Task.CompletedTask;
        }
    }
}