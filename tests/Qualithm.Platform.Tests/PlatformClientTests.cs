using System.Net;
using Qualithm.Platform.Api;
using Qualithm.Platform.Model;

namespace Qualithm.Platform.Tests;

public class PlatformClientTests
{
    private static readonly Guid _deviceId = Guid.Parse("0b9f8a1e-8c1d-4b6f-9a51-2f6c1e3d4a5b");

    private static Func<string, string?> Env(params (string Name, string Value)[] values)
    {
        var map = values.ToDictionary(v => v.Name, v => v.Value);
        return name => map.GetValueOrDefault(name);
    }

    [Fact]
    public async Task SendsTheTokenToTheProductionApiByDefault()
    {
        var stub = new StubHandler(body: """{"data":{"current":1,"items":[],"last":1}}""");
        using var platform = new PlatformClient(token: "qmt_test", environment: Env(), handler: stub);

        var page = await platform.Api<DevicesApi>().ListDevicesAsync(page: 2, limit: 10, TestContext.Current.CancellationToken);

        Assert.Empty(page.Data.Items);
        var request = Assert.Single(stub.Requests);
        Assert.Equal("https://api.qualithm.com/devices?page=2&limit=10", request.RequestUri?.ToString());
        Assert.Equal("Bearer qmt_test", request.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task ReadsTheTokenAndBaseUrlFromTheEnvironment()
    {
        var stub = new StubHandler(body: """{"status":"ok"}""");
        using var platform = new PlatformClient(
            environment: Env(("QUALITHM_API_TOKEN", "qmt_env"), ("QUALITHM_API_URL", "https://api.dev.qualithm.com/")),
            handler: stub);

        await platform.Api<HealthApi>().GetHealthzAsync(TestContext.Current.CancellationToken);

        var request = Assert.Single(stub.Requests);
        Assert.Equal("https://api.dev.qualithm.com/healthz", request.RequestUri?.ToString());
        Assert.Equal("Bearer qmt_env", request.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task PrefersArgumentsOverTheEnvironment()
    {
        var stub = new StubHandler(body: """{"status":"ok"}""");
        using var platform = new PlatformClient(
            token: "qmt_arg",
            baseUrl: "http://localhost:3000",
            environment: Env(("QUALITHM_API_TOKEN", "qmt_env"), ("QUALITHM_API_URL", "https://example.com")),
            handler: stub);

        await platform.Api<HealthApi>().GetHealthzAsync(TestContext.Current.CancellationToken);

        var request = Assert.Single(stub.Requests);
        Assert.Equal("http://localhost:3000/healthz", request.RequestUri?.ToString());
        Assert.Equal("Bearer qmt_arg", request.Headers.Authorization?.ToString());
    }

    [Fact]
    public void CreatesEachApiClassOnce()
    {
        using var platform = new PlatformClient(token: "qmt_test", environment: Env());

        Assert.Same(platform.Api<DevicesApi>(), platform.Api<DevicesApi>());
        Assert.Equal("https://api.qualithm.com", platform.Api<DevicesApi>().Configuration.BasePath);
    }

    [Fact]
    public void RejectsATypeThatIsNotAGeneratedApiClass()
    {
        using var platform = new PlatformClient(token: "qmt_test", environment: Env(), handler: new StubHandler());

        Assert.Throws<ArgumentException>(platform.Api<NotAnApi>);
    }

    [Fact]
    public void RefusesToStartWithoutAToken()
    {
        var error = Assert.Throws<PlatformException>(
            () => new PlatformClient(environment: Env(("QUALITHM_API_TOKEN", ""))));

        Assert.Equal(ApiErrorCode.Unauthorized, error.Code);
        Assert.Equal(0, error.Status);
        Assert.Equal("api token missing: pass token or set QUALITHM_API_TOKEN", error.Message);
    }

    private static async Task<PlatformException> FailureAsync(StubHandler stub)
    {
        using var platform = new PlatformClient(token: "qmt_test", environment: Env(), handler: stub);
        return await Assert.ThrowsAsync<PlatformException>(
            () => platform.Api<DevicesApi>().GetDeviceAsync(_deviceId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CarriesTheEnvelopesCodeMessageAndStatus()
    {
        var error = await FailureAsync(
            new StubHandler(HttpStatusCode.NotFound, """{"message":"device not found","code":"not_found"}"""));

        Assert.Equal(ApiErrorCode.NotFound, error.Code);
        Assert.Equal(404, error.Status);
        Assert.Equal("device not found", error.Message);
    }

    [Fact]
    public async Task DecodesAnUnknownCodeToTheFallback()
    {
        var error = await FailureAsync(
            new StubHandler(
                HttpStatusCode.UnprocessableEntity,
                """{"message":"device limit reached","code":"device_limit_reached"}"""));

        Assert.Equal(ApiErrorCode.UnknownDefaultOpenApi, error.Code);
        Assert.Equal(422, error.Status);
        Assert.Equal("device limit reached", error.Message);
    }

    [Theory]
    [InlineData("<html>bad gateway</html>")]
    [InlineData("""["not","an","envelope"]""")]
    [InlineData("""{"message":"","code":42}""")]
    public async Task NamesTheStatusWhenTheBodyIsNotAnEnvelope(string body)
    {
        var error = await FailureAsync(new StubHandler(HttpStatusCode.BadGateway, body));

        Assert.Equal(ApiErrorCode.UnknownDefaultOpenApi, error.Code);
        Assert.Equal(502, error.Status);
        Assert.Equal("request failed with status 502", error.Message);
    }

    [Fact]
    public async Task WrapsARequestThatNeverGotAResponse()
    {
        var cause = new HttpRequestException("Connection Refused");
        var error = await FailureAsync(new StubHandler(error: cause));

        Assert.Equal(ApiErrorCode.UnknownDefaultOpenApi, error.Code);
        Assert.Equal(0, error.Status);
        Assert.Equal("request failed: connection refused", error.Message);
        Assert.Same(cause, error.InnerException);
    }
}
