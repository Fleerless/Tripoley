namespace Tripoley.Api.Auth;

using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public sealed class SessionTokenIssuer(SecurityKey signingKey, IOptions<JwtOptions> options, TimeProvider time)
{
    private readonly SigningCredentials _credentials = new(signingKey, SecurityAlgorithms.HmacSha256);
    private readonly JsonWebTokenHandler _handler = new();

    public string Issue()
    {
        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())]),
            Expires = time.GetUtcNow().UtcDateTime.AddHours(options.Value.SessionHours),
            SigningCredentials = _credentials,
        });
    }
}
