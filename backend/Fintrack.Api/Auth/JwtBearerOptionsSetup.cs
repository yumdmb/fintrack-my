using System.Text;
using Fintrack.Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fintrack.Api.Auth;

public sealed class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name is not null && name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSigningKey(_jwtOptions.SigningKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);

    private static SymmetricSecurityKey CreateSigningKey(string signingKey)
    {
        var key = string.IsNullOrWhiteSpace(signingKey)
            ? "missing-signing-key-placeholder-for-startup-only"
            : signingKey;

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}
