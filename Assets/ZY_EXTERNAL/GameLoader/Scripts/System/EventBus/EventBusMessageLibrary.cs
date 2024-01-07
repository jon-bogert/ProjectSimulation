public class SimpleTextMessage : IEventBusMessage
{
    public string Message { get; set; }
    public object Sender { get; set; }
    public SimpleTextMessage(object sender, string message)
    {
        Sender = sender;
        Message = message;
    }
}