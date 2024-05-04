using Microsoft.AspNetCore.SignalR;
using PolyLink.Server.Model;
using PolyLink.Server.Util;

namespace PolyLink.Server.Service.SignalR;

public class GameHub(IProfileRepository profileRepository, ISessionRepository sessionRepository, ILogger<GameHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Check for token
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            Context.Abort();
            return;
        }
        
        var authHeader = httpContext.Request.Headers.Authorization;
        if (authHeader.Count == 0)
        {
            Context.Abort();
            return;
        }
        
        var authorization = authHeader[0]!;
        var match = RegexHelper.TokenParser().Match(authorization);
        if (!match.Success)
        {
            Context.Abort();
            return;
        }
        
        var token = match.Groups[1].Value;
        var profileByToken = await profileRepository.GetProfileByTokenAsync(token);
        if (profileByToken == null)
        {
            Context.Abort();
            return;
        }
        
        // Get SignalR client
        var id = Context.ConnectionId;
        var session = new Session
        {
            Id = id,
            ProfileId = profileByToken.Id,
            Profile = profileByToken
        };
        
        // Add session
        await sessionRepository.AddSessionAsync(session);
        
        logger.LogInformation("User '{}' connected", profileByToken.DisplayName);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        var session = await sessionRepository.GetSessionByIdAsync(id);
        if (session == null)
            return;
        await sessionRepository.RemoveSessionAsync(session);
        
        logger.LogInformation("User '{}' disconnected", session.Profile.DisplayName);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(string message)
    {
        var id = Context.ConnectionId;
        var session = await sessionRepository.GetSessionByIdAsync(id);
        if (session == null)
            return;
        
        // Broadcast message to all clients except sender
        await Clients.AllExcept(id).SendAsync("ReceiveMessage", session.Profile.DisplayName, message);
    }
}