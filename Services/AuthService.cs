using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EverestRemit.Api.DTOs;
using EverestRemit.Api.Entities;
using EverestRemit.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
namespace EverestRemit.Api.Services;

public sealed class AuthService(UserManager<ApplicationUser> users, IConfiguration config) : IAuthService
{

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest r,
        CancellationToken ct)
    {
        var u = new ApplicationUser
        {
            UserName = r.Email.Trim(),
            Email = r.Email.Trim(),
            FullName = r.FullName.Trim()
        };

        var result = await users.CreateAsync(u, r.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await users.AddToRoleAsync(u, "Admin");

        if (!roleResult.Succeeded)
        {
            // Roll back user creation if role assignment fails.
            await users.DeleteAsync(u);

            throw new InvalidOperationException(
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        return await Token(u);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest r, CancellationToken ct) { var u = await users.FindByEmailAsync(r.Email.Trim()); if (u is null || !u.IsActive || !await users.CheckPasswordAsync(u, r.Password)) throw new UnauthorizedAccessException(); return await Token(u); }
    private async Task<AuthResponse> Token(ApplicationUser u) { var roles = (await users.GetRolesAsync(u)).ToArray(); var now = DateTime.UtcNow; var exp = now.AddMinutes(Math.Clamp(config.GetValue("Jwt:ExpiryMinutes", 60), 5, 1440)); var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, u.Id), new(ClaimTypes.Name, u.FullName), new(JwtRegisteredClaimNames.Email, u.Email ?? ""), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) }; claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x))); var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)); var jwt = new JwtSecurityToken(config["Jwt:Issuer"] ?? "EverestRemit", config["Jwt:Audience"] ?? "EverestRemitUsers", claims, now, exp, new SigningCredentials(key, SecurityAlgorithms.HmacSha256)); return new(new JwtSecurityTokenHandler().WriteToken(jwt), exp, u.Id, u.Email ?? "", u.FullName, roles); }
}
