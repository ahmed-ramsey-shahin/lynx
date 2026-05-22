namespace Lynx.IdentityService.Application.Common.Settings
{
    public class ClientUrlOptions
    {
        public const string SectionName = "ClientUrls";

        public string ResetPasswordUrl { get; set; } = string.Empty;
        public string ActivateAccountUrl { get; set; } = string.Empty;
    }
}
