using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> users,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _users = users;
        _config = config;
        _logger = logger;
    }

    public async Task<(bool ok, string error)> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName
        };
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User registered: {Email}", dto.Email);
        return (true, string.Empty);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null) return null;
        if (!await _users.CheckPasswordAsync(user, dto.Password)) return null;

        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"] ?? "DEV_ONLY_KEY_REPLACE_IN_PRODUCTION_32CHARS!";
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(8);
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"] ?? "TaskFlow",
            audience: jwt["Audience"] ?? "TaskFlowUsers",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            },
            expires: expires,
            signingCredentials: creds);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return new AuthResponseDto(encoded, expires, user.DisplayName);
    }
}
