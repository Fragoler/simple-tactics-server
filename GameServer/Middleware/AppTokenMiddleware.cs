namespace GameServer.Middleware;

public class AppTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public AppTokenMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var expectedToken = _config["GameServer:AppToken"];
        var providedToken = context.Request.Query["app-token"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(providedToken) || providedToken != expectedToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Unauthorized: Invalid or missing application token" 
            });
            return;
        }

        await _next(context);
    }
}