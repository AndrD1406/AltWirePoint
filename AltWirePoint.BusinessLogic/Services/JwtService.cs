using AltWirePoint.BusinessLogic.Models.Identity;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.Common;
using AltWirePoint.DataAccess;
using AltWirePoint.DataAccess.Identity;
using AltWirePoint.DataAccess.Models;
using AltWirePoint.DataAccess.Repository.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AltWirePoint.BusinessLogic.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration configuration;
    private readonly IEntityRepository<long, PermissionsForRole> permissionsRepository;
    private readonly string key;

    public JwtService(
        IConfiguration configration,
        IEntityRepository<long, PermissionsForRole> permissionsRepository)
    {
        this.configuration = configration;
        this.permissionsRepository = permissionsRepository;
        key = configuration["Jwt:Key"];
    }

    public async Task<AuthenticationResponse> CreateJwtToken(ApplicationUser user)
    {
        // Get the permissions for this specific role
        var rolePermissions = await permissionsRepository
            .GetByFilter(p => p.RoleName == user.Role)
            .FirstOrDefaultAsync();

        // Default to empty if role not found (or handle as error depending on preference)
        var packedPermissions = rolePermissions?.PackedPermissions ?? string.Empty;

        var accessExpiration = DateTime.UtcNow
            .AddMinutes(Convert.ToDouble(configuration["Jwt:AccessTokenExpirationMinutes"]));
        var refreshExpiration = DateTime.UtcNow
            .AddMinutes(Convert.ToDouble(configuration["Jwt:RefreshTokenExpirationMinutes"]));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(IdentityResourceClaimsTypes.Permissions, packedPermissions)
        };

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: accessExpiration,
            signingCredentials: signingCredentials
        );

        var encodedToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AuthenticationResponse
        {
            Token = encodedToken,
            Email = user.Email,
            UserName = user.UserName,
            Expiration = accessExpiration,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpirationDateTime = refreshExpiration
        };
    }

    private string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateActor = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            ValidateLifetime = false,
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc.Message);
            throw;
        }
    }
}
