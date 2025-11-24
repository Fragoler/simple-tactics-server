using Server.Model;
using Server.View;


public sealed class Program
{
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddScoped<GameService>();
        builder.Services.AddSingleton<GameSessionManager>();
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Middleware ---c
        app.UseRouting();

        // Endpoints ---
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");

        app.MapHub<GameHub>("/game", options =>
        {
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
            options.ApplicationMaxBufferSize = 64 * 1024;
            options.TransportMaxBufferSize = 64 * 1024;
        });

        app.MapPost("/api/game/create", async (GameSessionManager sessionManager) =>
        {
            var gameId = Guid.NewGuid();
            var gameState = sessionManager.CreateGame(Guid.NewGuid(), Guid.NewGuid());
            return Results.Ok(new { gameId = gameState?.GameId });
        });

        // Run ---
        app.Run();
    }
}
