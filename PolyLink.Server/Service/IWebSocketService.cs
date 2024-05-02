using System.Net.WebSockets;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public interface IWebSocketService
{
    event EventHandler<ConnectedProfile>? ConnectionAdded;
    event EventHandler<ConnectedProfile>? ConnectionRemoved;
    
    Task AddConnectionAsync(Profile profile, WebSocket webSocket, TaskCompletionSource tcs);
    Task RemoveConnectionAsync(Profile profile);
    Task PruneConnectionsAsync();
    IEnumerable<ConnectedProfile> GetConnections();
}