namespace TestingOrleans.UptashCloneWebApp.Messaging.Topics.Infrastructure;

public static class EndpointsRouteBuilderExtensions
{
    public static void MapTopicsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCreateTopic();
        endpoints.MapGetTopicInfo();
        endpoints.MapGetTopicsInfo();
        endpoints.MapPublishToTopic();
    }
}