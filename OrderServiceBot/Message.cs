
namespace OrderServiceBot
{
    public class Message
    {
        public string message { get; set; }
        public string connectionId { get; set; }

        public Message(string connectionId, string message)
        {
            this.message = message;
            this.connectionId = connectionId;
        }   
    }
}
