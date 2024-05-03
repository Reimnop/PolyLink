using PolyLink.Common.Model;

namespace PolyLink.Server.Service;

public interface ISessionService
{
    Task<Profile> CreateProfileAsync(ProfileInfo profileInfo);
    Task<Profile?> GetProfileByNameAsync(string name);
    Task<Profile?> GetProfileByTokenAsync(string token);
    Task DeleteProfileAsync(Profile profile);
}