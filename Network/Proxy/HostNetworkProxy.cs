using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int Tolerance = 200;

        private readonly UdpClient _udpClient;
        private Dictionary<IPEndPoint, ConcurrentNetworkBuffer> _readBuffers;
        private Dictionary<IPEndPoint, ConcurrentNetworkBuffer> _writeBuffers;

        private CancellationTokenSource _tokenSource;

        private uint _sentPacketNumber;
        private Dictionary<IPEndPoint, (uint MaxNumber, uint[] Buffer, int Pointer)> _packetsInfo;

        public uint SessionId { get; }
        public IEnumerable<IPEndPoint> Clients { get; }

        public HostNetworkProxy(UdpClient udpClient, uint sessionId, IEnumerable<IPEndPoint> clients)
        {
            _udpClient = udpClient;
            SessionId = sessionId;
            Clients = clients;
        }

        public void Dispose() => Stop();

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _readBuffers = new Dictionary<IPEndPoint, ConcurrentNetworkBuffer>();
            _writeBuffers = new Dictionary<IPEndPoint, ConcurrentNetworkBuffer>();
            _packetsInfo = new Dictionary<IPEndPoint, (uint MaxNumber, uint[] Buffer, int Pointer)>();

            foreach (var client in Clients)
            {
                _readBuffers[client] = new ConcurrentNetworkBuffer(ReceiveBufferSize);
                _writeBuffers[client] = new ConcurrentNetworkBuffer(SendBufferSize);
                _packetsInfo[client] = (0, new uint[10], 0);
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
                var ip = clientBufferPair.Key;
                var buffer = clientBufferPair.Value;

                while (!buffer.IsEmpty)
                {
                    var data = buffer.Read();
                    var packet = PacketHelper.CreatePacket(SessionId, _sentPacketNumber, data);
                    await _udpClient.SendAsync(packet, packet.Length, ip);
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
                    if (PacketHelper.GetSessionId(result.Buffer) != SessionId) return;
                    var packetNumber = PacketHelper.GetNumber(result.Buffer);
                    var packetInfo = _packetsInfo[result.RemoteEndPoint];
                    packetInfo.Buffer[packetInfo.Pointer++] = packetNumber;

                    if (packetInfo.Pointer == packetInfo.Buffer.Length)
                    {
                        packetInfo.Pointer = 0;

                        if (PacketHelper.IsShouldCorrectPacketNumber(packetInfo.Buffer, Tolerance))
                        {
                            packetInfo.MaxNumber = packetInfo.Buffer.Min();
                        }
                    }

                    if (packetNumber >= packetInfo.MaxNumber)
                    {
                        buffer.Write(PacketHelper.GetData(result.Buffer));
                        packetInfo.MaxNumber = packetNumber;
                    }
                }
            }
        }
    }
}
