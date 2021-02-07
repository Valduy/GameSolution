﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameLoops
{
    public class FixedFpsGameLoop : FixedFpsGameLoopBase
    {
        private readonly Action<double> _gameFrame;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public FixedFpsGameLoop(Action<double> gameFrame, uint fps) 
            : base (fps) 
            => _gameFrame = gameFrame;

        public override void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            Task.Run(() =>
            {
                var timer = new Stopwatch();
                timer.Start();

                while (!_token.IsCancellationRequested)
                {
                    var accumulator = timer.ElapsedMilliseconds;

                    if (accumulator >= Dt)
                    {
                        timer.Restart();
                    }

                    if (accumulator > 3 * Dt)
                    {
                        accumulator = 3 * Dt;
                    }

                    while (accumulator >= Dt)
                    {
                        accumulator -= Dt;
                        _gameFrame(Frame);
                    }
                }
            }, _token);
        }

        public override void Stop()
        {
            _tokenSource?.Cancel();
            _tokenSource = null;
        }
    }
}
