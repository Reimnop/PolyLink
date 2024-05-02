using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PolyLink.Server.Service;

namespace PolyLink.Server.Controller;

[ApiController]
[Route("[controller]")]
public partial class GameController(ISessionService sessionService) : ControllerBase
{
    [HttpGet("ws")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status101SwitchingProtocols)]
    public async Task<IActionResult> WebSocket()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Check if user is authenticated
            var authHeader = Request.Headers.Authorization;
            if (authHeader.Count == 0)
                return Unauthorized();
            
            var authorization = authHeader[0]!;
            var match = TokenParser().Match(authorization);
            if (!match.Success)
                return BadRequest();
            
            var token = match.Groups[1].Value;
            var profileByToken = await sessionService.GetProfileByTokenAsync(token);
            if (profileByToken == null)
                return Unauthorized();
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            return new EmptyResult();
        }
        
        return BadRequest();
    }
    
    [GeneratedRegex("Bearer ([a-z0-9]{32})")]
    private static partial Regex TokenParser();
}