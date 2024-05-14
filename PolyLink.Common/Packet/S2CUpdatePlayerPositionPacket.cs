using System.Numerics;

namespace PolyLink.Common.Packet;

public class S2CUpdatePlayerPositionPacket
{
    public int PlayerId { get; set; }
    public Vector3 Position { get; set; }
}