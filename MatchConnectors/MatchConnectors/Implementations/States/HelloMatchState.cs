using System.Threading.Tasks;
using Network;
using Network.Messages;
using Newtonsoft.Json;

namespace Connectors.MatchConnectors
{
    internal class HelloMatchState : MatchConnectorStateBase
    {
        private readonly byte[] _message;

        public HelloMatchState(MatchConnector context) : base(context)
        {
            var privateEndPoint = new ClientEndPoint(Context.Ip, Context.Port);
            var data = JsonConvert.SerializeObject(privateEndPoint);
            _message = MessageHelper.GetMessage(NetworkMessages.Hello, data);
        }

        public override async Task SendMessageAsync() 
            => await Context.SendMessageAsync(_message);

        public override void ProcessMessage(byte[] message)
        {
            if (MessageHelper.GetMessageType(message) == NetworkMessages.Initial)
            {
                Context.State = new WaitMatchState(Context);
            }
        }
    }
}
