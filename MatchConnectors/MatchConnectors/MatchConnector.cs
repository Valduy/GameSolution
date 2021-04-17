using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Connectors.MatchConnectors.States;
using Network;
using Network.Messages;

namespace Connectors.MatchConnectors
{
    public class MatchConnector : IMatchConnectorBase
    {
        private bool _isRun;
        private UdpClient _udpClient;
        private CancellationToken _token;

        public string ServerIp { get; private set; }
        public int ServerPort { get; private set; }
        internal string Ip { get; private set; }
        internal int Port { get; private set; }
        internal ConnectionMessage ConnectionMessage { get; set; }
        internal MatchConnectorStateBase State { get; set; }

        public async Task<ConnectionMessage> ConnectAsync(UdpClient client, string serverIp, int serverPort, CancellationToken token = default)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            _udpClient = client;
            _token = token;
            Ip = NetworkHelper.GetLocalIPAddress();
            Port = client.GetPort();
            State = new HelloMatchState(this);
            await ConnectionLoopAsync();
            return ConnectionMessage;
        }

        internal async Task SendMessageAsync(byte[] message)
            => await _udpClient.SendAsync(message, message.Length, ServerIp, ServerPort);

        internal void FinishConnection()
        {
            _isRun = false;
        }

        private async Task ConnectionLoopAsync()
        {
            _isRun = true;

            while (_isRun && !_token.IsCancellationRequested)
            {
                await ConnectionFrameAsync();
                await Task.Delay(100, _token);
            }
        }

        private async Task ConnectionFrameAsync()
        {
            // TODO: если прием прошел неудачно
            while (_udpClient.Available > 0)
            {
                var receiveResult = await _udpClient.ReceiveAsync();
                await State.ProcessMessageAsync(receiveResult.Buffer);
            }
        }
    }
}
