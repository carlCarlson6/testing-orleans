using Orleans.Runtime;
using static TestingOrleans.UptashCloneWebApp.Infrastructure.Orleans;

namespace TestingOrleans.UptashCloneWebApp.Tenants;

public interface ITenants : IGrainWithIntegerKey // "singleton" grain - all ways use 0 key to retrieve
{
    ValueTask<string> CreateTenant(string tenantName);
    ValueTask<ITenant> GetTenant(string apiKey);
    ValueTask ValidateApiKey(string apiKey);
}

public class TenantsData
{
    public List<RegisteredTenantData> RegisteredTenants { get; set; } = new();
}

public record RegisteredTenantData(string Name, string ApiKey);

public class TenantsGrain : Grain, ITenants
{
    private readonly IPersistentState<TenantsData> _persistent;

    public TenantsGrain(
        [PersistentState(stateName: "tenants", storageName: TenantsStoreName)]
        IPersistentState<TenantsData> persistent)
    {
        _persistent = persistent;
    }

    public async ValueTask<string> CreateTenant(string tenantName) // move this to TenantGrain.OnActivateAsync ??
    {
        if (_persistent.State.RegisteredTenants.FirstOrDefault(x => x.Name == tenantName) is not null)
            throw new Exception($"tenant {tenantName} already exists");

        var tenantApiKey = await GrainFactory.GetGrain<ITenant>(tenantName).GetApiKey();
        
        _persistent.State.RegisteredTenants.Add(new RegisteredTenantData(tenantName, tenantApiKey));
        await _persistent.WriteStateAsync();
        
        return tenantApiKey;
    }

    public ValueTask<ITenant> GetTenant(string apiKey)
    {
        var maybeTenant = _persistent.State.RegisteredTenants.FirstOrDefault(x => x.ApiKey == apiKey);
        if (maybeTenant is null) 
            throw new Exception("invalid api key - no tenant associated was found");
        return ValueTask.FromResult(GrainFactory.GetGrain<ITenant>(maybeTenant.Name));
    }

    public async ValueTask ValidateApiKey(string apiKey) => await GetTenant(apiKey);
}