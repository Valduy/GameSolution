namespace Matchmaker.Messages
{
    public class ErrorMessage
    {
        public string Message { get; }

        public ErrorMessage(string message)
        {
            Message = message;
        }
    }
}
