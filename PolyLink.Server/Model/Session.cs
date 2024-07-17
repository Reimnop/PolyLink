using System.ComponentModel.DataAnnotations;

namespace PolyLink.Server.Model;

public class Session
{
    [Key]
    public required ulong ClientId { get; set; }
    public required string ConnectionId { get; set; }
    public required string DisplayName { get; set; }
}