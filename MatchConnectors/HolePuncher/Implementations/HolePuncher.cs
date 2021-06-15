using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network;
using Network.Messages;
using Newtonsoft.Json;

namespace Connectors.HolePuncher
{
    public class HolePuncher : IHolePuncher
    {
        private const int LoopDelay = 1000;

        private int _currentAttempts;
        private byte[] _checkMessage;
        private byte[] _confirmMessage;

        private UdpClient _udpClient;
        private uint _sessionId;
        private CancellationToken _cancellationToken;

        private List<ClientEndPoints> _potentials;
        private HashSet<IPEndPoint> _requesters;
        private List<IPEndPoint> _confirmed;

        public int MaxAttempts { get; set; } = 20;

        public async Task<List<IPEndPoint>> ConnectAsync(
            UdpClient udpClient, 
            uint sessionId,
            IEnumerable<ClientEndPoints> clients, 
            CancellationToken cancellationToken = default)
        {
            _udpClient = udpClient;
            _sessionId = sessionId;
            _cancellationToken = cancellationToken;
            _potentials = new List<ClientEndPoints>(clients);
            _requesters = new HashSet<IPEndPoint>();
            _confirmed = new List<IPEndPoint>();
            _checkMessage = GetConnectionMessage(ConnectionAction.Check);
            _confirmMessage = GetConnectionMessage(ConnectionAction.Confirm);
            await ConnectionLoop();
            return _confirmed;
        }

        private async Task ConnectionLoop()
        {
            while (!IsAllConfirmed())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(LoopDelay, _cancellationToken);
                await ConnectionFrame();
            }
        }

        private async Task ConnectionFrame()
        {
            await SendMessages();

            if (_udpClient.Available > 0)
            {
                while (_udpClient.Available > 0)
                {
                    try
                    {
                        var result = await _udpClient.ReceiveAsync();
                        _currentAttempts = 0;
                        ProcessMessage(result.Buffer, result.RemoteEndPoint);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else
            {
                _currentAttempts++;

                if (_currentAttempts >= MaxAttempts)
                {
                    throw new ConnectorException("Не удалось соединиться с другими игроками.");
                }
            }
        }

        private async Task SendMessages()
        {
            foreach (var client in _potentials)
            {
                await SendMessage(_checkMessage, client.PublicEndPoint);

                if (!IPAddress.IsLoopback(IPAddress.Parse(client.PublicEndPoint.Ip)))
                {
                    await SendMessage(_checkMessage, client.PrivateEndPoint);
                }
            }

            foreach (var endPoint in _requesters)
            {
                await SendMessage(_confirmMessage, endPoint);
            }
        }

        private void ProcessMessage(byte[] message, IPEndPoint endPoint)
        {
            ClientEndPoints client;

            if (IsExpected(message))
            {
                try
                {
                    var data = ToConnectionMessage(message);
                    if (data.SessionId != _sessionId) return;

                    if (data.ConnectionAction == ConnectionAction.Check 
                        && TryGetFromPotentials(endPoint, out client))
                    {
                        _potentials.Remove(client);
                        _requesters.Add(endPoint);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is JsonReaderException || ex is ArgumentNullException)
                    {
                        // TODO: лог о неправильном JSON'е?
                    }
                }
            }

            if (TryGetFromPotentials(endPoint, out client))
            {
                _potentials.Remove(client);
                _confirmed.Add(endPoint);
            }
            else if (_requesters.Contains(endPoint))
            {
                _requesters.Remove(endPoint);
                _confirmed.Add(endPoint);
            }
        }

        private async Task SendMessage(byte[] message, ClientEndPoint endPoint) 
            => await _udpClient.SendAsync(message, message.Length, endPoint.Ip, endPoint.Port);

        private async Task SendMessage(byte[] message, IPEndPoint endPoint)
            => await _udpClient.SendAsync(message, message.Length, endPoint);

        private byte[] GetConnectionMessage(ConnectionAction action) 
            => MessageHelper.GetMessage(NetworkMessages.Connect, JsonConvert.SerializeObject(new HolePuncherMessage
            {
                SessionId = _sessionId,
                ConnectionAction = action,
            }));

        private HolePuncherMessage ToConnectionMessage(byte[] message) 
            => JsonConvert.DeserializeObject<HolePuncherMessage>(MessageHelper.ToString(message));

        private bool TryGetFromPotentials(IPEndPoint endPoint, out ClientEndPoints client)
        {
            client = _potentials.FirstOrDefault(
                c => c.PrivateEndPoint.IsSame(endPoint) || c.PublicEndPoint.IsSame(endPoint));
            return client != null;
        }

        private bool IsExpected(byte[] message)
            => message.Length >= 4 && MessageHelper.GetMessageType(message) == NetworkMessages.Connect;

        private bool IsAllConfirmed() => !_potentials.Any() && !_requesters.Any();
    }
}
