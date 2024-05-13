namespace PolyLink.Common.Packet.S2C;

public class SetPlayerHealthPacket
{
    public int PlayerId { get; set; }
    public int Health { get; set; }
    public bool PlayHurtAnimation { get; set; }
}