using System.Net.WebSockets;
using PolyLink.Common.Model;
using PolyLink.Common.Networking;

namespace PolyLink.Server.Model;

// Don't touch WebSocketTcs, it's used to signal when the WebSocket is closed
// Call IWebSocketService.RemoveConnectionAsync to remove the connection
public record ConnectedProfile(Profile Profile, WebSocket WebSocket, ConnectionHandler Handler, TaskCompletionSource WebSocketTcs);