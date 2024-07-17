using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.SignalR;

namespace PolyLink.Server.Service;

public class NetworkHandler(IHubContext<GameHub> hubContext, IEnumerable<Player> players)
{
    private readonly List<Player> players = players.ToList();

    public async Task BroadcastUpdatePlayerPositionPacket(
        int playerId, 
        Vector2 position, 
        Predicate<int>? playerIdPredicate = null,
        CancellationToken cancellationToken = default)
    {
        await BroadcastPacketToAllPlayersAsync(
            "UpdatePlayerPosition", 
            new S2CUpdatePlayerPositionPacket
            {
                PlayerId = playerId,
                X = position.X,
                Y = position.Y
            }, 
            playerIdPredicate,
            cancellationToken);
    }
    
    public async Task BroadcastSetPlayerHealthPacket(
        int playerId, 
        int health, 
        bool playHurtAnimation = true, 
        Predicate<int>? playerIdPredicate = null,
        CancellationToken cancellationToken = default)
    {
        await BroadcastPacketToAllPlayersAsync(
            "SetPlayerHealth", 
            new SetPlayerHealthPacket
            {
                PlayerId = playerId,
                Health = health,
                PlayHurtAnimation = playHurtAnimation
            }, 
            playerIdPredicate,
            cancellationToken);
    }
    
    public async Task BroadcastKillPlayerPacket(
        int playerId, 
        Predicate<int>? playerIdPredicate = null,
        CancellationToken cancellationToken = default)
    {
        await BroadcastPacketToAllPlayersAsync(
            "KillPlayer", 
            new KillPlayerPacket
            {
                PlayerId = playerId
            }, 
            playerIdPredicate,
            cancellationToken);
    }
    
    public async Task BroadcastActivateCheckpointPacket(
        int checkpointIndex, 
        Predicate<int>? playerIdPredicate = null,
        CancellationToken cancellationToken = default)
    {
        await BroadcastPacketToAllPlayersAsync(
            "ActivateCheckpoint", 
            new ActivateCheckpointPacket
            {
                CheckpointIndex = checkpointIndex
            }, 
            playerIdPredicate,
            cancellationToken);
    }
    
    public async Task BroadcastRewindToCheckpointPacket(
        int checkpointIndex, 
        Predicate<int>? playerIdPredicate = null,
        CancellationToken cancellationToken = default)
    {
        await BroadcastPacketToAllPlayersAsync(
            "RewindToCheckpoint", 
            new RewindToCheckpointPacket
            {
                CheckpointIndex = checkpointIndex
            }, 
            playerIdPredicate,
            cancellationToken);
    }

    public Task BroadcastPacketToAllPlayersAsync<T>(
        string methodName, 
        T packet,
        Predicate<int>? playerIdPredicate = null,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients
            .Clients(players
                .Where(x => playerIdPredicate?.Invoke(x.PlayerId) ?? true)
                .Select(x => x.ConnectionId))
            .SendAsync(methodName, packet, cancellationToken);
    }
}