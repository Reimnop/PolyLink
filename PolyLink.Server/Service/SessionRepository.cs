﻿using Microsoft.EntityFrameworkCore;
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
    
    public async Task<Session?> GetSessionByIdAsync(string id)
    {
        return await context.Sessions
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}