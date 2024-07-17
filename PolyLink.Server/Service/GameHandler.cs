using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Server.Model;
using PolyLink.Server.SignalR;
using PolyLink.Server.Util;

namespace PolyLink.Server.Service;

public class GameHandler
{
    // TODO: Switch to ConcurrentDictionary when shit hits the fan
    private readonly Dictionary<string, Player> sessionIdToPlayerMap = [];
    private readonly Dictionary<int, Player> playerIdToPlayerMap = [];
    private readonly Dictionary<int, GamePlayer> playerIdToGamePlayerMap = [];
    
    private readonly IHubContext<GameHub> hubContext;
    private readonly NetworkHandler networkHandler;

    private int checkpointIndex;
    private bool checkpointIndexChanged;
    
    public GameHandler(IEnumerable<Session> sessions, IHubContext<GameHub> hubContext)
    {
        this.hubContext = hubContext;
        
        var players = sessions
            .Indexed()
            .Select(x =>
                new Player
                {
                    PlayerId = x.Index,
                    ConnectionId = x.Value.ConnectionId,
                    DisplayName = x.Value.DisplayName
                })
            .ToList();
        networkHandler = new NetworkHandler(hubContext, players);
        
        foreach (var player in players)
        {
            sessionIdToPlayerMap[player.ConnectionId] = player;
            playerIdToPlayerMap[player.PlayerId] = player;
            playerIdToGamePlayerMap[player.PlayerId] = new GamePlayer(player, networkHandler);
        }
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
        var gamePlayer = playerIdToGamePlayerMap[playerId];
        gamePlayer.Position = position;
    }
    
    public Task HurtPlayerAsync(int playerId)
    {
        var gamePlayer = playerIdToGamePlayerMap[playerId];
        var newHealth = Math.Max(gamePlayer.Health - 1, 0); // Make sure health doesn't go below 0
        gamePlayer.Health = newHealth;
        gamePlayer.Dead = newHealth <= 0;
        return Task.CompletedTask;
    }
    
    public async Task TickAsync(float delta, float time, CancellationToken cancellationToken)
    {
        // If checkpoint index changed, broadcast to clients, then revive all players
        if (checkpointIndexChanged)
        {
            checkpointIndexChanged = false;
            await networkHandler.BroadcastActivateCheckpointPacket(checkpointIndex, cancellationToken: cancellationToken);
            
            foreach (var gamePlayer in playerIdToGamePlayerMap.Values.Where(gamePlayer => gamePlayer.Dead))
            {
                gamePlayer.SilentlySetDead(false);
                gamePlayer.SilentlySetHealth(3);
            }
        }
        
        // Update all players
        foreach (var gamePlayer in playerIdToGamePlayerMap.Values)
            await gamePlayer.TickAsync(delta, time, cancellationToken);
        
        // If all players are dead, rewind game and silently set all players to alive
        if (playerIdToGamePlayerMap.Values.All(p => p.Dead))
        {
            foreach (var gamePlayer in playerIdToGamePlayerMap.Values)
            {
                gamePlayer.SilentlySetDead(false);
                gamePlayer.SilentlySetHealth(3);
            }
            
            await networkHandler.BroadcastRewindToCheckpointPacket(checkpointIndex, cancellationToken: cancellationToken);
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