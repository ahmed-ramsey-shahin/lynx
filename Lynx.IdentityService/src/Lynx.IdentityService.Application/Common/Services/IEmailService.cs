namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientEmail, string recipientName, string subjectTxt, string messageTxt, CancellationToken cancellationToken);
    }
}
