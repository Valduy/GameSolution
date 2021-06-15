using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network;
using Network.Messages;

namespace Connectors.MatchConnectors
{
    public class MatchConnector : IMatchConnector
    {
        private const int LoopDelay = 100;
        private const int ReceiveTimeout = 100;

        private bool _isRun;
        private int _currentAttempts;
        private UdpClient _udpClient;
        private CancellationToken _cancellationToken;

        public int MaxAttempts { get; set; } = 50;
        public string ServerIp { get; private set; }
        public int ServerPort { get; private set; }
        internal string Ip { get; private set; }
        internal int Port { get; private set; }
        internal ConnectionMessage ConnectionMessage { get; set; }
        internal MatchConnectorStateBase State { get; set; }

        public async Task<ConnectionMessage> ConnectAsync(
            UdpClient client,
            string serverIp,
            int serverPort,
            CancellationToken cancellationToken = default)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            _udpClient = client;
            _cancellationToken = cancellationToken;
            Ip = NetworkHelper.GetLocalIPAddress();
            Port = client.GetPort();
            State = new HelloMatchState(this);
            await ConnectionLoopAsync();
            return ConnectionMessage;
        }

        internal async Task SendMessageAsync(byte[] message)
            => await _udpClient.SendAsync(message, message.Length, ServerIp, ServerPort);

        internal void FinishConnection() => _isRun = false;

        private async Task ConnectionLoopAsync()
        {
            _isRun = true;

            while (_isRun)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                await ConnectionFrameAsync();
                await Task.Delay(LoopDelay, _cancellationToken);
            }
        }

        private async Task ConnectionFrameAsync()
        {
            await State.SendMessageAsync();

            if (_udpClient.Available > 0)
            {
                _currentAttempts = 0;

                while (_isRun && _udpClient.Available > 0)
                {
                    var result = await _udpClient.ReceiveAsync();
                    State.ProcessMessage(result.Buffer);
                }
            }
            else
            {
                _currentAttempts++;

                if (_currentAttempts >= MaxAttempts)
                {
                    throw new ConnectorException("Потеряно соединение с сервером.");
                }
            }
        }
    }
}
