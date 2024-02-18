using Microsoft.AspNetCore.Mvc;
using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging.Topics;

public static class CreateTopic
{
    public static void MapCreateTopic(this IEndpointRouteBuilder endpoints) => endpoints.MapPut(
        pattern: "/api/topics",
        handler: async (HttpContext context, IClusterClient client, [FromBody] CreateTopicRequest request) => 
            await client
                .GetGrain<IApiTopics>(context.GetApiKey())
                .Add(request.TopicName, request.Subscribers.Select(x => x.ToString()).ToList()));
}

public record CreateTopicRequest(string TopicName, List<Uri> Subscribers);