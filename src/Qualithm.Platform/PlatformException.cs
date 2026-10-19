using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Qualithm.Platform.Model;

namespace Qualithm.Platform;

/// <summary>
/// Every failure a platform call raises: an error response from the API, or a request that never
/// got one. Branch on <see cref="Code"/>, never on <see cref="Exception.Message"/>.
/// </summary>
public sealed class PlatformException : Exception
{
    private static readonly Dictionary<string, ApiErrorCode> _codes = typeof(ApiErrorCode)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.GetCustomAttribute<EnumMemberAttribute>()?.Value is not null)
        .ToDictionary(
            field => field.GetCustomAttribute<EnumMemberAttribute>()!.Value!,
            field => (ApiErrorCode)field.GetValue(null)!,
            StringComparer.Ordinal);

    /// <summary>Creates an error.</summary>
    /// <param name="message">What failed, lowercase, as the API sent it.</param>
    /// <param name="code">
    /// The envelope's stable code; <see cref="ApiErrorCode.UnknownDefaultOpenApi"/> for one this
    /// version doesn't know.
    /// </param>
    /// <param name="status">The HTTP status, or <c>0</c> when no response arrived.</param>
    /// <param name="innerException">The underlying error, when there is one.</param>
    public PlatformException(
        string message,
        ApiErrorCode code,
        int status,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Status = status;
    }

    /// <summary>
    /// The envelope's stable code. A code this version doesn't know yet, or a failure with no
    /// response, reads as <see cref="ApiErrorCode.UnknownDefaultOpenApi"/>; a missing token reads
    /// as <see cref="ApiErrorCode.Unauthorized"/>.
    /// </summary>
    public ApiErrorCode Code { get; }

    /// <summary>The HTTP status, or <c>0</c> when no response arrived.</summary>
    public int Status { get; }

    /// <summary>Builds the error for an error response, reading the <c>{ message, code }</c> envelope.</summary>
    /// <param name="status">The response's HTTP status.</param>
    /// <param name="body">The response body.</param>
    /// <returns>The error to raise.</returns>
    internal static PlatformException FromResponse(int status, string body)
    {
        string? message = null;
        var code = ApiErrorCode.UnknownDefaultOpenApi;
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                {
                    message = m.GetString();
                }

                if (root.TryGetProperty("code", out var c)
                    && c.ValueKind == JsonValueKind.String
                    && _codes.TryGetValue(c.GetString()!, out var known))
                {
                    code = known;
                }
            }
        }
        catch (JsonException)
        {
            // Not an envelope: the status alone names the failure.
        }

        return new PlatformException(
            string.IsNullOrEmpty(message) ? $"request failed with status {status}" : message,
            code,
            status);
    }

    /// <summary>Builds the error for a request that never got a response.</summary>
    /// <param name="cause">Why the request failed.</param>
    /// <returns>The error to raise.</returns>
    internal static PlatformException FromTransport(Exception cause) =>
        new($"request failed: {cause.Message.ToLowerInvariant()}", ApiErrorCode.UnknownDefaultOpenApi, 0, cause);
}
