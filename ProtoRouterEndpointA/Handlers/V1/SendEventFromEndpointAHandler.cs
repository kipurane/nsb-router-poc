namespace ProtoRouterEndpointA.Handlers.V1
{
    using System.Threading.Tasks;
    using NServiceBus;
    using Shared.Commands.V1;
    using Shared.Events.V1;

    public class SendEventFromEndpointAHandler : IHandleMessages<SendEventFromEndpointA>
    {
        public async Task Handle(SendEventFromEndpointA message, IMessageHandlerContext context)
        {
            await context.Publish(new EventFromEndpointA {Message = "Event from endpoint A."}).ConfigureAwait(false);
        }
    }
}