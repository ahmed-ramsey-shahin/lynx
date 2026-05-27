using System.Security.Cryptography;
using Hangfire;
using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Infrastructure.BackgroundJobs;
using Lynx.IdentityService.Infrastructure.Data;
using Lynx.IdentityService.Infrastructure.Data.Configuration;
using Lynx.IdentityService.Infrastructure.Exceptions;
using Lynx.IdentityService.Infrastructure.Services;
using Lynx.IdentityService.Infrastructure.Services.RabbitMq;
using Lynx.IdentityService.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        private static IServiceCollection AddRabbitMQ(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IRabbitMqConnectionManager>(_ => new RabbitMqConnectionManager(connectionString));
            services.AddSingleton<IRabbitMqChannelPool, RabbitMqChannelPool>();
            services.AddSingleton<IMessagePublishingService, MessagePublishingService>();
            return services;
        }

        private static IServiceCollection AddHangfireJobs(this IServiceCollection services)
        {
            services.AddHangfireServer();
            return services;
        }

        private static IServiceCollection AddLynxHealthChecks(
            this IServiceCollection services,
            string redisConnectionString
        )
        {
            services.AddHealthChecks()
                .AddMongoDb(
                    name: "MongoDB",
                    tags: ["ready", "database"]
                ).AddRabbitMQ(
                    name: "RabbitMQ",
                    tags: ["ready", "message-broker"]
                ).AddRedis(
                    redisConnectionString,
                    name: "Redis",
                    tags: ["ready", "cache"]
                );
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new InfrastructureConfigurationException("JwtSettings");
            var rsa = RSA.Create();
            rsa.ImportFromPem(jwtSettings.PrivateKey);
            var securityKey = new RsaSecurityKey(rsa);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5)
                };
            });
            return services;
        }

        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
        {
            var mongoDbConnectionString = config.GetConnectionString("MongoDB") ?? throw new InfrastructureConfigurationException("ConnectionStrings:MongoDB");
            var redisConnectionString = config.GetConnectionString("Redis") ?? throw new InfrastructureConfigurationException("ConnectionStrings:Redis");
            var rabbitMqConnectionString = config.GetConnectionString("RabbitMQ") ?? throw new InfrastructureConfigurationException("ConnectionStrings:RabbitMQ");
            var brevoApiKey = config["Email:ApiKey"] ?? throw new InfrastructureConfigurationException("Email:ApiKey");
            var passwordPepper = config["Security:PasswordPepper"] ?? throw new InfrastructureConfigurationException("Security:PasswordPepper");
            services.Configure<EmailSettings>(config.GetSection("EmailService"));
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            services.AddMongoDb(mongoDbConnectionString)
                .AddRedisCache(redisConnectionString)
                .AddBrevoEmails(brevoApiKey)
                .AddTransient<IOTPGeneratorService, OtpGeneratorService>()
                .AddTransient<IPasswordHashingService>(_ => new PasswordHashingService(passwordPepper))
                .AddTransient<ITokenProvider, TokenProvider>()
                .AddSingleton(TimeProvider.System)
                .AddRabbitMQ(rabbitMqConnectionString)
                .AddSingleton<IEmailBackgroundQueue, EmailBackgroundQueue>()
                .AddHostedService<EmailBackgroundWorker>()
                .AddHangfireJobs()
                .AddLynxHealthChecks(redisConnectionString)
                .AddJwtAuthentication(config);
            return services;
        }
    }
}
