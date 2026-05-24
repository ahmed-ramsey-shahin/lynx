namespace Lynx.IdentityService.Infrastructure.Exceptions
{
    public class InfrastructureConfigurationException : Exception
    {
        public InfrastructureConfigurationException()
            : base("Critical configuration missing: Some settings could not be resolved from appsettings or environment variables.")
        {
        }

        public InfrastructureConfigurationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public InfrastructureConfigurationException(string settingName)
            : base($"Critical configuration missing: The setting '{settingName}' could not be resolved from appsettings or environment variables.")
        {
        }
    }
}
