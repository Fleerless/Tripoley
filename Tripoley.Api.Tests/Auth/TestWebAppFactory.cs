using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Tripoley.Api.Auth;

namespace Tripoley.Api.Tests.Auth;

public sealed class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"{JwtOptions.SectionName}:SigningKey", TestTokens.SigningKey);
    }
}
