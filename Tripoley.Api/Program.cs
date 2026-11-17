
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tripoley.Api.Auth;
using Tripoley.Api.Hubs;
using Tripoley.Api.Rooms;

const string ClientCorsPolicy = "client";
const string GameHubPath = "/hubs/gamehub";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IRoomRegistry, InMemoryRoomRegistry>();
builder.Services.AddHostedService<IdleRoomSweeper>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.AddOptions<JwtOptions>()
    .Bind(jwtSection)
    .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
        "Jwt:SigningKey must be at least 256 bits (32 bytes).")
    .ValidateOnStart();

var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddSingleton(sp =>
    new SessionTokenIssuer(signingKey, sp.GetRequiredService<IOptions<JwtOptions>>(), sp.GetRequiredService<TimeProvider>()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                string? accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken)
                    && context.HttpContext.Request.Path.StartsWithSegments(GameHubPath))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

const string SessionMintPolicy = "session-mint";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(SessionMintPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
    options.AddPolicy(ClientCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

var app = builder.Build();

app.UseCors(ClientCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

var appVersion = app.Configuration["APP_VERSION"] ?? "dev";

app.MapGet("/", () => "Tripoley API is healthy.");
app.MapGet("/version", () => appVersion);

app.MapPost("/auth/session", (SessionTokenIssuer issuer) =>
    Results.Ok(new { token = issuer.Issue() }))
    .RequireRateLimiting(SessionMintPolicy);

app.MapHub<GameHub>(GameHubPath);

app.Run();

public partial class Program;
