using System.Net;
using System.Text;

namespace Qualithm.Platform.Tests;

/// <summary>Answers every request with one canned response, or fails it, and records what was sent.</summary>
internal sealed class StubHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;
    private readonly Exception? _error;

    public StubHandler(HttpStatusCode status = HttpStatusCode.OK, string body = "{}", Exception? error = null)
    {
        _status = status;
        _body = body;
        _error = error;
    }

    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (_error is not null)
        {
            throw _error;
        }

        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            RequestMessage = request,
        });
    }
}
