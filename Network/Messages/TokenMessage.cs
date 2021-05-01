namespace Network.Messages
{
    public class TokenMessage
    {
        public string accessToken { get; set; }
        public string id { get; set; }

        public TokenMessage() { }

        public TokenMessage(string accessToken, string id)
        {
            this.accessToken = accessToken;
            this.id = id;
        }
    }
}
