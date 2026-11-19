namespace Tripoley.Api.Rooms;

using Microsoft.Extensions.Hosting;

public sealed class IdleRoomSweeper(IRoomRegistry registry, TimeProvider time, ILogger<IdleRoomSweeper> logger) : BackgroundService
{
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval, time);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                int removed = registry.RemoveIdle(time.GetUtcNow() - IdleTimeout);
                if (removed > 0)
                    logger.LogInformation("Idle sweep removed {RoomCount} abandoned room(s).", removed);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
