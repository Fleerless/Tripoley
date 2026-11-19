namespace Tripoley.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; init; } = "";
    public int SessionHours { get; init; } = 12;
}
