using TestingOrleans.UptashCloneWebApp.Messaging.Infrastructure;
using TestingOrleans.UptashCloneWebApp.RateLimiting;
using TestingOrleans.UptashCloneWebApp.Tenants;
using TestingOrleans.UptashCloneWebApp.Tenants.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Infrastructure;

public static class WebAppExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.UseNeedsApiKeyMiddleware();
        app.UseRateLimitingMiddleware();
        app.MapTenantsRoutes();
        app.MapMessagingEndpoints();
    }

    private static void UseNeedsApiKeyMiddleware(this IApplicationBuilder app)
    {
        app.UseWhen(context => context.Request.Path.ToString().Contains("/api"), builder =>
            builder.Use(async (context, next) =>
            {
                await builder.ApplicationServices
                    .GetRequiredService<IClusterClient>()
                    .GetGrain<ITenants>(0)
                    .ValidateApiKey(context.GetApiKey());
                await next(context);
            }));
    }
    
    public static string GetApiKey(this HttpContext context)
    {
        var found = context.Request.Headers.TryGetValue("x-api-key", out var apiKey);
        if (!found || string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("no api key provided");
        return apiKey.ToString();
    }
}