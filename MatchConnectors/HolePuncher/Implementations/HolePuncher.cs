using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network;
using Network.Messages;

namespace Connectors.HolePuncher
{
    public enum ConnectionAction
    {
        Check,
        Confirm,
    }

    public class HolePuncher : IHolePuncher
    {
        private const int LoopDelay = 1000;

        private readonly byte[] CheckMessage;
        private readonly byte[] ConfirmMessage;

        private UdpClient _udpClient;
        private CancellationToken _cancellationToken;

        private List<ClientEndPoints> _potentials;
        private HashSet<IPEndPoint> _requesters;
        private List<IPEndPoint> _confirmed;

        public HolePuncher()
        {
            CheckMessage = GetConnectionMessage(ConnectionAction.Check);
            ConfirmMessage = GetConnectionMessage(ConnectionAction.Confirm);
        }

        public async Task<List<IPEndPoint>> ConnectAsync(
            UdpClient udpClient, 
            IEnumerable<ClientEndPoints> clients, 
            CancellationToken cancellationToken = default)
        {
            _udpClient = udpClient;
            _cancellationToken = cancellationToken;
            _potentials = new List<ClientEndPoints>(clients);
            _requesters = new HashSet<IPEndPoint>();
            _confirmed = new List<IPEndPoint>();
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

            while (_udpClient.Available > 0)
            {
                var result = await _udpClient.ReceiveAsync();
                ProcessMessage(result.Buffer, result.RemoteEndPoint);
            }
        }

        private async Task SendMessages()
        {
            foreach (var client in _potentials)
            {
                if (IsLoopback(client.PublicEndPoint.Ip))
                {
                    await SendMessage(CheckMessage, client.PublicEndPoint);
                }
                
                await SendMessage(CheckMessage, client.PrivateEndPoint);
            }

            foreach (var endPoint in _requesters)
            {
                await SendMessage(ConfirmMessage, endPoint);
            }
        }

        private void ProcessMessage(byte[] message, IPEndPoint endPoint)
        {
            if (MessageHelper.GetMessageType(message) == NetworkMessages.Connect)
            {
                switch (ToConnectionAction(message))
                {
                    case ConnectionAction.Check:
                    {
                        if (TryGetFromPotentials(endPoint, out var client))
                        {
                            _potentials.Remove(client);
                            _requesters.Add(endPoint);
                        }
                        break;
                    }
                    case ConnectionAction.Confirm:
                    {
                        if (TryGetFromPotentials(endPoint, out var client))
                        {
                            _potentials.Remove(client);
                            _confirmed.Add(endPoint);
                        }
                        else if (_requesters.Contains(endPoint))
                        {
                            _requesters.Remove(endPoint);
                            _confirmed.Add(endPoint);
                        }
                        break;
                    }
                }
            }
        }

        private async Task SendMessage(byte[] message, ClientEndPoint endPoint) 
            => await _udpClient.SendAsync(message, message.Length, endPoint.Ip, endPoint.Port);

        private async Task SendMessage(byte[] message, IPEndPoint endPoint)
            => await _udpClient.SendAsync(message, message.Length, endPoint);

        private bool IsLoopback(string ip) 
            => IPAddress.IsLoopback(IPAddress.Parse(ip));

        private byte[] GetConnectionMessage(ConnectionAction action)
            => MessageHelper.GetMessage(NetworkMessages.Connect, BitConverter.GetBytes((int)action));

        private ConnectionAction ToConnectionAction(byte[] message) 
            => (ConnectionAction) BitConverter.ToInt32(MessageHelper.ToByteArray(message), 0);

        private bool TryGetFromPotentials(IPEndPoint endPoint, out ClientEndPoints client)
        {
            client = _potentials.FirstOrDefault(
                c => c.PrivateEndPoint.IsSame(endPoint) || c.PublicEndPoint.IsSame(endPoint));
            return client != null;
        }

        private bool IsAllConfirmed() => !_potentials.Any() && !_requesters.Any();
    }
}
