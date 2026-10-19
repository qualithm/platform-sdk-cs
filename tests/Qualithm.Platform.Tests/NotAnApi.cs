using Qualithm.Platform.Client;

namespace Qualithm.Platform.Tests;

/// <summary>An API accessor without the generated constructor.</summary>
public sealed class NotAnApi : IApiAccessor
{
    public IReadableConfiguration Configuration { get; set; } = new Configuration();

    public ExceptionFactory ExceptionFactory { get; set; } = (_, _) => new InvalidOperationException();

    public string GetBasePath() => Configuration.BasePath;
}
