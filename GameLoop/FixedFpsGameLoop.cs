using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameLoops
{
    public class FixedFpsGameLoop : IDisposable
    {
        private readonly Action<double> _gameFrame;

        private CancellationTokenSource _tokenSource;

        public long Dt { get; }
        public double Frame { get; }

        public FixedFpsGameLoop(Action<double> gameFrame, uint fps)
        {
            _gameFrame = gameFrame;
            Dt = 1000 / fps;
            Frame = (double)Dt / 1000;
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                var timer = new Stopwatch();
                timer.Start();

                while (true)
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();
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
            }, _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        public void Dispose() => Stop();
    }
}
