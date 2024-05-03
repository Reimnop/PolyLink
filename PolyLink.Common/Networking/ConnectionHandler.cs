using System.Net.WebSockets;
using PolyLink.Common.Packet;

namespace PolyLink.Common.Networking;

public delegate Task ConnectionClosedCallback(ConnectionHandler connectionHandler);
public delegate Task PacketReceivedCallback(ConnectionHandler connectionHandler, IPacket packet);

public class ConnectionHandler(WebSocket socket, IPacketRegistry packetRegistry)
{
    public ConnectionClosedCallback? ConnectionClosed { get; set; }
    public PacketReceivedCallback? PacketReceived { get; set; }
    
    public IPacket? LastReceivedPacket { get; private set; }
    
    private readonly byte[] receiveBuffer = new byte[4096]; // 4 KiB receiveBuffer
    private readonly byte[] sendBuffer = new byte[4096]; // 4 KiB sendBuffer
    
    public async Task ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var result = await socket.ReceiveAsync(receiveBuffer, cancellationToken);
        
        switch (result.MessageType)
        {
            case WebSocketMessageType.Close:
                if (ConnectionClosed != null)
                    await ConnectionClosed(this);
                return;
            case WebSocketMessageType.Binary:
            {
                var packetId = BitConverter.ToInt32(receiveBuffer);
                var packet = packetRegistry.GetPacket(packetId);
            
                // Get packet memory
                var packetMemory = receiveBuffer.AsMemory(4);
                packet.Read(packetMemory);
                
                LastReceivedPacket = packet;
            
                if (PacketReceived != null)
                    await PacketReceived(this, packet);
                break;
            }
        }
    }
    
    public async Task SendAsync(IPacket packet, CancellationToken cancellationToken = default)
    {
        var packetId = packetRegistry.GetPacketId(packet.GetType());
        
        // Write packet id
        BitConverter.TryWriteBytes(sendBuffer, packetId);
        
        // Write packet data
        var packetMemory = sendBuffer.AsMemory(4);
        packet.Write(packetMemory);
        
        await socket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, cancellationToken);
    }
    
    public async Task<T> WaitUntilPacketAsync<T>(CancellationToken cancellationToken = default) where T : IPacket, new()
    {
        // Wait until we receive a packet
        while (LastReceivedPacket is not T && !cancellationToken.IsCancellationRequested)
            await Task.Yield();
        
        cancellationToken.ThrowIfCancellationRequested();

        return (T) LastReceivedPacket!;
    }
}