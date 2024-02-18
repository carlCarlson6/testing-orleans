using Orleans.Runtime;
using static TestingOrleans.UptashCloneWebApp.Infrastructure.Orleans;

namespace TestingOrleans.UptashCloneWebApp.Messaging.Topics;

public interface ITopic : IGrainWithStringKey // key is the api key + topic name
{
    ValueTask UpdateSubscriber(List<string> subscribers);
    ValueTask Publish(string body);
    ValueTask<TopicInfo> GetInfo();

    static string BuildTopicId(string apiKey, string topicName) => $"{apiKey}/{topicName}";
}

public class TopicData
{
    public List<Guid> Messages { get; set; } = new();
    public List<string> Subscribers { get; set; } = new();
}

// ReSharper disable once UnusedType.Global
public class TopicGrain : Grain, ITopic
{
    private readonly IPersistentState<TopicData> _data;

    public TopicGrain(
        [PersistentState(stateName: "topic.state", storageName: TopicsStoreName)]
        IPersistentState<TopicData> data)
    {
        _data = data;
    }

    public async ValueTask UpdateSubscriber(List<string> subscribers)
    {
        _data.State.Subscribers = subscribers;
        await _data.WriteStateAsync();
    }

    public async ValueTask Publish(string body)
    {
        var topicQueue = GrainFactory.GetGrain<IQueue>(this.GetPrimaryKeyString());
        foreach (var subscriberUri in _data.State.Subscribers)
        {
            var messageId = await topicQueue.Publish(subscriberUri, body, new Dictionary<string, string>(), TimeSpan.Zero);
            _data.State.Messages.Add(messageId);
        }
        await _data.WriteStateAsync();
    }

    public async ValueTask<TopicInfo> GetInfo()
    {
        var messages = await GrainFactory.GetGrain<IQueue>(this.GetPrimaryKeyString()).GetAllMessages();
        return new TopicInfo(
            Name: TopicName,
            _data.State.Subscribers,
            Messages: messages);
    }
    
    private string ApiKey => this.GetPrimaryKeyString().Split("/").FirstOrDefault()
                             ?? throw new Exception("invalid topic key");
    private string TopicName => this.GetPrimaryKeyString().Split("/").LastOrDefault() 
                                ?? throw new Exception("invalid topic key");
}

[GenerateSerializer]
public record TopicInfo(string Name, List<string> Subscriber, List<MessageInfo> Messages);