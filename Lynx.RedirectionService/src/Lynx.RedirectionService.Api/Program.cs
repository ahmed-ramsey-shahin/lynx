using Lynx.RedirectionService.Infrastructure;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddInfrastructureLayer(config)
    .AddHttpContextAccessor()
    .AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [],
    });
}
else
{
    app.UseHsts();
}

await app.Services.ConfigureMongoDbAsync();

app.MapControllers();

app.Run();
