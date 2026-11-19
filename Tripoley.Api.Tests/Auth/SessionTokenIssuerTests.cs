using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tripoley.Api.Auth;

namespace Tripoley.Api.Tests.Auth;

public class SessionTokenIssuerTests
{
    private static readonly SymmetricSecurityKey Key =
        new(Encoding.UTF8.GetBytes(TestTokens.SigningKey));

    private static SessionTokenIssuer CreateIssuer(TimeProvider time, int sessionHours = 12) =>
        new(Key, Options.Create(new JwtOptions { SigningKey = TestTokens.SigningKey, SessionHours = sessionHours }), time);

    [Fact]
    public async Task Issue_ProducesATokenValidAgainstTheConfiguredKeyCarryingAGuidIdentity()
    {
        var issuer = CreateIssuer(new FakeTimeProvider());

        string token = issuer.Issue();

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = Key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
        });

        Assert.True(result.IsValid);
        Assert.Contains(result.ClaimsIdentity.Claims, claim => Guid.TryParse(claim.Value, out _));
    }

    [Fact]
    public void Issue_SetsExpiryToSessionHoursFromCurrentTime()
    {
        var time = new FakeTimeProvider();
        var issuer = CreateIssuer(time, sessionHours: 3);

        string token = issuer.Issue();

        var expiry = new JsonWebTokenHandler().ReadJsonWebToken(token).ValidTo;
        Assert.Equal(time.GetUtcNow().UtcDateTime.AddHours(3), expiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Issue_MintsAFreshIdentityEachCall()
    {
        var issuer = CreateIssuer(new FakeTimeProvider());

        Assert.NotEqual(issuer.Issue(), issuer.Issue());
    }
}
