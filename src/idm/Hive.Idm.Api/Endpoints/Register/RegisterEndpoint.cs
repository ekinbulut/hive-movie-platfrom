using Common.Crypto;
using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Hive.Idm.Api.Endpoints.Register;

public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterEndpoint(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.Email))
        {
            await Send.ErrorsAsync(400, ct);
            return;
        }

        // Check if username already exists
        var usernameExists = await _userRepository.UsernameExistsAsync(req.Username);
        if (usernameExists)
        {
            await Send.ErrorsAsync(409, ct); // Conflict
            return;
        }

        // Create new user
        var passwordHash = HashHelper.ComputeSha256Hash(req.Password);
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _userRepository.CreateAsync(user);
            
            await Send.OkAsync(new RegisterResponse
            {
                Success = true,
                Message = "User registered successfully",
                UserId = user.Id.ToString()
            }, ct);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500, ct);
        }
    }
}