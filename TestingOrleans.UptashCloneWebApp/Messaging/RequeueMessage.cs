using Microsoft.AspNetCore.Mvc;
using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging;

public static class RequeueMessage
{
    public static void MapRequeueMessage(this IEndpointRouteBuilder app) => app.MapPost(
        pattern: "/api/messaging/requeue-message/{messageId:Guid}", 
        handler:  async (HttpContext context, IClusterClient client, [FromRoute] Guid messageId) => await client
            .GetGrain<IQueue>(context.GetApiKey())
            .RequeueMessage(messageId));
}