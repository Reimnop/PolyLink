namespace PolyLink.Common.Packet;

public class C2SHeartbeatPacket : IPacket
{
    // Do nothing
    public int Write(Memory<byte> buffer) => 0;
    public int Read(ReadOnlyMemory<byte> buffer) => 0;
}