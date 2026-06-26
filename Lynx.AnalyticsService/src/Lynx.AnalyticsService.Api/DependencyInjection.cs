namespace Lynx.AnalyticsService.Api
{
    public static class DependencyInjection
    {
        public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
