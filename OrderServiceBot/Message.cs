
namespace OrderServiceBot
{
    public class Message
    {
        public string message { get; set; }
        public int userId { get; set; }

        public Message(int userId, string message)
        {
            this.message = message;
            this.userId = userId;
        }   
    }
}
