using System.Reflection;
using Hangfire;
using Lynx.IdentityService.Application.Features.Identity.Commands.DeletedUnactivatedUsers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lynx.IdentityService.Infrastructure.BackgroundJobs
{
    public static class BackgroundJobsInitializer
    {
        public static IApplicationBuilder UseBackgroundJobs(this IApplicationBuilder app)
        {
            var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
            var currentClass = typeof(BackgroundJobsInitializer);
            var privateMethods = currentClass.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(method => method.ReturnType == typeof(void) && method.GetParameters().Length == 1);

            foreach (var method in privateMethods)
            {
                method.Invoke(null, [ recurringJobManager ]);
            }

            return app;
        }

#pragma warning disable RCS1213, IDE0051
        private static void DailyCirculation(IRecurringJobManager recurringJobManager)
        {
            recurringJobManager.AddOrUpdate<ISender>(
                "delete-unactivated-users",
                sender => sender.Send(new DeleteUnactivatedUsersCommand(), default),
                Cron.Hourly
            );
        }
#pragma warning restore RCS1213, IDE0051
    }
}
