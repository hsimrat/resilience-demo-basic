using System.Net;
using System.Net.Http.Json;

namespace TechScriptAid.ResilienceDemo.API.Services;

public sealed class PaymentsClient
{
    private readonly HttpClient _http;
    public PaymentsClient(IHttpClientFactory factory) => _http = factory.CreateClient("payments");

    public async Task<PaymentStatus> GetStatusAsync(string id, string mode, CancellationToken ct)
    {
        using var res = await _http.GetAsync($"/sim/payments/{id}?mode={mode}", ct);
        if (res.StatusCode == HttpStatusCode.TooManyRequests) throw new Exception("Rate limited");
        res.EnsureSuccessStatusCode();
        var status = await res.Content.ReadFromJsonAsync<PaymentStatus>(cancellationToken: ct);
        return status ?? new PaymentStatus(id, "unknown", DateTimeOffset.UtcNow);
    }
}

public sealed record PaymentStatus(string Id, string State, DateTimeOffset UpdatedAt);
