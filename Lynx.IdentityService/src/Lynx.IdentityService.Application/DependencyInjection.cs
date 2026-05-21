using System.Reflection;
using FluentValidation;
using Lynx.IdentityService.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Lynx.IdentityService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
                configuration.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
                configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
                configuration.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
                configuration.AddOpenBehavior(typeof(CachingBehavior<,>));
            });
            return services;
        }
    }
}
