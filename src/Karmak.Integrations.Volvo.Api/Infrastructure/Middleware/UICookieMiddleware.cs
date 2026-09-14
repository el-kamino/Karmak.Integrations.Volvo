namespace Karmak.Integrations.Volvo.Api.Infrastructure.Middleware;

internal class UICookieMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        if (httpContext.Request.Path.StartsWithSegments("/UI", StringComparison.InvariantCultureIgnoreCase))
        {
            string auth = httpContext.Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(auth))
            {
                if(httpContext.Request.Cookies.TryGetValue("session", out string token))
                {
                    if(!string.IsNullOrWhiteSpace(token))
                    {
                        httpContext.Request.Headers.Authorization = $"Bearer {token}";
                    }
                }
            }
        }

        await next(httpContext);
    }
}
