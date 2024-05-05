using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.SignalR;

namespace PolyLink.Server.Controller;

[ApiController]
[Route("[controller]")]
public class SessionController(IHubContext<GameHub> context) : ControllerBase
{
    [HttpPost("switchToArcade")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendSwitchToArcadePacket([FromQuery] string levelId)
    {
        await context.Clients.All.SendAsync("SwitchToArcade", new SwitchToArcadePacket
        {
            LevelId = levelId
        });
        return Ok();
    }
}