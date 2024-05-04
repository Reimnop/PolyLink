using System.Diagnostics;

namespace PolyLink.Server.Service;

public class GameServer
{
    private readonly WebApplication webApplication;

    public GameServer(WebApplication webApplication)
    {
        this.webApplication = webApplication; 
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var gameLoop = FixedRateLoop(cancellationToken);
        var webAppRun = webApplication.RunAsync(cancellationToken);
        await Task.WhenAll(gameLoop, webAppRun);
    }

    private async Task FixedRateLoop(CancellationToken cancellationToken)
    {
        const float tickRate = 0.05f; // 20 ticks per second
        
        var stopwatch = Stopwatch.StartNew();
        var lastTime = 0.0f;
        var accumulator = 0.0f;
        while (!cancellationToken.IsCancellationRequested)
        {
            var time = (float) stopwatch.ElapsedTicks / Stopwatch.Frequency;
            var delta = time - lastTime;
            lastTime = time;
            
            accumulator += delta;
            while (accumulator >= tickRate)
            {
                // Update game state
                accumulator -= tickRate;

                await UpdateGameLogicAsync(delta, time, cancellationToken);
            }

            await Task.Yield();
        }
        stopwatch.Stop();
    }

    private async Task UpdateGameLogicAsync(float delta, float time, CancellationToken cancellationToken)
    {
    }
}