using System.Threading.Tasks;
using Network;
using Network.Messages;
using Newtonsoft.Json;

namespace Connectors.MatchConnectors.States
{
    public class HelloMatchState : MatchConnectorStateBase
    {
        private readonly byte[] _message;

        public HelloMatchState(MatchConnector context) : base(context)
        {
            var privateEndPoint = new ClientEndPoint(Context.Ip, Context.Port);
            var data = JsonConvert.SerializeObject(privateEndPoint);
            _message = MessageHelper.GetMessage(NetworkMessages.Hello, data);
        }

        public override async Task ProcessMessageAsync(byte[] message)
        {
            if (IsExpectedMessageType(message))
            {
                Context.State = new WaitMatchState(Context);
            }

            await Context.SendMessageAsync(_message);
        }

        private bool IsExpectedMessageType(byte[] message)
        {
            var messageType = MessageHelper.GetMessageType(message);
            return messageType == NetworkMessages.Hello || messageType == NetworkMessages.Initial;
        }
    }
}
