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
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseBackgroundJobs();
            return app;
        }
    }
}
