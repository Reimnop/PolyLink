using System.Numerics;

namespace PolyLink.Server.Model;

public class Player
{
    public required string SessionId { get; set; }
    public required int PlayerId { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
}