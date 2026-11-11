using System.Text.Json.Serialization;
using Tripoley.Api.Hubs;
using Tripoley.Api.Rooms;

const string ClientCorsPolicy = "client";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IRoomRegistry, InMemoryRoomRegistry>();

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

var appVersion = app.Configuration["APP_VERSION"] ?? "dev";

app.MapGet("/", () => "Tripoley API is healthy.");
app.MapGet("/version", () => appVersion);
app.MapHub<GameHub>("/hubs/gamehub");

app.Run();

public partial class Program;
