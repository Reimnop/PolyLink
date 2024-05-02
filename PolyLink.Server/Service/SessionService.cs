using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PolyLink.Server.Controller;
using PolyLink.Server.Model;

namespace PolyLink.Server.Service;

public partial class SessionService(PolyLinkContext context) : ISessionService
{
    public async Task<Profile> CreateProfileAsync(ProfileInfo profileInfo)
    {
        // Check if name is valid
        if (!NameValidator().IsMatch(profileInfo.Name))
            throw new Exception("The name of the profile contains invalid characters");
        
        // Check if profile already exists
        if (await GetProfileByNameAsync(profileInfo.Name) != null)
            throw new Exception("A profile with that name already exists");
        
        var profile = new Profile
        {
            Id = Random.Shared.Next(),
            Name = profileInfo.Name,
            DisplayName = profileInfo.DisplayName,
            LoginToken = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyz0123456789", 32),
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = 300 // 5 minutes
        };
        
        // Add user to database
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();
        
        return profile;
    }

    public async Task<Profile?> GetProfileByNameAsync(string name)
    {
        // Try to get profile from db
        var profile = await context.Profiles.FirstOrDefaultAsync(x => x.Name == name);
        if (profile == null)
            return null;
        
        // Check if profile is expired
        if (profile.CreatedAt.AddSeconds(profile.ExpiresIn) < DateTime.UtcNow)
        {
            context.Profiles.Remove(profile);
            await context.SaveChangesAsync();
            return null;
        }
        
        return profile;
    }

    public async Task<Profile?> GetProfileByTokenAsync(string token)
    {
        // Try to get profile from db
        var profile = await context.Profiles.FirstOrDefaultAsync(x => x.LoginToken == token);
        if (profile == null)
            return null;
        
        // Check if profile is expired
        if (profile.CreatedAt.AddSeconds(profile.ExpiresIn) < DateTime.UtcNow)
        {
            context.Profiles.Remove(profile);
            await context.SaveChangesAsync();
            return null;
        }
        
        return profile;
    }

    public async Task DeleteProfileAsync(Profile profile)
    {
        context.Profiles.Remove(profile);
        await context.SaveChangesAsync();
    }

    [GeneratedRegex("^[a-z0-9_]{3,16}$")]
    private static partial Regex NameValidator();
}