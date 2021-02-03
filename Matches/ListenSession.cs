using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Matches
{
    public class ListenSession : IMatch
    {
        private UdpClient _udpClient;
        private Task _matchTask;
        private List<IPEndPoint> _players;

        public static int PlayersCount => 2;
        public int Port { get; }
        public long TimeForStarting { get; set; } = 30000;

        public event Action<IMatch> Started;
        public event Action<IMatch> Ended;

        public ListenSession()
        {
            _udpClient = new UdpClient(0);
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                if (await WaitForPlayersAsync())
                {
                    Started?.Invoke(this);
                    await MatchLoopAsync();
                }

                Ended?.Invoke(this);
            });
        }

        public void Stop()
        {
            // TODO: Остановить матч
        }

        /// <summary>
        /// Метод ожидает игроков, готовых присоединиться к матчу.
        /// </summary>
        /// <returns>true, если игроков достаточно для создания матча, false в противном случае.</returns>
        private async Task<bool> WaitForPlayersAsync()
        {
            var timer = new Stopwatch();
            timer.Start();

            // TODO: использовать игровой цикл.
            while (timer.ElapsedMilliseconds < TimeForStarting && _players.Count < PlayersCount)
            {
                if (_udpClient.Available > 0)
                {
                    var player = await _udpClient.ReceiveAsync();

                    if (!_players.Any(o => o.Equals(player.RemoteEndPoint)))
                    {
                        _players.Add(player.RemoteEndPoint);
                    }

                    // TODO: послать ответное сообщение
                    await Task.Delay(10);
                }
            }

            timer.Stop();
            return _players.Count > 0;
        }

        private async Task MatchLoopAsync()
        {
            // TODO: цикл матча
        }
    }
}
