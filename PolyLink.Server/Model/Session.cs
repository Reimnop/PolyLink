using System.ComponentModel.DataAnnotations.Schema;
using PolyLink.Common.Model;

namespace PolyLink.Server.Model;

public class Session
{
    public required string Id { get; set; }
    public required string ProfileId { get; set; }
    public required Profile Profile { get; set; }
}