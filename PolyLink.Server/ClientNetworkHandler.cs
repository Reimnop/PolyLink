using PolyLink.Common.Packet;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public class ClientNetworkHandler(ConnectedProfile connection, IWebSocketService webSocketService)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var receiveTask = ReceiveInLoopAsync(cancellationToken);
        var heartbeatTask = SendHeartbeatInLoopAsync(cancellationToken);
        await Task.WhenAll(receiveTask, heartbeatTask);
    }

    private async Task ReceiveInLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await connection.Handler.ReceiveAsync(cancellationToken);
        }
    }

    private async Task SendHeartbeatInLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await SendHeartbeatAsync(cancellationToken);
            await Task.Delay(5000, cancellationToken);
        }
    }
    
    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        try
        {
            await connection.Handler.SendAsync(new S2CHeartbeatPacket(), cancellationToken);
            
            var cts = new CancellationTokenSource();
            var bothCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
            await connection.Handler.WaitUntilPacketAsync<C2SHeartbeatPacket>(bothCts.Token);
            cts.CancelAfter(5000);
        }
        catch
        {
            await webSocketService.RemoveConnectionAsync(connection.Profile);
        }
    }
}