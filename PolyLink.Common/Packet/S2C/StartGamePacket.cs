using PolyLink.Common.Data;

namespace PolyLink.Common.Packet.S2C;

public class StartGamePacket
{
    public ulong LevelId { get; set; }
    public int LocalPlayerId { get; set; }
    public ICollection<PlayerInfo> Players { get; set; }
}