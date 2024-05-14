using Microsoft.AspNetCore.Mvc;
using PolyLink.Server.Service;

namespace PolyLink.Server.Controller;

#if DEBUG

[ApiController]
[Route("[controller]")]
public class DebugController(ISessionRepository sessionRepository, IGameService gameService) : ControllerBase
{
    [HttpPost("StartGame")]
    public async Task StartGame([FromQuery] ulong levelId)
    {
        await gameService.StartGameAsync(sessionRepository.GetSessionsAsync().ToEnumerable(), levelId);
    }
    
    [HttpPost("StopGame")]
    public async Task StopGame()
    {
        await gameService.StopGameAsync();
    }
}

#endif