using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    public class GameLoop
    {
        private readonly Action<double> _gameFrame;
        private readonly long _DT;
        private readonly double _frame;

        private bool _isRun;
        private Task _gameTask;

        public GameLoop(Action<double> gameFrame, uint framesPerSeconds)
        {
            _gameFrame = gameFrame;
            _DT = 1000 / framesPerSeconds;
            _frame = (double)_DT / 1000;
        }

        public void Start()
        {
            _gameTask = Task.Run(() =>
            {
                _isRun = true;
                var timer = new Stopwatch();
                timer.Start();

                while (_isRun)
                {
                    var accumulator = timer.ElapsedMilliseconds;

                    if (accumulator >= _DT)
                    {
                        timer.Restart();
                    }

                    if (accumulator > 3 * _DT)
                    {
                        accumulator = 3 * _DT;
                    }

                    while (accumulator >= _DT)
                    {
                        accumulator -= _DT;
                        _gameFrame(_frame);
                    }
                }
            });
        }

        public void Stop()
        {
            _isRun = false;
            _gameTask.Wait();
        }
    }
}
