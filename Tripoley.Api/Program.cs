using Tripoley.Api.Hubs;
using Tripoley.Api.Rooms;

const string ClientCorsPolicy = "client";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
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

app.MapGet("/", () => "Hello World, I am healthy!");
app.MapHub<GameHub>("/hubs/gamehub");

app.Run();
