using Orleans.Runtime;
using static TestingOrleans.UptashCloneWebApp.Infrastructure.Orleans;

namespace TestingOrleans.UptashCloneWebApp.Messaging;

public interface IMessage : IGrainWithGuidKey // key is the messaged id
{
    const int MaxRetries = 3;
    ValueTask Publish(string uri, string body, Dictionary<string, string> customHeaders, TimeSpan? delay = null);
    ValueTask<MessageInfo> GetInfo();
    ValueTask Requeue();
}

public class MessageGrainData
{
    public MessagePayload Payload { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Created;
    public int RemainingRetries { get; set; } = IMessage.MaxRetries;
}

[GenerateSerializer]
public class MessagePayload
{
    public string Uri { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> CustomHeader { get; set; } = new();
}

public enum MessageStatus
{
    Created      = 1,
    Delivered    = 2,
    Failed       = 3,
    DeadLettered = 4
}

public class MessageGrain : Grain, IMessage, IRemindable
{
    private readonly IPersistentState<MessageGrainData> _persistent;
    private readonly ILogger<MessageGrain> _logger;
    
    public MessageGrain(
        [PersistentState(stateName: "message.state", storageName: MessagesStoreName)]
        IPersistentState<MessageGrainData> persistent,
        ILogger<MessageGrain> logger)
    {
        _persistent = persistent;
        _logger = logger;
    }

    public async ValueTask Publish(string uri, string body, Dictionary<string, string> customHeaders, TimeSpan? delay = null)
    {
        _persistent.State.Payload = new MessagePayload { Uri = uri, Body = body };
        await _persistent.WriteStateAsync();
        _logger.LogInformation("publishing message {MessageId}", this.GetPrimaryKey());
        await this.RegisterOrUpdateReminder(
            reminderName: "publish-trigger",
            dueTime: delay ?? TimeSpan.FromSeconds(0), 
            period: TimeSpan.FromMinutes(1));
    }
    
    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        try
        {
            _logger.LogInformation("trying to deliver message {MessageId}", this.GetPrimaryKey());
            var client = new HttpClient();
            foreach (var header in _persistent.State.Payload.CustomHeader)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            var response = await client.PostAsJsonAsync(_persistent.State.Payload.Uri, _persistent.State.Payload.Body);
            if (!response.IsSuccessStatusCode)
                await OnMessageDeliverFailed();
            else
                await OnMessageDeliverSuccessfully();
        }
        catch
        {
            await OnMessageDeliverFailed();
        }
    }

    private async ValueTask OnMessageDeliverSuccessfully()
    {
        _persistent.State.Status = MessageStatus.Delivered;
        await _persistent.WriteStateAsync();
        _logger.LogInformation("message {MessageId} delivered", this.GetPrimaryKey());
        await UnregisterReminder();
    }

    private async ValueTask OnMessageDeliverFailed()
    {
        _logger.LogError("failed to deliver {MessageId} remaining retries {RemainingRetries}", 
            this.GetPrimaryKey(), _persistent.State.RemainingRetries);
        _persistent.State.Status = MessageStatus.Failed;
        _persistent.State.RemainingRetries--;
        await _persistent.WriteStateAsync();
        if (_persistent.State.RemainingRetries <= 0)
            await OnMaxRetriesReached();
    }

    private async ValueTask OnMaxRetriesReached()
    {
        _logger.LogError("max retries reached for {MessageId}, dead lettering message", this.GetPrimaryKey());
        _persistent.State.Status = MessageStatus.DeadLettered;
        await _persistent.WriteStateAsync();
        await UnregisterReminder();
    }

    private async ValueTask UnregisterReminder()
    {
        _logger.LogInformation("unregistered reminder for message {MessageId}", this.GetPrimaryKey());
        var reminder = await this.GetReminder("publish-trigger");
        await this.UnregisterReminder(reminder);
    }
    
    public ValueTask<MessageInfo> GetInfo() => ValueTask.FromResult(new MessageInfo(
        Id:     this.GetPrimaryKey(),
        To:     _persistent.State.Payload.Uri,
        Status: Enum.GetName(_persistent.State.Status)!));

    public async ValueTask Requeue()
    {
        await _persistent.ReadStateAsync();
        if (_persistent.State.Status != MessageStatus.DeadLettered)
            throw new Exception("cannot requeue a non dead lettered message");
        _logger.LogInformation("re-queueing message {MessageId}", this.GetPrimaryKey());
        await this.RegisterOrUpdateReminder(
            reminderName: "publish-trigger",
            dueTime: TimeSpan.FromSeconds(5),
            period: TimeSpan.FromMinutes(1));
    }
}

[GenerateSerializer]
public record MessageInfo(Guid Id, string To, string Status);