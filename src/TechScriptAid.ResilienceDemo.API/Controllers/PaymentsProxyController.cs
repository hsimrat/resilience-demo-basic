using Microsoft.AspNetCore.Mvc;
using TechScriptAid.ResilienceDemo.API.Services;

namespace TechScriptAid.ResilienceDemo.API.Controllers;

[ApiController]
[Route("demo/proxy")]
public class PaymentsProxyController : ControllerBase
{
    [HttpGet("payments/{id}")]
    public Task<PaymentStatus> Get([FromServices] PaymentsClient client, string id, [FromQuery] string mode = "ok", CancellationToken ct = default)
        => client.GetStatusAsync(id, mode, ct);
}
