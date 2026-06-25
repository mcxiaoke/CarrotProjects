using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RemotePCControl.Models;

namespace RemotePCControl.Middleware;

public class TokenAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly ILogger<TokenAuthMiddleware> _logger;

    public TokenAuthMiddleware(RequestDelegate next, IConfiguration config, ILogger<TokenAuthMiddleware> logger)
    {
        _next = next;
        _config = config;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var security = _config.GetSection("Security").Get<SecuritySettings>() ?? new SecuritySettings();
        var request = context.Request;
        var path = request.Path.ToString().ToLowerInvariant();

        var isStaticOrRoot = path == "/" || path.StartsWith("/css/") || path.StartsWith("/js/") || path.StartsWith("/lib/") || path == "/favicon.ico";

        if (!security.RequireToken || isStaticOrRoot)
        {
            await _next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "n/a";

        if (security.EnableIpWhitelist)
        {
            if (!security.AllowedIpPrefixes.Any(prefix => ip.StartsWith(prefix, StringComparison.Ordinal)))
            {
                _logger.LogWarning("IP {ip} blocked by whitelist", ip);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("{\"error\":\"IP not in whitelist\"}");
                return;
            }
        }

        var token = request.Query["token"].FirstOrDefault()
                    ?? request.Headers["X-Token"].FirstOrDefault()
                    ?? request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", string.Empty, StringComparison.Ordinal);

        if (string.Equals(token, security.AccessToken, StringComparison.Ordinal))
        {
            await _next(context);
            return;
        }

        _logger.LogWarning("Invalid token from {ip}", ip);
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Unauthorized. Provide ?token= or X-Token header." });
    }
}

public static class TokenAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenAuth(this IApplicationBuilder builder)
        => builder.UseMiddleware<TokenAuthMiddleware>();
}
