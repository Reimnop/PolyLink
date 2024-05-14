using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Data;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.SignalR;

namespace PolyLink.Server.Service;

public class GameService(IHubContext<GameHub> hubContext) : IGameService
{
    public bool IsGameRunning => gameHandler != null;
    
    private GameHandler? gameHandler;

    public async Task StartGameAsync(IEnumerable<Session> sessions, ulong levelId)
    {
        if (gameHandler != null)
            throw new InvalidOperationException("Game is already running!");
        gameHandler = new GameHandler(sessions, hubContext);
        foreach (var player in gameHandler.GetPlayers())
        {
            var client = hubContext.Clients.Client(player.SessionId);
            await client.SendAsync("StartGame", new StartGamePacket
            {
                LevelId = levelId,
                LocalPlayerId = player.PlayerId,
                Players = gameHandler.GetPlayers().Select(p => new PlayerInfo
                {
                    Id = p.PlayerId,
                    DisplayName = p.DisplayName
                }).ToList()
            });
        }
    }

    public Task StopGameAsync()
    {
        if (gameHandler == null)
            throw new InvalidOperationException("Game is not running!");
        gameHandler = null;
        return Task.CompletedTask;
    }

    public async Task ActivateCheckpointAsync(int checkpointIndex)
    {
        if (gameHandler == null)
            throw new InvalidOperationException("Game is not running!");
        await gameHandler.ActivateCheckpointAsync(checkpointIndex);
    }

    public Task UpdatePlayerPositionAsync(int playerId, Vector2 position)
    {
        if (gameHandler == null)
            throw new InvalidOperationException("Game is not running!");
        return gameHandler.UpdatePlayerPositionAsync(playerId, position);
    }

    public async Task TickAsync(float delta, float time, CancellationToken cancellationToken)
    {
        if (gameHandler == null)
            return;
        await gameHandler.TickAsync(delta, time, cancellationToken);
    }

    public async Task<Player?> GetPlayerFromSessionAsync(string sessionId)
    {
        if (gameHandler == null)
            return null;
        return await gameHandler.GetPlayerFromSessionAsync(sessionId);
    }

    public async Task<Player?> GetPlayerFromPlayerIdAsync(int playerId)
    {
        if (gameHandler == null)
            return null;
        return await gameHandler.GetPlayerFromPlayerIdAsync(playerId);
    }

    public IEnumerable<Player> GetPlayers()
    {
        if (gameHandler == null)
            throw new InvalidOperationException("Game is not running!");
        return gameHandler.GetPlayers();
    }
}