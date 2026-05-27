using Hangfire;
using Lynx.IdentityService.Application;
using Lynx.IdentityService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddApplicationLayer(config)
    .AddInfrastructureLayer(config);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [],
    });
}

app.UseCoreMiddlewares(app.Configuration);
await app.Services.ConfigureMongoDbAsync();

app.Run();
