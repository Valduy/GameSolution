using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameLoops
{
    public abstract class FixedFpsGameLoopBase
    {
        public long Dt { get; }
        public double Frame { get; }


        public FixedFpsGameLoopBase(uint fps)
        {
            Dt = 1000 / fps;
            Frame = (double)Dt / 1000;
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
