namespace ProtoRouterEndpointB.Handlers
{
    using System.Threading.Tasks;
    using NServiceBus;
    using Serilog;
    using Shared.Commands.V1;

    public class SendCommandToEndpointBHandler : IHandleMessages<SendCommandToEndpointB>
    {
        public Task Handle(SendCommandToEndpointB message, IMessageHandlerContext context)
        {
            Log.Information($"Got {message.GetType().Name} command from ENDPOINT A with message {message.Message}.");
            return Task.CompletedTask;
        }
    }
}