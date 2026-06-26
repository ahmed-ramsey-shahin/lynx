using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Infrastructure.Exceptions;
using Lynx.RedirectionService.Infrastructure.BackgroundServices;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lynx.RedirectionService.Infrastructure.Data.Configuration;
using MongoDB.Driver;
using Lynx.RedirectionService.Infrastructure.Data;
using Lynx.RedirectionService.Application.Common.Repositories;
using StackExchange.Redis;
using Lynx.RedirectionService.Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;

namespace Lynx.RedirectionService.Infrastructure
{
    public static class DependencyInjection
    {
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
        public static async Task ConfigureMongoDbAsync(this IServiceProvider services)
        {
            MongoDbConfiguration.ConfigureMappings();
            var client = services.GetRequiredService<IMongoClient>();
            await MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(client);
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddScoped<IUrlRepository, UrlRepository>();
            return services;
        }
        private static IServiceCollection AddRabbitMQ(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IRabbitMqConnectionManager>(_ => new RabbitMqConnectionManager(connectionString));
            services.AddSingleton<IRabbitMqChannelPool, RabbitMqChannelPool>();
            services.AddSingleton<IMessagePublishingService, MessagePublishingService>();
            services.AddSingleton<IMessageChannel, MessageChannel>();
            services.AddHostedService<RabbitMqPublisherBackgroundService>();
            services.AddKeyedSingleton<IMessagePublishingService, BackgroundMessagePublishingService>("background");
            services.AddKeyedSingleton<IMessagePublishingService, MessagePublishingService>("instant");
            return services;
        }

        private static IServiceCollection AddDynamicJwkAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var baseIdentityAddress = config["Authentication:IdentityServiceUrl"] ?? throw new InfrastructureConfigurationException("Authentication:IdentityServiceUrl");
            var jwksUrl = $"{baseIdentityAddress}/api/auth/jwk";
            var configManager = new ConfigurationManager<JsonWebKeySet>(
                jwksUrl,
                new JwksRetriever(),
                new HttpClient()
            );
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config["Authentication:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = config["Authentication:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                        {
                            var jwks = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
                            var keys = jwks.GetSigningKeys();

                            if(!string.IsNullOrEmpty(kid) && !keys.Any(k => k.KeyId == kid))
                            {
                                configManager.RequestRefresh();
                                jwks = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
                                keys = jwks.GetSigningKeys();
                            }

                            return keys;
                        }
                    };
                });
            services.AddAuthorization();
            return services;
        }

        private static IEnumerable<SecurityKey> FetchPublicKeyFromIdentityService(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("IdentityService");
            var publicKeyPem = client.GetStringAsync("/api/auth/jwk").GetAwaiter().GetResult();
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            return [new RsaSecurityKey(rsa)];
        }
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
        {
            var mongoDbConnectionString = config.GetConnectionString("MongoDB") ?? throw new InfrastructureConfigurationException("ConnectionStrings:MongoDB");
            var rabbitMqConnectionString = config.GetConnectionString("RabbitMQ") ?? throw new InfrastructureConfigurationException("ConnectionStrings:RabbitMQ");
            var redisConnectionString = config.GetConnectionString("Redis") ?? throw new InfrastructureConfigurationException("ConnectionStrings:Redis");
            services.AddRabbitMQ(rabbitMqConnectionString)
                .AddMongoDb(mongoDbConnectionString)
                .AddRedisCache(redisConnectionString)
                .AddSingleton(TimeProvider.System)
                .AddDynamicJwkAuthentication(config);
            return services;
        }
    }
}
