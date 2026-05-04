using ShopDotNet.Services;

namespace ShopDotNet.Middleware;

public class AdminMiddleware
{
    private readonly RequestDelegate _next;

    public AdminMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService auth)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
        {
            var user = auth.GetCurrentUser(context.Session);
            if (user == null)
            {
                context.Response.Redirect("/login");
                return;
            }
            if (!user.IsAdmin)
            {
                context.Response.Redirect("/");
                return;
            }
        }

        await _next(context);
    }
}
