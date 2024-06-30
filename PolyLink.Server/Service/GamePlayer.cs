using System.Numerics;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public class GamePlayer(Player player, NetworkHandler networkHandler)
{
    public Vector2 Position { get; set; }
    public int Health { get; set; } = 3;
    public bool Dead { get; set; }

    private Vector2 lastPosition;
    private int lastHealth = 3;
    private bool lastDead;

    public void SilentlySetPosition(Vector2 position)
    {
        Position = position;
        lastPosition = position;
    }

    public void SilentlySetHealth(int health)
    {
        Health = health;
        lastHealth = health;
    }

    public void SilentlySetDead(bool dead)
    {
        Dead = dead;
        lastDead = dead;
    }

    public async Task TickAsync(float delta, float time, CancellationToken cancellationToken)
    {
        // If player position changed, broadcast to clients
        if (Position != lastPosition)
        {
            lastPosition = Position;
            await networkHandler.BroadcastUpdatePlayerPositionPacket(
                player.PlayerId, 
                Position, 
                x => x != player.PlayerId, // Don't broadcast to the client that moved
                cancellationToken);
        }
        
        // If player health changed, broadcast to clients
        if (Health != lastHealth)
        {
            lastHealth = Health;
            await networkHandler.BroadcastSetPlayerHealthPacket(
                player.PlayerId, 
                Health,
                cancellationToken: cancellationToken);
        }
        
        // If player died, broadcast to clients
        if (Dead != lastDead)
        {
            lastDead = Dead;
            if (Dead)
                await networkHandler.BroadcastKillPlayerPacket(player.PlayerId, cancellationToken: cancellationToken);
        }
    }
}