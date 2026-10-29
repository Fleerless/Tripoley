using Tripoley.Api.Hubs;
using Tripoley.Api.Rooms;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomRegistry, InMemoryRoomRegistry>();
var app = builder.Build();

app.MapGet("/", () => "Hello World, I am healthy!");

app.MapHub<GameHub>("/hubs/gamehub");
app.Run();
