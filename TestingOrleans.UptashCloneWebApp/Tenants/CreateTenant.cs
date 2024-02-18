using Microsoft.AspNetCore.Mvc;

namespace TestingOrleans.UptashCloneWebApp.Tenants;

public static class CreateTenant
{
    public static void MapCreateTenant(this IEndpointRouteBuilder endpoints) => endpoints.MapPost(
        pattern: "tenants",
        handler: async (IClusterClient client, [FromBody] CreateTenantRequest request) =>
        {
            var apiKey = await client.GetGrain<ITenants>(0).CreateTenant(request.TenantName);
            return new CreateTenantResponse(apiKey);
        });
}

public record CreateTenantRequest(string TenantName);
public record CreateTenantResponse(string ApiKey);