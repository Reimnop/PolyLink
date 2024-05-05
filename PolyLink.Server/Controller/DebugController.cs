using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PolyLink.Common.Packet;
using PolyLink.Server.SignalR;

namespace PolyLink.Server.Controller;

#if DEBUG

[ApiController]
[Route("[controller]")]
public class DebugController(IHubContext<GameHub> hubContext) : ControllerBase
{
    [HttpPost("broadcastSwitchToArcade")]
    public async Task BroadcastSwitchToArcade([FromQuery] string levelId)
    {
        await hubContext.Clients.All.SendAsync("SwitchToArcade", new SwitchToArcadePacket
        {
            LevelId = levelId
        });
    }
}

#endif