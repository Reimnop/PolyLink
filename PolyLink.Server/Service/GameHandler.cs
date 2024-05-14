using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Data;
using PolyLink.Server.Model;
using PolyLink.Server.SignalR;
using PolyLink.Server.Util;

namespace PolyLink.Server.Service;

public class GameHandler
{
    private readonly Dictionary<string, Player> sessionIdToPlayerMap = [];
    private readonly Dictionary<int, Player> playerIdToPlayerMap = [];
    
    private readonly IHubContext<GameHub> hubContext;

    private int checkpointIndex;
    private bool checkpointIndexChanged;
    
    private bool playerPositionsChanged;
    
    public GameHandler(IEnumerable<Session> sessions, IHubContext<GameHub> hubContext)
    {
        foreach (var (i, session) in sessions.Indexed())
        {
            var player = new Player
            {
                PlayerId = i,
                SessionId = session.Id,
                Name = session.Name,
                DisplayName = session.DisplayName
            };
            sessionIdToPlayerMap[player.SessionId] = player;
            playerIdToPlayerMap[player.PlayerId] = player;
        }
        this.hubContext = hubContext;
    }
    
    public Task ActivateCheckpointAsync(int checkpointIndex)
    {
        if (this.checkpointIndex != checkpointIndex)
        {
            this.checkpointIndex = checkpointIndex;
            checkpointIndexChanged = true;
        }
        return Task.CompletedTask;
    }
    
    public async Task UpdatePlayerPositionAsync(int playerId, Vector2 position)
    {
        var player = await GetPlayerFromPlayerIdAsync(playerId);
        if (player == null)
            return;
        player.Position = position;
        playerPositionsChanged = true;
    }
    
    public async Task TickAsync(float delta, float time, CancellationToken cancellationToken)
    {
        // If checkpoint index changed, broadcast to clients
        if (checkpointIndexChanged)
        {
            checkpointIndexChanged = false;
            await hubContext.Clients.All.SendAsync("ActivateCheckpoint", checkpointIndex, cancellationToken);
        }
        
        // If player positions changed, broadcast to clients
        if (playerPositionsChanged)
        {
            playerPositionsChanged = false;
            var playerPositions = playerIdToPlayerMap.Values
                .Select(player => new PlayerPosition
                {
                    PlayerId = player.PlayerId,
                    X = player.Position.X,
                    Y = player.Position.Y
                });
            await hubContext.Clients
                .Clients(GetPlayers().Select(x => x.SessionId))
                .SendAsync("UpdatePlayerPositions", playerPositions, cancellationToken);
        }
    }
    
    public Task<Player?> GetPlayerFromSessionAsync(string sessionId)
    {
        if (sessionIdToPlayerMap.TryGetValue(sessionId, out var player))
            return Task.FromResult<Player?>(player);
        return Task.FromResult<Player?>(null);
    }
    
    public Task<Player?> GetPlayerFromPlayerIdAsync(int playerId)
    {
        if (playerIdToPlayerMap.TryGetValue(playerId, out var player))
            return Task.FromResult<Player?>(player);
        return Task.FromResult<Player?>(null);
    }
    
    public IEnumerable<Player> GetPlayers()
    {
        return playerIdToPlayerMap.Values;
    }
}