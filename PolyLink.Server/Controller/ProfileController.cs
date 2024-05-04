using Microsoft.AspNetCore.Mvc;
using PolyLink.Common.Model;
using PolyLink.Server.Service;
using PolyLink.Server.Util;

namespace PolyLink.Server.Controller;

[ApiController]
[Route("[controller]")]
public class ProfileController(IProfileRepository profileRepository) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Profile>> CreateProfile([FromBody] ProfileInfo profileInfo)
    {
        try
        {
            var profile = await profileRepository.CreateProfileAsync(profileInfo);
            return Ok(new Profile
            {
                Id = profile.Id,
                Name = profile.Name,
                DisplayName = profile.DisplayName,
                LoginToken = profile.LoginToken
            });
        }
        catch
        {
            return BadRequest();
        }
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile([FromQuery] string? name = null)
    {
        if (name != null)
        {
            var profile = await profileRepository.GetProfileByNameAsync(name);
            if (profile == null)
                return NotFound();
            return Ok(new ProfileInfo
            {
                Name = profile.Name,
                DisplayName = profile.DisplayName
            });
        }
        
        // If name is not provided, we check for token
        var authHeader = Request.Headers.Authorization;
        if (authHeader.Count == 0)
            return BadRequest();
        
        var authorization = authHeader[0]!;
        var match = RegexHelper.TokenParser().Match(authorization);
        if (!match.Success)
            return BadRequest();
        
        var token = match.Groups[1].Value;
        var profileByToken = await profileRepository.GetProfileByTokenAsync(token);
        if (profileByToken == null)
            return Unauthorized();
        
        return Ok(new Profile
        {
            Id = profileByToken.Id,
            Name = profileByToken.Name,
            DisplayName = profileByToken.DisplayName,
            LoginToken = profileByToken.LoginToken
        });
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DeleteProfile()
    {
        // Get token from header
        var authHeader = Request.Headers.Authorization;
        if (authHeader.Count == 0)
            return Unauthorized();

        var authorization = authHeader[0]!;
        var match = RegexHelper.TokenParser().Match(authorization);
        if (!match.Success)
            return BadRequest();
        
        var token = match.Groups[1].Value;
        var profile = await profileRepository.GetProfileByTokenAsync(token);
        if (profile == null)
            return NotFound();

        if (profile.Session != null)
            return BadRequest();
            
        await profileRepository.DeleteProfileAsync(profile);
        return Ok();
    }
}