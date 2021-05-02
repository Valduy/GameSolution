using System.Threading.Tasks;
using GameLoops;
using Xunit;

namespace UnitTests.GameLoops
{
    public class FixedFpsGameLoopTests
    {
        [Fact]
        public async Task Start_IncrementAsGameFrame_CounterNear60AfterSecondOfIterations()
        {
            // Arrange
            uint fps = 60;
            int error = 10;
            double actualFps = 0;
            var gameLoop = new FixedFpsGameLoop((dt) => actualFps++, fps);

            // Act
            await Task.Run(async () =>
            {
                gameLoop.Start();
                await Task.Delay(1000);
                gameLoop.Stop();
            });

            // Assert
            Assert.InRange(actualFps, fps - error, fps + error);
        }
    }
}
