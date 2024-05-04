using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PolyLink.Common.Model;
using PolyLink.Server.Controller;
using PolyLink.Server.Util;
using Profile = PolyLink.Server.Model.Profile;

namespace PolyLink.Server.Service;

public class ProfileRepository(PolyLinkContext context) : IProfileRepository
{
    public async Task<Profile> CreateProfileAsync(ProfileInfo profileInfo)
    {
        // Check if name is valid
        if (!RegexHelper.NameValidator().IsMatch(profileInfo.Name))
            throw new Exception("The name of the profile contains invalid characters");
        
        // Check if profile already exists
        if (await GetProfileByNameAsync(profileInfo.Name) != null)
            throw new Exception("A profile with that name already exists");
        
        var profile = new Profile
        {
            Id = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyz0123456789", 8),
            Name = profileInfo.Name,
            DisplayName = profileInfo.DisplayName,
            LoginToken = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyz0123456789", 32)
        };
        
        // Add user to database
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();
        
        return profile;
    }

    public async Task<Profile?> GetProfileByNameAsync(string name)
    {
        return await context.Profiles
            .Include(x => x.Session)
            .FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<Profile?> GetProfileByTokenAsync(string token)
    {
        return await context.Profiles
            .Include(x => x.Session)
            .FirstOrDefaultAsync(x => x.LoginToken == token);
    }

    public async Task DeleteProfileAsync(Profile profile)
    {
        context.Profiles.Remove(profile);
        await context.SaveChangesAsync();
    }
}