namespace PolyLink.Common.Packet;

public class S2CHeartbeatPacket : IPacket
{
    // Do nothing
    public int Write(Memory<byte> buffer) => 0;
    public int Read(ReadOnlyMemory<byte> buffer) => 0;
}