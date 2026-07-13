using NisanDavetiye.BLL.Security;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Middleware;

public class SecurityMiddleware
{
    public const string DavetKeyHeader = "X-Davet-Key";
    public const string AdminKeyHeader = "X-Admin-Key";
    public const string PanelUidHeader = "X-Panel-Uid";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public SecurityMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IInviteAuthService inviteAuth,
        IPanelAuthService panelAuth)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        if (RequiresPanelAuth(path, method))
        {
            var panelUid = context.Request.Headers[PanelUidHeader].FirstOrDefault();
            if (!await panelAuth.IsValidPanelUidAsync(panelUid))
            {
                await WriteForbiddenAsync(context, "Geçersiz yönetim bağlantısı.");
                return;
            }

            if (!IsValidAdminKey(context))
            {
                await WriteUnauthorizedAsync(context);
                return;
            }
        }
        else if (RequiresDavetKey(path, method))
        {
            var provided = context.Request.Headers[DavetKeyHeader].FirstOrDefault();
            if (!await inviteAuth.IsValidDavetKeyAsync(provided))
            {
                await WriteForbiddenAsync(context, "Geçersiz davetiye bağlantısı.");
                return;
            }
        }

        await _next(context);
    }

    private static bool RequiresDavetKey(string path, string method) =>
        (path.Equals("/api/rsvp", StringComparison.OrdinalIgnoreCase) && method == "POST")
        || (path.Equals("/api/galeri/upload", StringComparison.OrdinalIgnoreCase) && method == "POST");

    private static bool RequiresPanelAuth(string path, string method)
    {
        if (!path.StartsWith("/api/panel/", StringComparison.OrdinalIgnoreCase))
            return false;

        if (path.StartsWith("/api/panel/access/", StringComparison.OrdinalIgnoreCase) && method == "GET")
            return false;

        return true;
    }

    private bool IsValidAdminKey(HttpContext context)
    {
        var expected = _config["Admin:ApiKey"];
        var provided = context.Request.Headers[AdminKeyHeader].FirstOrDefault();
        return SecureCompare.Equals(expected, provided);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = "Yetkisiz erişim." });
    }

    private static async Task WriteForbiddenAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}

public static class SecurityMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurity(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityMiddleware>();
}
