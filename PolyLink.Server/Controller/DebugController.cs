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
    [HttpPost("broadcastStartGame")]
    public async Task BroadcastStartGame([FromBody] StartGamePacket packet)
    {
        await hubContext.Clients.All.SendAsync("StartGame", packet);
    }
    
    [HttpPost("broadcastSetPlayerHealth")]
    public async Task BroadcastHurtPlayer([FromBody] SetPlayerHealthPacket packet)
    {
        await hubContext.Clients.All.SendAsync("SetPlayerHealth", packet);
    }
    
    [HttpPost("broadcastKillPlayer")]
    public async Task BroadcastKillPlayer([FromBody] KillPlayerPacket packet)
    {
        await hubContext.Clients.All.SendAsync("KillPlayer", packet);
    }
}

#endif