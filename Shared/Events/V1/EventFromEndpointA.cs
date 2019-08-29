namespace Shared.Events.V1
{
    using NServiceBus;

    public class EventFromEndpointA : IEvent
    {
        public string Message { get; set; }
    }
}