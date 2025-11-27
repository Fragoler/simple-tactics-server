using GameServer.Middleware;
using GameServer.Model.IoC;

namespace GameServer;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        var config = builder.Configuration;
        Console.WriteLine($"ContentRootPath: {builder.Environment.ContentRootPath}");
        Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
        Console.WriteLine($"Config sources count: {config.Sources.Count()}");
        
        var appToken = config["GameServer:AppToken"];
        Console.WriteLine($"AppToken from config: {appToken ?? "NOT FOUND"}");
        
        
        
        
        
        
        // Services
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddHealthChecks();
        
        // Register Presenter
        builder.Services.AddSingleton<IoCManager>();

        var app = builder.Build();

        // Middleware
        app.UseRouting();
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/api"),
            appBuilder => appBuilder.UseMiddleware<AppTokenMiddleware>()
        );

        // Endpoints
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");

        app.MapControllers();
        // app.MapHub<GameHub>("/game", options =>
        // {
        //     options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
        //     options.ApplicationMaxBufferSize = 64 * 1024;
        //     options.TransportMaxBufferSize = 64 * 1024;
        // });

        app.Run();
    }
}