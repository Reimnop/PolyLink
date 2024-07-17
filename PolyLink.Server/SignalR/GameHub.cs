using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.Service;

namespace PolyLink.Server.SignalR;

public class GameHub(ISessionRepository sessionRepository, IGameService gameService, ILogger<GameHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Check for name and display name
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            Context.Abort();
            return;
        }
        
        var clientIdValues = httpContext.Request.Query["clientId"];
        var displayNameValues = httpContext.Request.Query["displayName"];
        
        var clientIdString = clientIdValues.Count > 0 ? clientIdValues[0] : null;
        var displayName = displayNameValues.Count > 0 ? displayNameValues[0] : null;
        
        // Check if client ID is null
        if (string.IsNullOrWhiteSpace(clientIdString))
        {
            Context.Abort();
            return;
        }
        
        // Check if client ID is valid
        if (!ulong.TryParse(clientIdString, out var clientId))
        {
            Context.Abort();
            return;
        }
        
        // Check if display name is null
        if (string.IsNullOrWhiteSpace(displayName))
        {
            Context.Abort();
            return;
        }
        
        // Check if there's an existing session
        if (await sessionRepository.GetSessionByClientIdAsync(clientId) != null)
        {
            Context.Abort();
            return;
        }
        
        // Create SignalR session instance
        var session = new Session
        {
            ClientId = clientId,
            ConnectionId = Context.ConnectionId,
            DisplayName = displayName
        };
        
        // Add session
        await sessionRepository.AddSessionAsync(session);
        
        logger.LogInformation("Player '{}' connected", session.DisplayName);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var session = await sessionRepository.GetSessionByConnectionIdAsync(connectionId);
        if (session == null)
            return;
        await sessionRepository.RemoveSessionAsync(session);
        
        logger.LogInformation("Player '{}' disconnected", session.DisplayName);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task ActivateCheckpoint(ActivateCheckpointPacket packet)
    {
        await gameService.ActivateCheckpointAsync(packet.CheckpointIndex);
    }

    public async Task UpdatePlayerPosition(C2SUpdatePlayerPositionPacket packet)
    {
        // Get session
        var session = await sessionRepository.GetSessionByConnectionIdAsync(Context.ConnectionId);
        if (session == null)
            return;
        
        // Get player to update position
        var player = await gameService.GetPlayerFromSessionAsync(session.ConnectionId);
        if (player == null)
            return;
        
        await gameService.UpdatePlayerPositionAsync(player.PlayerId, new Vector2(packet.X, packet.Y));
    }

    public async Task HurtPlayer()
    {
        // Get session
        var session = await sessionRepository.GetSessionByConnectionIdAsync(Context.ConnectionId);
        if (session == null)
            return;
        
        // Get player to update health
        var player = await gameService.GetPlayerFromSessionAsync(session.ConnectionId);
        if (player == null)
            return;
        
        await gameService.HurtPlayerAsync(player.PlayerId);
    }
}