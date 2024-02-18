using Microsoft.AspNetCore.Mvc;
using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging.Topics;

public static class GetTopicInfo
{
    public static void MapGetTopicInfo(this IEndpointRouteBuilder endpoints) => endpoints.MapGet(
        pattern: "/api/topics/{topicName}",
        handler: async (HttpContext context, IClusterClient client, [FromRoute] string topicName) => await client
            .GetGrain<ITopic>(ITopic.BuildTopicId(context.GetApiKey(), topicName))
            .GetInfo());

    public  static void MapGetTopicsInfo(this IEndpointRouteBuilder endpoints) => endpoints.MapGet(
        pattern: "/api/topics",
        handler: async (HttpContext context, IClusterClient client) => await client
            .GetGrain<IApiTopics>(context.GetApiKey())
            .GetAllTopicsInfo());
}