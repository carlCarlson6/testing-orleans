using Orleans.Runtime;

namespace TestingOrleans.UptashCloneWebApp.Messaging;

public interface IQueue : IGrainWithStringKey // key is the api key associated to the tenant
{
    ValueTask<Guid> Publish(string uri, string body, Dictionary<string, string> customHeaders, TimeSpan delay);
    ValueTask<List<MessageInfo>> GetAllMessages();
    ValueTask RequeueMessage(Guid messageId);
}

public class QueueData
{
    public List<Guid> Messages { get; set; } = new();
}

// ReSharper disable once UnusedType.Global
public class QueueGrain : Grain, IQueue
{
    private readonly IPersistentState<QueueData> _persistent;

    public QueueGrain(
        [PersistentState(stateName: "queue.state", storageName: UptashCloneWebApp.Infrastructure.Orleans.MessagesStoreName)]
        IPersistentState<QueueData> persistent)
    {
        _persistent = persistent;
    }

    public async ValueTask<Guid> Publish(string uri, string body, Dictionary<string, string> customHeaders, TimeSpan delay)
    {
        var message = GrainFactory.GetGrain<IMessage>(Guid.NewGuid());
        await message.Publish(uri, body, customHeaders, delay);
        _persistent.State.Messages.Add(message.GetPrimaryKey());
        await _persistent.WriteStateAsync();
        return message.GetPrimaryKey();
    }
    
    public async ValueTask<List<MessageInfo>> GetAllMessages()
    {
        var messagesData = new List<MessageInfo>();
        foreach (var messageId in _persistent.State.Messages)
            messagesData.Add(await GrainFactory.GetGrain<IMessage>(messageId).GetInfo());
        return messagesData;
    }

    public async ValueTask RequeueMessage(Guid messageId)
    {
        var maybeMessage = _persistent.State.Messages.FirstOrDefault(x => x == messageId);
        if (maybeMessage == Guid.Empty)
            throw new Exception("message not found");
        var message = GrainFactory.GetGrain<IMessage>(maybeMessage);
        await message.Requeue();
    }
}