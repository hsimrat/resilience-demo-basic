using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechScriptAid.ResilienceDemo.API.Resilience;
using Xunit;

public sealed class FlakyHandler : DelegatingHandler
{
    private int _calls = 0;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _calls++;
        if (_calls <= 2)
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
    }
}

public class HttpClientResilienceTests
{
    [Fact]
    public async Task NamedClient_retries_then_succeeds()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("test").AddResilience().AddHttpMessageHandler(() => new FlakyHandler());
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("test");
        var res = await client.GetAsync("http://any", CancellationToken.None);
        Assert.True(res.IsSuccessStatusCode);
    }
}
