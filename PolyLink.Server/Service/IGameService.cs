using System.Numerics;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public interface IGameService
{
    bool IsGameRunning { get; }
    
    Task StartGameAsync(IEnumerable<Session> sessions, ulong levelId);
    Task StopGameAsync();
    
    Task ActivateCheckpointAsync(int checkpointIndex);
    Task UpdatePlayerPositionAsync(int playerId, Vector2 position);
    Task HurtPlayerAsync(int playerId);
    Task TickAsync(float delta, float time, CancellationToken cancellationToken);
    
    Task<Player?> GetPlayerFromSessionAsync(string sessionId);
    Task<Player?> GetPlayerFromPlayerIdAsync(int playerId);
    
    IEnumerable<Player> GetPlayers();
}