using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Infrastructure.BackgroundJobs
{
    public class EmailBackgroundWorker(
        IEmailBackgroundQueue emailQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundWorker> logger
    ) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background email worker is starting.");

            try
            {
                await foreach (var job in emailQueue.DequeueAllAsync(stoppingToken))
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("Sending background email {@EmailJob}.", job);
                        await emailService.SendEmailAsync(
                            job.To,
                            job.Username,
                            job.Subject,
                            job.Body,
                            stoppingToken
                        );
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send email {@EmailJob}.", job);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
