namespace TestingOrleans.UptashCloneWebApp.Infrastructure;

public static class Orleans
{
    public const string MessagesStoreName = "messages.store";
    public const string TopicsStoreName = "topics.store";
    public const string TenantsStoreName = "tenants.store";
    public const string RateLimitingStoreName = "rate.limiting";

    // client is also added
    public static IHostBuilder UseOrleans(this IHostBuilder builder, IConfiguration config) => builder.UseOrleans(silo => silo
        .AddGrainStorage(new List<string>{ MessagesStoreName, TenantsStoreName, TopicsStoreName, RateLimitingStoreName }, config)
        .UseAzureTableReminderService(config.ReadStorageAccountConnectionString())
        .UseLocalhostClustering()
        .ConfigureLogging(logging => logging.AddConsole()));

    private static ISiloBuilder AddGrainStorage(this ISiloBuilder silo, List<string> storagesNames, IConfiguration config)
    {
        foreach (var storageName in storagesNames)
            silo.AddAzureTableGrainStorage(
                name: storageName,
                configureOptions: o => o.ConfigureTableServiceClient(config.ReadStorageAccountConnectionString()));
        return silo;
    } 
    
    private static string ReadStorageAccountConnectionString(this IConfiguration config) =>
        $"DefaultEndpointsProtocol=https;AccountName={config.GetConnectionString("az-storage-name")};AccountKey={config.GetConnectionString("az-storage-key")}";
}