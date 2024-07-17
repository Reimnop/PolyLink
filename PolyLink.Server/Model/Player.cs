namespace PolyLink.Server.Model;

public class Player
{
    public required string ConnectionId { get; set; }
    public required int PlayerId { get; set; }
    public required string DisplayName { get; set; }
}