using System.Net;
using System.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;

namespace TechScriptAid.ResilienceDemo.API.Resilience;

public static class HttpClientResilienceExtensions
{
    public static IHttpClientBuilder ConfigurePaymentsClient(this IServiceCollection services, IConfiguration cfg)
    {
        return services.AddHttpClient("payments", c =>
        {
            c.BaseAddress = new Uri(cfg["Payments:BaseUrl"] ?? "https://localhost:5143");
            c.Timeout = TimeSpan.FromSeconds(5);
            c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TechScriptAid", "1.0"));
        })
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)))
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    public static IHttpClientBuilder AddResilience(this IHttpClientBuilder builder)
        => builder
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)))
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, attempt =>
                TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 50)));

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
