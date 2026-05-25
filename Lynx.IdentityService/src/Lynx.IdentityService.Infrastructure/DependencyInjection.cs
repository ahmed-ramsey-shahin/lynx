using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Infrastructure.Data;
using Lynx.IdentityService.Infrastructure.Data.Configuration;
using Lynx.IdentityService.Infrastructure.Exceptions;
using Lynx.IdentityService.Infrastructure.Services;
using Lynx.IdentityService.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Lynx.IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static async Task ConfigureMongoDbAsync(this IServiceProvider services)
        {
            MongoDbConfiguration.ConfigureMappings();
            var client = services.GetRequiredService<IMongoClient>();
            await MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(client);
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }

        private static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 10_000;
            options.SyncTimeout = 10_000;
            options.AsyncTimeout = 10_000;
            options.ConnectRetry = 5;
            options.AbortOnConnectFail = false;
            options.ReconnectRetryPolicy = new ExponentialRetry(500, 2000);
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }

        private static IServiceCollection AddBrevoEmails(this IServiceCollection services, string apiKey)
        {
            services.AddHttpClient("BrevoEmailClient", client =>
            {
                client.BaseAddress = new Uri("https://api.brevo.com/v3/smtp/email");
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("api-key", apiKey);
            });
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }

        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
        {
            var mongoDbConnectionString = config.GetConnectionString("MongoDbConnectionString") ?? throw new InfrastructureConfigurationException("MongoDbConnectionString");
            var redisConnectionString = config.GetConnectionString("RedisConnectionString") ?? throw new InfrastructureConfigurationException("RedisConnectionString");
            var brevoApiKey = config["Email:ApiKey"] ?? throw new InfrastructureConfigurationException("Email:ApiKey");
            var passwordPepper = config["Security::PasswordPepper"] ?? throw new InfrastructureConfigurationException("PasswordPepper");
            services.Configure<EmailSettings>(config.GetSection("EmailService"));
            services.AddMongoDb(mongoDbConnectionString)
                .AddRedisCache(redisConnectionString)
                .AddBrevoEmails(brevoApiKey)
                .AddTransient<IOTPGeneratorService, OtpGeneratorService>()
                .AddTransient<IPasswordHashingService>(_ => new PasswordHashingService(passwordPepper))
                .AddTransient<ITokenProvider, TokenProvider>();
            return services;
        }
    }
}
