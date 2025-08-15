namespace ChatChallenge.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Query["access_token"];
        
        // If the request is for our SignalR hub...
        var path = context.Request.Path;
        if (!string.IsNullOrEmpty(token) && 
            (path.StartsWithSegments("/chathub") || path.StartsWithSegments("/api")))
        {
            // Read the token out of the query string
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        await _next(context);
    }
}
