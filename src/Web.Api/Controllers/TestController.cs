using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
    }

    [HttpPost("echo")]
    public IActionResult Echo([FromBody] object data)
    {
        return Ok(new { message = "Echo successful", receivedData = data });
    }
}
