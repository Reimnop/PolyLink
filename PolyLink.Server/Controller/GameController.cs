using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using PolyLink.Server.Service;
using PolyLink.Server.Util;

namespace PolyLink.Server.Controller;

[ApiController]
[Route("[controller]")]
public class GameController(ISessionService sessionService, IWebSocketService webSocketService) : ControllerBase
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
            var match = RegexHelper.TokenParser().Match(authorization);
            if (!match.Success)
                return BadRequest();
            
            var token = match.Groups[1].Value;
            var profileByToken = await sessionService.GetProfileByTokenAsync(token);
            if (profileByToken == null)
                return Unauthorized();
            
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            try
            {
                var tcs = new TaskCompletionSource();
                await webSocketService.AddConnectionAsync(profileByToken, webSocket, tcs);
                await tcs.Task;
                return Empty;
            }
            catch (Exception e)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, e.Message, CancellationToken.None);
                webSocket.Dispose();
                return BadRequest();
            }
        }
        
        return BadRequest();
    }
}