using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Qualithm.Platform.Client;

namespace Qualithm.Platform;

/// <summary>
/// The Qualithm Platform API, authenticated with one API token. Each generated <c>…Api</c> class
/// is one resource group from the spec:
/// <code>
/// using var platform = new PlatformClient();
/// var page = await platform.Api&lt;DevicesApi&gt;().ListDevicesAsync();
/// </code>
/// Every error response and failed request throws a <see cref="PlatformException"/>.
/// </summary>
public sealed class PlatformClient : IDisposable
{
    /// <summary>The production API; <c>QUALITHM_API_URL</c> or <c>baseUrl</c> points the client elsewhere.</summary>
    public const string DefaultBaseUrl = "https://api.qualithm.com";

    private readonly HttpClient _http;
    private readonly ConcurrentDictionary<Type, IApiAccessor> _apis = new();

    /// <summary>Creates a client.</summary>
    /// <param name="token">An API token (<c>qmt_…</c>). Defaults to <c>QUALITHM_API_TOKEN</c>.</param>
    /// <param name="baseUrl">The API's base URL. Defaults to <c>QUALITHM_API_URL</c>, else the production API.</param>
    /// <param name="environment">
    /// Where the defaults are read from. Defaults to <see cref="Environment.GetEnvironmentVariable(string)"/>.
    /// </param>
    /// <param name="handler">
    /// The transport. Defaults to <see cref="SocketsHttpHandler"/>. The client disposes it.
    /// </param>
    /// <exception cref="PlatformException">
    /// With code <see cref="Model.ApiErrorCode.Unauthorized"/> when no token is given or set in
    /// <c>QUALITHM_API_TOKEN</c>.
    /// </exception>
    public PlatformClient(
        string? token = null,
        string? baseUrl = null,
        Func<string, string?>? environment = null,
        HttpMessageHandler? handler = null)
    {
        environment ??= Environment.GetEnvironmentVariable;
        string? Set(string name) => environment(name) is { Length: > 0 } value ? value : null;

        token ??= Set("QUALITHM_API_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new PlatformException(
                "api token missing: pass token or set QUALITHM_API_TOKEN",
                Model.ApiErrorCode.Unauthorized,
                0);
        }

        var url = (baseUrl ?? Set("QUALITHM_API_URL") ?? DefaultBaseUrl).TrimEnd('/');
        Configuration = new Configuration { BasePath = url };
        _http = new HttpClient(new PlatformHandler(token, handler ?? new SocketsHttpHandler()));
    }

    /// <summary>The generated client configuration every API class shares.</summary>
    public Configuration Configuration { get; }

    /// <summary>One generated API class, bound to this client and created once.</summary>
    /// <typeparam name="T">A generated API class, such as <c>DevicesApi</c>.</typeparam>
    /// <returns>The instance for this client.</returns>
    public T Api<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IApiAccessor =>
        (T)_apis.GetOrAdd(typeof(T), _ => Create<T>());

    /// <summary>Releases the HTTP client and the transport.</summary>
    public void Dispose() => _http.Dispose();

    private T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IApiAccessor
    {
        var constructor = typeof(T).GetConstructor([typeof(HttpClient), typeof(Configuration), typeof(HttpClientHandler)])
            ?? throw new ArgumentException($"{typeof(T).Name} is not a generated API class", nameof(T));
        return (T)constructor.Invoke([_http, Configuration, null]);
    }

    /// <summary>Signs every request and turns every failure into a <see cref="PlatformException"/>.</summary>
    private sealed class PlatformHandler(string token, HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        private readonly AuthenticationHeaderValue _authorization = new("Bearer", token);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization = _authorization;
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                throw PlatformException.FromTransport(e);
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            using (response)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw PlatformException.FromResponse((int)response.StatusCode, body);
            }
        }
    }
}
