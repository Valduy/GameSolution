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
    public class HolePuncher : IHolePuncher
    {
        private const int LoopDelay = 1000;

        private int _currentAttempts;
        private byte[] _packet;

        private UdpClient _udpClient;
        private uint _sessionId;
        private CancellationToken _cancellationToken;

        private List<ClientEndPoints> _potentials; 
        private List<IPEndPoint> _connected;

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
            _connected = new List<IPEndPoint>();
            _packet = PacketHelper.CreatePacket(_sessionId, 0);
            await ConnectionLoop();
            return _connected;
        }

        private async Task ConnectionLoop()
        {
            while (_potentials.Any())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(LoopDelay, _cancellationToken);
                await ConnectionFrame();
            }
        }

        private async Task ConnectionFrame()
        {
            await SendPackets();

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

        private async Task SendPackets()
        {
            foreach (var client in _potentials)
            {
                await SendPacket(_packet, client.PublicEndPoint);

                if (!IPAddress.IsLoopback(IPAddress.Parse(client.PublicEndPoint.Ip)))
                {
                    await SendPacket(_packet, client.PrivateEndPoint);
                }
            }

            foreach (var client in _connected)
            {
                await SendPacket(_packet, client);
            }
        }

        private void ProcessMessage(byte[] message, IPEndPoint endPoint)
        {
            if (message.Length >= PacketHelper.HeaderSize 
                && PacketHelper.GetSessionId(message) == _sessionId 
                && TryGetFromPotentials(endPoint, out var client))
            {
                _potentials.Remove(client);
                _connected.Add(endPoint);
            }
        }

        private async Task SendPacket(byte[] message, ClientEndPoint endPoint) 
            => await _udpClient.SendAsync(message, message.Length, endPoint.Ip, endPoint.Port);

        private async Task SendPacket(byte[] message, IPEndPoint endPoint)
            => await _udpClient.SendAsync(message, message.Length, endPoint);

        private bool TryGetFromPotentials(IPEndPoint endPoint, out ClientEndPoints client)
        {
            client = _potentials.FirstOrDefault(
                c => c.PrivateEndPoint.IsSame(endPoint) || c.PublicEndPoint.IsSame(endPoint));
            return client != null;
        }
    }
}
