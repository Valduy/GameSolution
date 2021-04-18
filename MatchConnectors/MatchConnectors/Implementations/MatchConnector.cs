using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Connectors.MatchConnectors.States;
using Network;
using Network.Messages;

namespace Connectors.MatchConnectors
{
    public class MatchConnector : IMatchConnector
    {
        private const int LoopDelay = 100;
        private const int ReceiveTimeout = 100;
        private const int MaxAttempts = 10;

        private bool _isRun;
        private int _currentAttempts;
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
                await Task.Delay(LoopDelay, _token);
            }
        }

        private async Task ConnectionFrameAsync()
        {
            await State.SendMessageAsync();

            while (_udpClient.Available > 0)
            {
                if (TryReceive(out var message))
                {
                    State.ProcessMessage(message);
                    _currentAttempts = 0;
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

        private bool TryReceive(out byte[] message)
        {
            try
            {
                SetSocketTimeout();
                IPEndPoint endPoint = null;
                message = _udpClient.Receive(ref endPoint);
                ResetSocketTimeout();
                return true;
            }
            catch (SocketException)
            {
                ResetSocketTimeout();
                message = null;
                return false;
            }
        }

        private void SetSocketTimeout() 
            => _udpClient.Client.ReceiveTimeout = ReceiveTimeout;

        private void ResetSocketTimeout() 
            => _udpClient.Client.ReceiveTimeout = 0;
    }
}
