using System.Collections.Concurrent;
using System.Net.WebSockets;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public class WebSocketService : IWebSocketService
{
    public event EventHandler<ConnectedProfile>? ConnectionAdded;
    public event EventHandler<ConnectedProfile>? ConnectionRemoved;
    
    private readonly ConcurrentDictionary<int, ConnectedProfile> connections = [];

    public Task AddConnectionAsync(Profile profile, WebSocket webSocket, TaskCompletionSource tcs)
    {
        var connection = new ConnectedProfile(profile, webSocket, tcs);
        if (!connections.TryAdd(profile.Id, connection))
            throw new InvalidOperationException("Profile is already connected");
        ConnectionAdded?.Invoke(this, connection);
        return Task.CompletedTask;
    }

    public async Task RemoveConnectionAsync(Profile profile)
    {
        if (connections.TryRemove(profile.Id, out var connection))
        {
            ConnectionRemoved?.Invoke(this, connection);
            var webSocket = connection.WebSocket;
            var tcs = connection.WebSocketTcs;
            
            // Check if WebSocket is still open, if it is, close it
            if (!webSocket.CloseStatus.HasValue)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            
            webSocket.Dispose();
            tcs.SetResult();
        }
    }

    public async Task PruneConnectionsAsync()
    {
        foreach (var connection in connections.Values)
        {
            if (connection.WebSocket.CloseStatus.HasValue)
                await RemoveConnectionAsync(connection.Profile);
        }
    }

    public IEnumerable<ConnectedProfile> GetConnections()
    {
        return connections.Values;
    }
}