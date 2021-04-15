﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameLoops;
using Matches;
using Network;

namespace Matchmaker.Services
{
    public class MatchmakerService<TMatch> : IMatchmakerService, IDisposable where TMatch : IMatch, new()
    {
        private readonly FixedFpsGameLoop _matchmakingLoop;
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

        private readonly List<string> _waitingPlayers = new List<string>();
        private readonly List<Task> _matchesTasks = new List<Task>();
        private readonly Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        private bool _disposed;

        public int PlayersPerMatch { get; }

        public MatchmakerService(int playersPerMatch)
        {
            PlayersPerMatch = playersPerMatch;
            _matchmakingLoop = new FixedFpsGameLoop(ManageMatchmaking, 60);
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _matchmakingLoop.Start();
        }

        ~MatchmakerService()
        {
            Dispose(false);
        }

        public bool Enqueue(string userId)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Any(o => o == userId))
                {
                    return false;
                }

                _waitingPlayers.Add(userId);
                return true;
            }
        }

        public UserStatus GetStatus(string userId)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Any(o => o == userId))
                {
                    return UserStatus.Wait;
                }
            }

            lock (_playerToMatch)
            {
                if (_playerToMatch.ContainsKey(userId))
                {
                    return UserStatus.Connected;
                }
            }

            return UserStatus.Absent;
        }

        public int? GetMatch(string userId)
        {
            lock (_playerToMatch)
            {
                if (_playerToMatch.TryGetValue(userId, out var port))
                {
                    _playerToMatch.Remove(userId);
                    return port;
                }

                return null;
            }
        }

        public bool Remove(string userId)
        {
            lock (_waitingPlayers)
            {
                return _waitingPlayers.Remove(userId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) { }

            _matchmakingLoop.Stop();
            _tokenSource.Cancel();

            lock (_matchesTasks)
            {
                Task.WaitAll(_matchesTasks.ToArray());
            }

            _disposed = true;
        }

        private void ManageMatchmaking(double dt)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Count >= PlayersPerMatch)
                {
                    var match = new TMatch();
                    match.MatchStarted += OnMatchStarted;

                    for (int i = 0; i < PlayersPerMatch; i++)
                    {
                        var player = _waitingPlayers.Last();
                        _playerToMatch[player] = match.Port;
                        _waitingPlayers.Remove(player);
                    }

                    ConfigureMatchTask(match);
                }
            }
        }

        private void ConfigureMatchTask(TMatch match)
        {
            var matchTask = new Task(async () => await match.WorkAsync(PlayersPerMatch, _token), _token);
            matchTask.ContinueWith(OnMatchTaskEnded, _token);

            lock (_matchesTasks)
            {
                _matchesTasks.Add(matchTask);
            }

            matchTask.Start();
        }

        private void OnMatchStarted(IMatch matchBase)
        {
            lock (_playerToMatch)
            {
                var matchPlayers = _playerToMatch
                    .Where(o => o.Value == matchBase.Port)
                    .Select(o => o.Key);

                foreach (var player in matchPlayers)
                {
                    _playerToMatch.Remove(player);
                }
            }
        }

        private void OnMatchTaskEnded(Task task)
        {
            lock (_matchesTasks)
            {
                _matchesTasks.Remove(task);
            }
        }
    }
}
