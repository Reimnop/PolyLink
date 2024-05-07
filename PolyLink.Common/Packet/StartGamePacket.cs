using PolyLink.Common.Data;

namespace PolyLink.Common.Packet;

public class StartGamePacket
{
    public ulong LevelId { get; set; }
    public ICollection<PlayerInfo> Players { get; set; }
}