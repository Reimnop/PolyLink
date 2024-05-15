using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.Service;
using PolyLink.Server.Util;

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
        
        var nameValues = httpContext.Request.Query["name"];
        var displayNameValues = httpContext.Request.Query["displayName"];

        var name = nameValues.Count > 0 ? nameValues[0] : null;
        var displayName = displayNameValues.Count > 0 ? displayNameValues[0] : null;
        
        // Check if either name or display name is null
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(displayName))
        {
            Context.Abort();
            return;
        }
        
        // Check if name is valid
        if (!RegexHelper.NameValidator().IsMatch(name))
        {
            Context.Abort();
            return;
        }
        
        // Check if there's an existing session
        if (await sessionRepository.GetSessionByNameAsync(name) != null)
        {
            Context.Abort();
            return;
        }
        
        // Get SignalR client
        var id = Context.ConnectionId;
        var session = new Session
        {
            Id = id,
            Name = name,
            DisplayName = displayName
        };
        
        // Add session
        await sessionRepository.AddSessionAsync(session);
        
        logger.LogInformation("Player '{}' connected", session.DisplayName);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        var session = await sessionRepository.GetSessionByIdAsync(id);
        if (session == null)
            return;
        await sessionRepository.RemoveSessionAsync(session);
        
        logger.LogInformation("Player '{}' disconnected", session.DisplayName);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task ActivateCheckpoint(ActivateCheckpointPacket packet)
    {
        logger.LogInformation("Received ActivateCheckpoint packet with checkpoint index: {}", packet.CheckpointIndex);
        await gameService.ActivateCheckpointAsync(packet.CheckpointIndex);
    }

    public async Task UpdatePlayerPosition(C2SUpdatePlayerPositionPacket packet)
    {
        var session = await sessionRepository.GetSessionByIdAsync(Context.ConnectionId);
        if (session == null)
            return;
        
        // Get player to update position
        var player = await gameService.GetPlayerFromSessionAsync(session.Id);
        if (player == null)
            return;
        
        await gameService.UpdatePlayerPositionAsync(player.PlayerId, new Vector2(packet.X, packet.Y));
    }

    public async Task HurtPlayer()
    {
        var session = await sessionRepository.GetSessionByIdAsync(Context.ConnectionId);
        if (session == null)
            return;
        
        // Get player to update health
        var player = await gameService.GetPlayerFromSessionAsync(session.Id);
        if (player == null)
            return;
        
        await gameService.HurtPlayerAsync(player.PlayerId);
    }
}