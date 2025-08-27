using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

namespace TechScriptAid.ResilienceDemo.API.Controllers;

[ApiController]
[Route("sim/[controller]")]
public class PaymentsController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, int> Calls = new();

    [HttpGet("{id}")]
    public IActionResult Get(string id, [FromQuery] string mode = "ok")
    {
        switch (mode.ToLowerInvariant())
        {
            case "flaky":
                var n = Calls.AddOrUpdate(id, 1, (_, old) => old + 1);
                if (n <= 2) return StatusCode(503, new { id, state = "unavailable", updatedAt = DateTimeOffset.UtcNow });
                return Ok(new { id, state = "succeeded", updatedAt = DateTimeOffset.UtcNow });

            case "down":
                return StatusCode(503, new { id, state = "down", updatedAt = DateTimeOffset.UtcNow });

            case "ok":
            default:
                return Ok(new { id, state = "succeeded", updatedAt = DateTimeOffset.UtcNow });
        }
    }
}
