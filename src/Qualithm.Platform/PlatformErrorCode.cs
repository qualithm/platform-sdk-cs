namespace Qualithm.Platform;

/// <summary>
/// The stable code on a <see cref="PlatformException"/>. The platform can add codes without a new
/// client version, so a code this version doesn't know reads as <see cref="Unknown"/>.
/// </summary>
public enum PlatformErrorCode
{
    /// <summary>A code this version doesn't know, or a request that got no response.</summary>
    Unknown = 0,

    /// <summary><c>bad_request</c>: The request is malformed.</summary>
    BadRequest,

    /// <summary><c>unauthorized</c>: The token is missing, invalid or expired.</summary>
    Unauthorized,

    /// <summary><c>forbidden</c>: The token can't do this.</summary>
    Forbidden,

    /// <summary><c>not_found</c>: The resource doesn't exist, or isn't visible to the token.</summary>
    NotFound,

    /// <summary><c>conflict</c>: The request conflicts with the resource's current state.</summary>
    Conflict,

    /// <summary><c>precondition_failed</c>: The resource changed since it was read.</summary>
    PreconditionFailed,

    /// <summary><c>unsupported_media_type</c>: The request body's content type isn't accepted.</summary>
    UnsupportedMediaType,

    /// <summary><c>unprocessable_entity</c>: The request is well formed but its values aren't valid.</summary>
    UnprocessableEntity,

    /// <summary><c>failed_dependency</c>: A resource the request depends on failed.</summary>
    FailedDependency,

    /// <summary><c>rate_limited</c>: Too many requests; retry later.</summary>
    RateLimited,

    /// <summary><c>internal_error</c>: The platform failed.</summary>
    InternalError,

    /// <summary><c>backend_unavailable</c>: A platform backend is unavailable; retry later.</summary>
    BackendUnavailable,

    /// <summary><c>last_owner</c>: The change would leave the team without an owner.</summary>
    LastOwner,

    /// <summary><c>role_too_high</c>: The role is above the caller's own.</summary>
    RoleTooHigh,

    /// <summary><c>email_in_use</c>: The email address is already in use.</summary>
    EmailInUse,

    /// <summary><c>otp_invalid</c>: The one-time code is wrong or expired.</summary>
    OtpInvalid,

    /// <summary><c>owns_team</c>: The account still owns a team.</summary>
    OwnsTeam,

    /// <summary><c>account_not_found</c>: No account has that address.</summary>
    AccountNotFound,

    /// <summary><c>already_member</c>: The account is already a member of the team.</summary>
    AlreadyMember,

    /// <summary><c>zone_closed</c>: The zone isn't open.</summary>
    ZoneClosed,

    /// <summary><c>zone_mismatch</c>: The resource is in a different zone.</summary>
    ZoneMismatch,

    /// <summary><c>plan_limit</c>: The team's plan limit is reached.</summary>
    PlanLimit,

    /// <summary><c>registered_limit</c>: The registered limit is reached.</summary>
    RegisteredLimit,

    /// <summary><c>name_taken</c>: The name is already in use.</summary>
    NameTaken,

    /// <summary><c>paid_plan_required</c>: The feature needs a paid plan.</summary>
    PaidPlanRequired,

    /// <summary><c>scope_not_empty</c>: The scope still holds resources.</summary>
    ScopeNotEmpty,

    /// <summary><c>idempotency_mismatch</c>: The idempotency key was reused with a different request.</summary>
    IdempotencyMismatch,

    /// <summary><c>idempotency_in_flight</c>: A request with the same idempotency key is still running.</summary>
    IdempotencyInFlight,
}

/// <summary>Maps the envelope's wire code to <see cref="PlatformErrorCode"/>.</summary>
internal static class PlatformErrorCodes
{
    /// <summary>Decodes a wire code such as <c>not_found</c>.</summary>
    /// <param name="code">The envelope's <c>code</c>.</param>
    /// <returns>The matching code, or <see cref="PlatformErrorCode.Unknown"/> for one this version doesn't know.</returns>
    internal static PlatformErrorCode Parse(string? code) => code switch
    {
        "bad_request" => PlatformErrorCode.BadRequest,
        "unauthorized" => PlatformErrorCode.Unauthorized,
        "forbidden" => PlatformErrorCode.Forbidden,
        "not_found" => PlatformErrorCode.NotFound,
        "conflict" => PlatformErrorCode.Conflict,
        "precondition_failed" => PlatformErrorCode.PreconditionFailed,
        "unsupported_media_type" => PlatformErrorCode.UnsupportedMediaType,
        "unprocessable_entity" => PlatformErrorCode.UnprocessableEntity,
        "failed_dependency" => PlatformErrorCode.FailedDependency,
        "rate_limited" => PlatformErrorCode.RateLimited,
        "internal_error" => PlatformErrorCode.InternalError,
        "backend_unavailable" => PlatformErrorCode.BackendUnavailable,
        "last_owner" => PlatformErrorCode.LastOwner,
        "role_too_high" => PlatformErrorCode.RoleTooHigh,
        "email_in_use" => PlatformErrorCode.EmailInUse,
        "otp_invalid" => PlatformErrorCode.OtpInvalid,
        "owns_team" => PlatformErrorCode.OwnsTeam,
        "account_not_found" => PlatformErrorCode.AccountNotFound,
        "already_member" => PlatformErrorCode.AlreadyMember,
        "zone_closed" => PlatformErrorCode.ZoneClosed,
        "zone_mismatch" => PlatformErrorCode.ZoneMismatch,
        "plan_limit" => PlatformErrorCode.PlanLimit,
        "registered_limit" => PlatformErrorCode.RegisteredLimit,
        "name_taken" => PlatformErrorCode.NameTaken,
        "paid_plan_required" => PlatformErrorCode.PaidPlanRequired,
        "scope_not_empty" => PlatformErrorCode.ScopeNotEmpty,
        "idempotency_mismatch" => PlatformErrorCode.IdempotencyMismatch,
        "idempotency_in_flight" => PlatformErrorCode.IdempotencyInFlight,
        _ => PlatformErrorCode.Unknown,
    };
}
