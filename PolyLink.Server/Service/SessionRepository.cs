using Microsoft.EntityFrameworkCore;
using PolyLink.Server.Controller;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public class SessionRepository(PolyLinkContext context) : ISessionRepository
{
    public async Task AddSessionAsync(Session session)
    {
        await context.Sessions.AddAsync(session);
        await context.SaveChangesAsync();
    }

    public async Task RemoveSessionAsync(Session session)
    {
        context.Sessions.Remove(session);
        await context.SaveChangesAsync();
    }
    
    public Task<Session?> GetSessionByClientIdAsync(ulong clientId)
    {
        return context.Sessions.FirstOrDefaultAsync(x => x.ClientId == clientId);
    }
    
    public async Task<Session?> GetSessionByConnectionIdAsync(string connectionId)
    {
        return await context.Sessions.FirstOrDefaultAsync(x => x.ConnectionId == connectionId);
    }

    public async Task<int> GetSessionCountAsync()
    {
        return await context.Sessions.CountAsync();
    }

    public IAsyncEnumerable<Session> GetSessionsAsync()
    {
        return context.Sessions.AsAsyncEnumerable();
    }
}