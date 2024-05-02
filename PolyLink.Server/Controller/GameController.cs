using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PolyLink.Server.Service;

namespace PolyLink.Server.Controller;

[ApiController]
[Route("[controller]")]
public partial class GameController(ISessionService sessionService) : ControllerBase
{
    [HttpGet("connect")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status101SwitchingProtocols)]
    public async Task<IActionResult> ConnectWebSocket()
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
            
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await webSocket.SendAsync("The server has authenticated you~!!!"u8.ToArray(), WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
            return new EmptyResult();
        }
        
        return BadRequest();
    }
    
    [GeneratedRegex("Bearer ([a-z0-9]{32})")]
    private static partial Regex TokenParser();
}