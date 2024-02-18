using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging;

public static class GetAllMessages
{
    public static void MapGetAllMessages(this IEndpointRouteBuilder app) => app.MapGet(
        pattern: "/api/messages",
        handler: async (HttpContext context, IClusterClient client) => await client
            .GetGrain<IQueue>(context.GetApiKey())
            .GetAllMessages());
}