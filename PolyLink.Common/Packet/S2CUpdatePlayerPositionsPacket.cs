namespace PolyLink.Common.Packet;

public class S2CUpdatePlayerPositionPacket
{
    public int PlayerId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
}