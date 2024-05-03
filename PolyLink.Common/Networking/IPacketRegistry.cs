using PolyLink.Common.Packet;

namespace PolyLink.Common.Networking;

public interface IPacketRegistry
{
    int GetPacketId<T>() where T : IPacket
    {
        return GetPacketId(typeof(T));   
    }
    int GetPacketId(Type type);
    IPacket GetPacket(int id);
}