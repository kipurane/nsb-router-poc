namespace ProtoRouterEndpointA.Handlers.V1
{
    using System.Threading.Tasks;
    using NServiceBus;
    using Shared.Commands.V1;

    public class SendCommandFromEndpointAHandler : IHandleMessages<SendCommandFromEndpointA>
    {
        public async Task Handle(SendCommandFromEndpointA message, IMessageHandlerContext context)
        {
            await context.Send(new SendCommandToEndpointB { Message = "This came from ENDPOINT A." }).ConfigureAwait(false);
        }
    }
}