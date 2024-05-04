using PolyLink.Common.Model;
using Profile = PolyLink.Server.Model.Profile;

namespace PolyLink.Server.Service;

public interface IProfileRepository
{
    Task<Profile> CreateProfileAsync(ProfileInfo profileInfo);
    Task<Profile?> GetProfileByNameAsync(string name);
    Task<Profile?> GetProfileByTokenAsync(string token);
    Task DeleteProfileAsync(Profile profile);
}