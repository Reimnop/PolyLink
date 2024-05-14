﻿using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.Model;
using PolyLink.Server.Service;
using PolyLink.Server.Util;

namespace PolyLink.Server.SignalR;

public class GameHub(ISessionRepository sessionRepository, ILogger<GameHub> logger) : Hub
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
        var session = await sessionRepository.GetSessionByIdAsync(Context.ConnectionId);
        if (session == null)
            return;
        logger.LogInformation("Player '{}' activated checkpoint index {}", session.DisplayName, packet.CheckpointIndex);
        
        // Broadcast to all clients except the sender
        await Clients.Others.SendAsync("ActivateCheckpoint", packet);
    }

    public async Task UpdatePlayerPosition(C2SUpdatePlayerPositionPacket packet)
    {
        var session = await sessionRepository.GetSessionByIdAsync(Context.ConnectionId);
        if (session == null)
            return;
        logger.LogInformation("Player '{}' updated position to {}", session.DisplayName, packet.Position);
        
        // Broadcast to all clients except the sender
        await Clients.Others.SendAsync("UpdatePlayerPosition", new S2CUpdatePlayerPositionPacket()
        {
            PlayerId = // TODO: Make game manager to store player id
        });
    }
}