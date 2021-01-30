using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ECS.Tests
{
    public class GameLoopTests
    {
        [Fact]
        public void FpsFact()
        {
            uint fps = 60;
            double actualFps = 0;
            var gameLoop = new GameLoop((dt) => actualFps++, fps);

            var task = Task.Run(async () =>
            {
                gameLoop.Start();
                await Task.Delay(1000);
                gameLoop.Stop();
            });
            task.Wait();

            Assert.InRange(actualFps, fps - 10, fps + 10);
        }
    }
}
