using System.Collections.Concurrent;
using System.Diagnostics;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.Util;

namespace PolyLink.Server.Service;

public class GameServer
{
    private readonly WebApplication webApplication;
    private readonly IWebSocketService webSocketService;

    private readonly PeriodicAsyncTimer webSocketPruneTimer;
    private readonly ConcurrentQueue<ConnectedProfile> newConnections = [];
    private readonly List<Task> clientNetworkTasks = [];

    public GameServer(WebApplication webApplication)
    {
        this.webApplication = webApplication;
        
        webSocketService = webApplication.Services.GetRequiredService<IWebSocketService>();
        webSocketPruneTimer = new PeriodicAsyncTimer(20.0f, webSocketService.PruneConnectionsAsync);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        webSocketService.ConnectionAdded += OnConnectionAdded;
        webSocketService.ConnectionRemoved += OnConnectionRemoved;
        
        var gameLoop = FixedRateLoop(cancellationToken);
        var webAppRun = webApplication.RunAsync(cancellationToken);
        await Task.WhenAll(gameLoop, webAppRun);
    }
    
    private void OnConnectionAdded(object? sender, ConnectedProfile e)
    {
        newConnections.Enqueue(e);
        Console.WriteLine($"Player connected: {e.Profile.DisplayName} ({e.Profile.Name})");
    }

    private void OnConnectionRemoved(object? sender, ConnectedProfile e)
    {
        Console.WriteLine($"Player disconnected: {e.Profile.DisplayName} ({e.Profile.Name})");
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

            await Task.Yield(); // Some delay to prevent busy loop
        }
        stopwatch.Stop();
    }
    
    private async Task HandleConnections(float delta, CancellationToken cancellationToken)
    {
        while (newConnections.TryDequeue(out var connection))
        {
            var clientNetworkHandler = new ClientNetworkHandler(connection, webSocketService);
            var networkTask = clientNetworkHandler.RunAsync(cancellationToken);
            clientNetworkTasks.Add(networkTask);
        }

        // Make sure we don't miss any exceptions
        foreach (var networkTask in clientNetworkTasks)
        {
            if (networkTask.IsFaulted)
                throw networkTask.Exception!;
        }
        clientNetworkTasks.RemoveAll(task => task.IsCompleted);
        
        await webSocketPruneTimer.UpdateAsync(delta);
    }

    private async Task UpdateGameLogicAsync(float delta, float time, CancellationToken cancellationToken)
    {
        await HandleConnections(delta, cancellationToken);
    }
}