using System.Net.Http.Json;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Lynx.IdentityService.Infrastructure.Services
{
    public class EmailService(
        IHttpClientFactory clientFactory,
        IOptions<EmailServiceConfigurations> config
    ) : IEmailService
    {
        private readonly HttpClient _emailClient = clientFactory.CreateClient("BrevoEmailClient");

        public async Task SendEmailAsync(string recipientEmail, string recipientName, string subjectTxt, string messageTxt, CancellationToken cancellationToken = default)
        {
            var requestPayload = new
            {
                subject = subjectTxt,
                htmlContent = messageTxt,
                sender = new
                {
                    name = config.Value.SenderName,
                    email = config.Value.SenderEmail
                },
                to = new[]
                {
                    new
                    {
                        email = recipientEmail,
                        name = recipientName
                    }
                }
            };
            var response = await _emailClient.PostAsJsonAsync("", requestPayload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
