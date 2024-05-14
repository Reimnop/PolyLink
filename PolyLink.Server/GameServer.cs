using System.Diagnostics;

namespace PolyLink.Server.Service;

public class GameServer
{
    private delegate Task LoopedTaskFactory(float delta, float time, CancellationToken cancellationToken);
    
    private readonly WebApplication webApplication;

    public GameServer(WebApplication webApplication)
    {
        this.webApplication = webApplication; 
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var gameLoop = FixedRateLoop(UpdateGameLogicAsync, cancellationToken);
        var webAppRun = webApplication.RunAsync(cancellationToken);
        await Task.WhenAll(gameLoop, webAppRun);
    }

    private async Task FixedRateLoop(LoopedTaskFactory loopedTaskFactory, CancellationToken cancellationToken)
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
                accumulator -= tickRate;

                await loopedTaskFactory(tickRate, time, cancellationToken);
            }

            await Task.Yield();
        }
        stopwatch.Stop();
    }

    private async Task UpdateGameLogicAsync(float delta, float time, CancellationToken cancellationToken)
    {
        var gameService = webApplication.Services.GetRequiredService<IGameService>();
        await gameService.TickAsync(delta, time, cancellationToken);
    }
}