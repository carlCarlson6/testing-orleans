using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.RateLimiting;

public static class ApplicationBuilderExtensions
{
    public static void UseRateLimitingMiddleware(this IApplicationBuilder app)
    {
        app.UseWhen(context => context.Request.Path.ToString().Contains("/api/messaging"), builder =>
            builder.Use(async (context, next) =>
            {
                await builder.ApplicationServices
                    .GetRequiredService<IClusterClient>()
                    .GetGrain<IRateLimiter>(context.GetApiKey())
                    .CheckReachedLimits();
                await next(context);
            }));
    }
}