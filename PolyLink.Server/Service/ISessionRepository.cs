using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session);
    Task RemoveSessionAsync(Session session);
    Task<Session?> GetSessionByIdAsync(string id);
    Task<Session?> GetSessionByNameAsync(string name);
}