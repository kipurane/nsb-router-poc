namespace Shared.Commands.V1
{
    using NServiceBus;

    public class SendCommandToEndpointB : ICommand
    {
        public string Message { get; set; }
    }
}