using Lynx.IdentityService.Infrastructure.BackgroundJobs;

namespace Lynx.IdentityService.Api
{
    public static class DependencyInjection
    {
        public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseCors(configuration["AppSettings:CorsPolicyName"]!);
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOutputCache();
            app.UseBackgroundJobs();
            return app;
        }
    }
}
