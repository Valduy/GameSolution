using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network.NetworkBuffers;

namespace Network.Proxy
{
    public class ClientNetworkProxy
    {
        private const int SendBufferSize = 10;
        private const int ReceiveBufferSize = 20;

        private readonly UdpClient _udpClient;

        private CancellationTokenSource _tokenSource;
        private ConcurrentNetworkBuffer _writeBuffer;
        private ConcurrentNetworkBuffer _readBuffer;

        public IPEndPoint Host { get; }
        public IWriteOnlyNetworkBuffer WriteBuffer => _writeBuffer;
        public IReadOnlyNetworkBuffer ReadBuffer => _readBuffer;

        public ClientNetworkProxy(UdpClient udpClient, IPEndPoint host)
        {
            _udpClient = udpClient;
            Host = host;
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _writeBuffer = new ConcurrentNetworkBuffer(SendBufferSize);
            _readBuffer = new ConcurrentNetworkBuffer(ReceiveBufferSize);

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
                var message = _writeBuffer.Read();
                await _udpClient.SendAsync(message, message.Length, Host);
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
                    _readBuffer.Write(result.Buffer);
                }
            }
        }
    }
}
