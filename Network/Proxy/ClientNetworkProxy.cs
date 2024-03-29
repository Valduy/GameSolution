﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network.NetworkBuffers;

namespace Network.Proxy
{
    public class ClientNetworkProxy : IClientNetworkProxy
    {
        private const int SendBufferSize = 10;
        private const int ReceiveBufferSize = 20;
        private const int Tolerance = 200;

        private readonly UdpClient _udpClient;

        private CancellationTokenSource _tokenSource;
        private ConcurrentNetworkBuffer _writeBuffer;
        private ConcurrentNetworkBuffer _readBuffer;

        private uint _sentPacketNumber;
        private uint _receivedPacketNumber;
        private uint[] _packetNumbersBuffer;
        private int _pointer;

        public uint SessionId { get; }
        public IPEndPoint Host { get; }
        public IWriteOnlyNetworkBuffer WriteBuffer => _writeBuffer;
        public IReadOnlyNetworkBuffer ReadBuffer => _readBuffer;
        
        public ClientNetworkProxy(UdpClient udpClient, uint sessionId, IPEndPoint host)
        {
            _udpClient = udpClient;
            SessionId = sessionId;
            Host = host;
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _writeBuffer = new ConcurrentNetworkBuffer(SendBufferSize);
            _readBuffer = new ConcurrentNetworkBuffer(ReceiveBufferSize);
            _sentPacketNumber = 0;
            _receivedPacketNumber = 0;
            _pointer = 0;
            _packetNumbersBuffer = new uint[10];
            
            Task.Run(SendLoopAsync, _tokenSource.Token);
            Task.Run(ReceiveLoopAsync, _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        public void Dispose() => Stop();

        private async Task SendLoopAsync()
        {
            while (true)
            {
                _tokenSource.Token.ThrowIfCancellationRequested();
                await SendFrameAsync();
            }
        }

        private async Task SendFrameAsync()
        {
            while (!_writeBuffer.IsEmpty)
            {
                var data = _writeBuffer.Read();
                var packet = PacketHelper.CreatePacket(SessionId, _sentPacketNumber++, data);
                await _udpClient.SendAsync(packet, packet.Length, Host);
            }
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

                if (Equals(result.RemoteEndPoint, Host))
                {
                    if (PacketHelper.GetSessionId(result.Buffer) != SessionId) return;
                    var packetNumber = PacketHelper.GetNumber(result.Buffer);
                    _packetNumbersBuffer[_pointer++] = packetNumber;

                    if (_pointer == _packetNumbersBuffer.Length)
                    {
                        _pointer = 0;

                        if (PacketHelper.IsShouldCorrectPacketNumber(_packetNumbersBuffer, Tolerance))
                        {
                            _receivedPacketNumber = _packetNumbersBuffer.Min();
                        }
                    }
                    
                    if (packetNumber >= _receivedPacketNumber)
                    {
                        _readBuffer.Write(PacketHelper.GetData(result.Buffer));
                        _receivedPacketNumber = packetNumber;
                    }
                }
            }
        }
    }
}
