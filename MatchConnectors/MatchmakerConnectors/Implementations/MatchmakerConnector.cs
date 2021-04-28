using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Connectors.MatchmakerConnectors
{
    public class MatchmakerConnector : IMatchmakerConnector
    {
        private const int LoopDelay = 1000;

        private bool _isRun;
        private HttpClient _client;
        private CancellationToken _cancellationToken;

        public ClientEndPoint PrivateEndPoint { get; private set; }
        public string Host { get; private set; }

        internal MatchmakerConnectorStateBase State { get; set; }
        internal int? MatchPort { get; set; }

        public async Task<int?> ConnectAsync(
            ClientEndPoint privateEndPoint,
            string host,
            string bearerToken,
            CancellationToken cancellationToken = default)
        {
            PrivateEndPoint = privateEndPoint;
            Host = host;
            _cancellationToken = cancellationToken;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization 
                = new AuthenticationHeaderValue("Bearer", bearerToken);
            State = new EnqueueState(this);
            await ConnectionLoopAsync();
            return MatchPort;
        }

        internal async Task<HttpResponseMessage> PostAsync(string uri, string json) 
            => await _client.PostAsync(
                uri, 
                new StringContent(json, Encoding.ASCII, "application/json"), 
                _cancellationToken);

        internal async Task<HttpResponseMessage> GetAsync(string uri) 
            => await _client.GetAsync(uri, _cancellationToken);

        internal void FinishConnection() => _isRun = false;

        private async Task ConnectionLoopAsync()
        {
            _isRun = true;

            while (_isRun && !_cancellationToken.IsCancellationRequested)
            {
                await ConnectionFrameAsync();
                await Task.Delay(LoopDelay, _cancellationToken);
            }

            // TODO: сообщение о выходе из очереди
            _client.Dispose();
        }

        private async Task ConnectionFrameAsync()
        {
            await State.ConnectAsync();
        }
    }
}
