using Orleans.Runtime;
using static System.Convert;
using static System.Text.Encoding;

namespace TestingOrleans.UptashCloneWebApp.Tenants;

public interface ITenant : IGrainWithStringKey // key is the tenant name
{
    ValueTask<string> GetApiKey();
}

public class TenantData
{
    public string ApiKey { get; set; } = string.Empty;
}

public class TenantGrain : Grain, ITenant
{
    private readonly IPersistentState<TenantData> _tenantData;

    public TenantGrain(
        [PersistentState(stateName: "tenant", storageName: "tenants.store")]
        IPersistentState<TenantData> tenantData)
    {
        _tenantData = tenantData;
    }

    public async ValueTask<string> GetApiKey() => string.IsNullOrWhiteSpace(_tenantData.State.ApiKey) 
        ? await GenerateApiKey() 
        : _tenantData.State.ApiKey;
    
    private async ValueTask<string> GenerateApiKey() // basic encryption used for simplicity - a better hashing mechanism should be used
    {
        var seed = $"{this.GetPrimaryKeyString()}-{Guid.NewGuid()}";
        var plainSeedBytes = UTF8.GetBytes(seed);
        _tenantData.State.ApiKey = ToBase64String(plainSeedBytes);
        await _tenantData.WriteStateAsync();
        return _tenantData.State.ApiKey;
    }
}