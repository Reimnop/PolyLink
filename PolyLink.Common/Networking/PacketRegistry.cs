using PolyLink.Common.Packet;

namespace PolyLink.Common.Networking;

public class PacketRegistry : IPacketRegistry
{
    public static PacketRegistry Default { get; } = new();
    
    private static readonly Dictionary<Type, int> PacketIds = new()
    {
        {typeof(S2CHeartbeatPacket), 0},
        {typeof(C2SHeartbeatPacket), 1}
    };
    
    private static readonly Dictionary<int, Func<IPacket>> PacketFactories = new()
    {
        {0, () => new S2CHeartbeatPacket()},
        {1, () => new C2SHeartbeatPacket()}
    };

    public int GetPacketId(Type type)
    {
        return PacketIds[type];
    }

    public IPacket GetPacket(int id)
    {
        return PacketFactories[id]();
    }
}