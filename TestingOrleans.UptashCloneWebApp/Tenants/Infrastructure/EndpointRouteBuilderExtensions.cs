namespace TestingOrleans.UptashCloneWebApp.Tenants.Infrastructure;

public static class EndpointRouteBuilderExtensions
{
    public static void MapTenantsRoutes(this IEndpointRouteBuilder endpoints) => endpoints
        .MapCreateTenant();
}