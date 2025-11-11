using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services
{
    /// <summary>
    /// JWT token generator implementation.
    /// Reads configuration from appsettings / environment variables.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly int _lifetimeMinutes;

        public TokenService(IConfiguration configuration)
        {
            _issuer = configuration["Jwt:Issuer"] ?? "ProyectoNet";
            _audience = configuration["Jwt:Audience"] ?? "ProyectoNetClients";
            _secret = configuration["Jwt:Secret"] ?? throw new ArgumentException("Jwt:Secret is not configured");

            if (!int.TryParse(configuration["Jwt:LifetimeMinutes"], out _lifetimeMinutes))
            {
                _lifetimeMinutes = 60; // default 60 minutes
            }
        }

        public int GetTokenLifetimeMinutes() => _lifetimeMinutes;

        public string GenerateToken(int userId, string email, int tenantId, IEnumerable<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("user_id", userId.ToString())
            };

            if (roles != null)
            {
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_lifetimeMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
