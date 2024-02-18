using TestingOrleans.UptashCloneWebApp.Messaging.Topics.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging.Infrastructure;

public static class EndpointRouteBuilderExtensions
{
    public static void MapMessagingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetAllMessages();
        endpoints.MapPublishMessage();
        endpoints.MapRequeueMessage();
        
        endpoints.MapTopicsEndpoint();
    }
}