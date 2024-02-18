using Microsoft.AspNetCore.Mvc;
using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging.Topics;

public static class PublishToTopic
{
    public static void MapPublishToTopic(this IEndpointRouteBuilder app) => app.MapPost(
        pattern: "/api/messaging/publish-topic/{topicName}",
        handler: async (HttpContext context, IClusterClient client, [FromRoute] string topicName) =>
        {
            var topic = await client.GetGrain<IApiTopics>(context.GetApiKey()).GetTopic(topicName);
            await topic.Publish(await context.ReadRequestBodyAsync());
        });

    private static async ValueTask<string> ReadRequestBodyAsync(this HttpContext context) => 
        await new StreamReader(context.Request.Body).ReadToEndAsync();
}