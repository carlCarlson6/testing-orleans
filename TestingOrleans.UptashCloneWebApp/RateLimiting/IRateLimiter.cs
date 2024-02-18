using Orleans.Runtime;
using static TestingOrleans.UptashCloneWebApp.Infrastructure.Orleans;

namespace TestingOrleans.UptashCloneWebApp.RateLimiting;

public interface IRateLimiter : IGrainWithStringKey
{
    ValueTask CheckReachedLimits();
}

// ReSharper disable once UnusedType.Global
public class RateLimiterGrain : Grain, IRateLimiter
{
    private readonly IPersistentState<Dictionary<long, int>> _data;

    public RateLimiterGrain(
        [PersistentState("rate.state", RateLimitingStoreName)]
        IPersistentState<Dictionary<long, int>> data)
    {
        _data = data;
    }

    public async ValueTask CheckReachedLimits()
    {
        _data.State.TryGetValue(DateTime.UtcNow.Date.Ticks, out var numberRequest);
        _data.State[DateTime.UtcNow.Date.Ticks] = ++numberRequest;
        await _data.WriteStateAsync();
        if (numberRequest > 50)
            throw new Exception("max published messaged reached"); ;
    }
}