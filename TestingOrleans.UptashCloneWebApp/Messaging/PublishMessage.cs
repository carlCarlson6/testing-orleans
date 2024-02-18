using Microsoft.AspNetCore.Mvc;
using TestingOrleans.UptashCloneWebApp.Infrastructure;

namespace TestingOrleans.UptashCloneWebApp.Messaging;

public static class PublishMessage
{
    public static void MapPublishMessage(this IEndpointRouteBuilder app) => app.MapPost(
        pattern: "/api/messaging/publish-message",
        handler: async (
            HttpContext context, 
            IClusterClient client,
            ILoggerFactory loggerFactory,
            [FromBody] PublishMessageRequest request) =>
    {
        var messageId = Guid.NewGuid();
        loggerFactory
            .CreateLogger(context.Request.Path.ToString())
            .LogInformation("new request to deliver message {MessageId}", messageId);
        await client
            .GetGrain<IQueue>(context.GetApiKey())
            .Publish(request.Uri.ToString(), request.Body, request.CustomHeaders, TimeSpan.FromMilliseconds(request.DelayInMilliSeconds));
    });
}

public record PublishMessageRequest(
    Uri Uri, 
    string Body, 
    Dictionary<string, string> CustomHeaders, 
    int DelayInMilliSeconds);