using PolyLink.Common.Data;

namespace PolyLink.Common.Packet;

public class S2CUpdatePlayerPositionsPacket
{
    public ICollection<PlayerPosition> PlayerPositions { get; set; }
}