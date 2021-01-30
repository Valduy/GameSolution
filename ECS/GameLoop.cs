using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ECS
{
    public class GameLoop
    {
        private readonly Action<double> _gameFrame;
        private readonly long _dt;
        private readonly double _frame;

        private CancellationTokenSource _token;
        private Task _gameTask;

        public GameLoop(Action<double> gameFrame, uint fps)
        {
            _gameFrame = gameFrame;
            _dt = 1000 / fps;
            _frame = (double)_dt / 1000;
        }

        public void Start()
        {
            _token = new CancellationTokenSource();

            _gameTask = Task.Run(() =>
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
            });
        }

        public void Stop()
        {
            _token.Cancel();
            _gameTask.Wait();
            _gameTask = null;
            _token = null;
        }
    }
}
