using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session);
    Task RemoveSessionAsync(Session session);
    Task<Session?> GetSessionByClientIdAsync(ulong clientId);
    Task<Session?> GetSessionByConnectionIdAsync(string connectionId);
    Task<int> GetSessionCountAsync();
    IAsyncEnumerable<Session> GetSessionsAsync();
}