using Orleans.Runtime;
using static TestingOrleans.UptashCloneWebApp.Infrastructure.Orleans;
using static TestingOrleans.UptashCloneWebApp.Messaging.Topics.ITopic;

namespace TestingOrleans.UptashCloneWebApp.Messaging.Topics;

public interface IApiTopics : IGrainWithStringKey // key is api key
{
    ValueTask Add(string topicName, List<string> subscribers);
    ValueTask<ITopic> GetTopic(string topicName);
    ValueTask<List<TopicInfo>> GetAllTopicsInfo();
}

public class ApiTopicsData
{
    public HashSet<string> RegisteredTopics = new();
}

public class ApiTopicsGrain : Grain, IApiTopics
{
    private readonly IPersistentState<ApiTopicsData> _data;

    public ApiTopicsGrain(
        [PersistentState(stateName: "api.topics", storageName: TopicsStoreName)]
        IPersistentState<ApiTopicsData> data)
    {
        _data = data;
    }

    public async ValueTask Add(string topicName, List<string> subscribers)
    {
        await GrainFactory
            .GetGrain<ITopic>(BuildTopicId(this.GetPrimaryKeyString(), topicName))
            .UpdateSubscriber(subscribers);
        _data.State.RegisteredTopics.Add(topicName);
        await _data.WriteStateAsync();
    }

    public ValueTask<ITopic> GetTopic(string topicName)
    {
        var maybeTopicName = _data.State.RegisteredTopics.FirstOrDefault(x => x == topicName);
        return string.IsNullOrWhiteSpace(maybeTopicName)
            ? throw new Exception("unknown topic")
            : ValueTask.FromResult(GrainFactory.GetGrain<ITopic>(BuildTopicId(this.GetPrimaryKeyString(), topicName)));
    }

    public async ValueTask<List<TopicInfo>> GetAllTopicsInfo()
    {
        var topicsInfo = new List<TopicInfo>();
        foreach (var registeredTopic in _data.State.RegisteredTopics)
        {
            var topicInfo = await GrainFactory.GetGrain<ITopic>(registeredTopic).GetInfo();
            topicsInfo.Add(topicInfo);
        }
        return topicsInfo;
    }
}