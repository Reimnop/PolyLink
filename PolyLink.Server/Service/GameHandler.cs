using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.SignalR;
using PolyLink.Server.Util;

namespace PolyLink.Server.Service;

public class GameHandler
{
    private record struct IndividualPlayerData(
        Vector2 Position, bool PositionChanged, 
        int Health, bool HealthChanged,
        bool Dead, bool DeathChanged);
    
    // TODO: Switch to ConcurrentDictionary when shit hits the fan
    private readonly Dictionary<string, Player> sessionIdToPlayerMap = [];
    private readonly Dictionary<int, Player> playerIdToPlayerMap = [];
    private readonly Dictionary<int, IndividualPlayerData> playerIdToPlayerDataMap = [];
    
    private readonly IHubContext<GameHub> hubContext;

    private int checkpointIndex;
    private bool checkpointIndexChanged;
    
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
            playerIdToPlayerDataMap[player.PlayerId] = new IndividualPlayerData(
                Vector2.Zero, false,
                3, false,
                false, false);
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
        var playerData = playerIdToPlayerDataMap[playerId];
        playerIdToPlayerDataMap[playerId] = playerData with { Position = position, PositionChanged = true };
    }
    
    public Task HurtPlayerAsync(int playerId)
    {
        var playerData = playerIdToPlayerDataMap[playerId];
        var newHealth = playerData.Health - 1;
        playerIdToPlayerDataMap[playerId] = playerData with { Health = newHealth, HealthChanged = true };
        if (newHealth == 0)
            playerIdToPlayerDataMap[playerId] = playerData with { Dead = true, DeathChanged = true };
        return Task.CompletedTask;
    }
    
    public async Task TickAsync(float delta, float time, CancellationToken cancellationToken)
    {
        // If checkpoint index changed, broadcast to clients
        if (checkpointIndexChanged)
        {
            checkpointIndexChanged = false;
            await SendPacketToAllClientsAsync("ActivateCheckpoint", new ActivateCheckpointPacket
            {
                CheckpointIndex = checkpointIndex
            }, cancellationToken);
        }
        
        // If player positions changed, broadcast to clients
        foreach (var playerId in playerIdToPlayerMap.Keys)
        {
            if (!playerIdToPlayerDataMap.TryGetValue(playerId, out var playerData))
                continue;
            if (!playerData.PositionChanged)
                continue;
            
            await SendPacketToAllClientsAsync("UpdatePlayerPosition", new S2CUpdatePlayerPositionPacket
            {
                PlayerId = playerId,
                X = playerData.Position.X,
                Y = playerData.Position.Y
            }, cancellationToken);
            
            playerIdToPlayerDataMap[playerId] = playerData with { PositionChanged = false };
        }
        
        // If player health changed, broadcast to clients
        foreach (var playerId in playerIdToPlayerMap.Keys)
        {
            if (!playerIdToPlayerDataMap.TryGetValue(playerId, out var playerData))
                continue;
            if (!playerData.HealthChanged)
                continue;
            
            await SendPacketToAllClientsAsync("SetPlayerHealth", new SetPlayerHealthPacket
            {
                PlayerId = playerId,
                Health = playerData.Health,
                PlayHurtAnimation = true
            }, cancellationToken);
            
            playerIdToPlayerDataMap[playerId] = playerData with { HealthChanged = false };
        }
        
        // If player died, broadcast to clients
        foreach (var playerId in playerIdToPlayerMap.Keys)
        {
            if (!playerIdToPlayerDataMap.TryGetValue(playerId, out var playerData))
                continue;
            if (!playerData.DeathChanged)
                continue;

            if (playerData.Dead)
            {
                await SendPacketToAllClientsAsync("KillPlayer", new KillPlayerPacket
                {
                    PlayerId = playerId
                }, cancellationToken);
            }
            
            playerIdToPlayerDataMap[playerId] = playerData with { DeathChanged = false };
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

    private Task SendPacketToAllClientsAsync<T>(string methodName, T packet, CancellationToken cancellationToken = default)
    {
        return hubContext.Clients
            .Clients(GetPlayers().Select(x => x.SessionId))
            .SendAsync(methodName, packet, cancellationToken);
    }
}