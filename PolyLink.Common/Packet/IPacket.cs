namespace PolyLink.Common.Packet;

public interface IPacket
{
    int Write(Memory<byte> buffer);
    int Read(ReadOnlyMemory<byte> buffer);
}