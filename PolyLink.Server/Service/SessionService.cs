using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PolyLink.Server.Controller;
using PolyLink.Server.Model;
using PolyLink.Server.Util;

namespace PolyLink.Server.Service;

public class SessionService(PolyLinkContext context) : ISessionService
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
            Id = Random.Shared.Next(),
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
        // Try to get profile from db
        return await context.Profiles.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<Profile?> GetProfileByTokenAsync(string token)
    {
        // Try to get profile from db
        return await context.Profiles.FirstOrDefaultAsync(x => x.LoginToken == token);
    }

    public async Task DeleteProfileAsync(Profile profile)
    {
        context.Profiles.Remove(profile);
        await context.SaveChangesAsync();
    }
}