using System.Text.Json;
using Lynx.RedirectionService.Api;
using Lynx.RedirectionService.Application;
using Lynx.RedirectionService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddApplicationLayer()
    .AddInfrastructureLayer(config)
    .AddHttpContextAccessor()
    .AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseHangfireDashboard("/hangfire", new DashboardOptions
    // {
    //     Authorization = [],
    // });
}
else
{
    app.UseHsts();
}

app.UseCoreMiddlewares();
await app.Services.ConfigureMongoDbAsync();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                component = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapControllers();

app.Run();
