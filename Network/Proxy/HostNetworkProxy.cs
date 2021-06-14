using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network.NetworkBuffers;

namespace Network.Proxy
{
    public class HostNetworkProxy
    {
        private const int SendBufferSize = 10;
        private const int ReceiveBufferSize = 20;

        private readonly UdpClient _udpClient;
        private Dictionary<IPEndPoint, ConcurrentNetworkBuffer> _readBuffers;
        private Dictionary<IPEndPoint, ConcurrentNetworkBuffer> _writeBuffers;

        private CancellationTokenSource _tokenSource;

        private uint _sentPacketNumber;
        private Dictionary<IPEndPoint, uint> _receivedPacketsNumber;

        public IEnumerable<IPEndPoint> Clients { get; }

        public HostNetworkProxy(UdpClient udpClient, IEnumerable<IPEndPoint> clients)
        {
            _udpClient = udpClient;
            Clients = clients;
        }

        public void Dispose() => Stop();

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _readBuffers = new Dictionary<IPEndPoint, ConcurrentNetworkBuffer>();
            _writeBuffers = new Dictionary<IPEndPoint, ConcurrentNetworkBuffer>();
            _receivedPacketsNumber = new Dictionary<IPEndPoint, uint>();

            foreach (var client in Clients)
            {
                _readBuffers[client] = new ConcurrentNetworkBuffer(ReceiveBufferSize);
            }

            foreach (var client in Clients)
            {
                _writeBuffers[client] = new ConcurrentNetworkBuffer(SendBufferSize);
            }

            Task.Run(SendLoopAsync);
            Task.Run(ReceiveLoopAsync);
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        public IReadOnlyNetworkBuffer GetReadBuffer(IPEndPoint endPoint) => _readBuffers[endPoint];

        public IWriteOnlyNetworkBuffer GetWriteBuffer(IPEndPoint endPoint) => _writeBuffers[endPoint];

        private async Task SendLoopAsync()
        {
            while (true)
            {
                _tokenSource.Token.ThrowIfCancellationRequested();
                await SendFrameAsync();
            }
        }

        protected virtual async Task SendFrameAsync()
        {
            foreach (var clientBufferPair in _writeBuffers)
            {
                var buffer = clientBufferPair.Value;

                while (!buffer.IsEmpty)
                {
                    var data = buffer.Read();
                    var packet = PacketHelper.CreatePacket(_sentPacketNumber, data);
                    await _udpClient.SendAsync(packet, packet.Length, clientBufferPair.Key);
                }
            }

            _sentPacketNumber++;
        }

        private async Task ReceiveLoopAsync()
        {
            while (true)
            {
                try
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    await ReceiveFrameAsync();
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async Task ReceiveFrameAsync()
        {
            if (_udpClient.Available > 0)
            {
                var result = await _udpClient.ReceiveAsync();

                if (_readBuffers.TryGetValue(result.RemoteEndPoint, out var buffer))
                {
                    var packetNumber = PacketHelper.GetNumber(result.Buffer);

                    if (packetNumber >= _receivedPacketsNumber[result.RemoteEndPoint])
                    {
                        buffer.Write(result.Buffer);
                        _receivedPacketsNumber[result.RemoteEndPoint] = packetNumber;
                    }
                }
            }
        }
    }
}
