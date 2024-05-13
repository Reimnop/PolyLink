namespace PolyLink.Common.Packet;

public class SetPlayerHealthPacket
{
    public int PlayerId { get; set; }
    public int Health { get; set; }
    public bool PlayHurtAnimation { get; set; }
}