using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Elk.Identity.Extraction.Jwt;

namespace Karmak.Integrations.Volvo.Api.Infrastructure.Middleware;

internal class ImplicitElkContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var extractor = new ElkContextFromClaimsExtractor();
        ElkContext? context = extractor.GetFromClaims(httpContext.User.Claims);

        if (context == null)
        {
            await next.Invoke(httpContext);
        }
        else
        {
            await ImplicitElkContext.WithCurrentAsync(context, async () =>
            {
                await next.Invoke(httpContext);
            });
        }
    }
}
