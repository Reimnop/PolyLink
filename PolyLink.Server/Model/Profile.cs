namespace PolyLink.Server.Model;

public class Profile
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string LoginToken { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required int ExpiresIn { get; set; }
}