﻿using Microsoft.AspNetCore.Mvc;
using PolyLink.Server.Service;

namespace PolyLink.Server.Controller;

[ApiController]
[Route("[controller]")]
public class SessionController(ISessionRepository sessionRepository) : ControllerBase
{
    [HttpGet("Count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessionCount()
    {
        return Ok(await sessionRepository.GetSessionCountAsync());
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSessions()
    {
        return Ok(sessionRepository.GetSessionsAsync()
            .Select(x => new
            {
                Id = x.ConnectionId,
                x.ClientId,
                x.DisplayName
            }));
    }
}