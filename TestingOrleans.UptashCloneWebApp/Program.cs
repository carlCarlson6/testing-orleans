using TestingOrleans.UptashCloneWebApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseOrleans(builder.Configuration)
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime();

var app = builder.Build();
app.MapAppEndpoints();
app.Run();