using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Infrastructure.Data;
using Lynx.IdentityService.Infrastructure.Data.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Lynx.IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static async Task ConfigureMongoDbAsync(IServiceProvider serviceProvider)
        {
            MongoDbConfiguration.ConfigureMappings();
            await MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(serviceProvider.GetService<IMongoClient>()!);
        }

        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
        {
            var mongoDbConnectionString = config.GetConnectionString("MongoDbConnectionString");
            services.AddSingleton<IMongoClient>(new MongoClient(mongoDbConnectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }
    }
}
