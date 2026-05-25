using Lynx.IdentityService.Application;
using Lynx.IdentityService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddApplicationLayer(config)
    .AddInfrastructureLayer(config);

var app = builder.Build();
await app.Services.ConfigureMongoDbAsync();

app.Run();
