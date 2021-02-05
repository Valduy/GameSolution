using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameLoops
{
    public class FixedFpsGameLoop
    {
        private readonly Action<double> _gameFrame;
        private readonly long _dt;
        private readonly double _frame;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public FixedFpsGameLoop(Action<double> gameFrame, uint fps)
        {
            _gameFrame = gameFrame;
            _dt = 1000 / fps;
            _frame = (double)_dt / 1000;
        }

        public void Start()
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

                    if (accumulator >= _dt)
                    {
                        timer.Restart();
                    }

                    if (accumulator > 3 * _dt)
                    {
                        accumulator = 3 * _dt;
                    }

                    while (accumulator >= _dt)
                    {
                        accumulator -= _dt;
                        _gameFrame(_frame);
                    }
                }
            }, _token);
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            _tokenSource = null;
        }
    }
}
