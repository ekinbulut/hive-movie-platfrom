using FastEndpoints;
using System.Security.Claims;

namespace Hive.Idm.Api.Endpoints.Auth;

public class ValidateTokenResponse
{
    public bool IsValid { get; set; }
    public string? Username { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ValidateTokenEndpoint : EndpointWithoutRequest<ValidateTokenResponse>
{
    public override void Configure()
    {
        Get("/auth/validate");
        Roles("User", "Admin"); // Requires authentication
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var expClaim = User.FindFirst("exp")?.Value;
        
        DateTime? expiresAt = null;
        if (long.TryParse(expClaim, out var exp))
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
        }

        await Send.OkAsync(new ValidateTokenResponse
        {
            IsValid = true,
            Username = username,
            ExpiresAt = expiresAt
        }, ct);
    }
}