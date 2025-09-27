using FastEndpoints;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Crypto;
using Domain.Interfaces;

namespace Hive.Idm.Api.Endpoints.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    IUserRepository _userRepository;
    public LoginEndpoint(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // Simple validation (replace with your actual user validation logic)
        if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        {
            await Send.ErrorsAsync(400, ct);
            return;
        }

        // Mock user validation - replace with actual user service
        if (!IsValidUser(req.Username, req.Password))
        {
            await Send.ErrorsAsync(401, ct);
            return;
        }

        var token = GenerateJwtToken(req.Username);
        
        await Send.OkAsync(new LoginResponse
        {
            AccessToken = token,
            ExpiresIn = 3600 // 1 hour
        }, ct);
    }

    private bool IsValidUser(string username, string password)
    {
        var exists = _userRepository.UsernameExistsAsync(username)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        // Mock validation - replace with actual user validation

        if (!exists)
        {
            return false;
        }
        
        var user = _userRepository.GetByUsernameAsync(username)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        
        
        return HashHelper.ComputeSha256Hash(password) == user.PasswordHash;
    }

    private string GenerateJwtToken(string username)
    {
        var config = Config;
        var secretKey = config["JwtSettings:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = config["JwtSettings:Issuer"] ?? "HiveIdm";
        var audience = config["JwtSettings:Audience"] ?? "HiveApi";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
