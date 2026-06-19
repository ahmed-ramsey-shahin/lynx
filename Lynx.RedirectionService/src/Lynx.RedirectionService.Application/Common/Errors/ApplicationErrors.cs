using Lynx.RedirectionService.Domain.Common.Results;

namespace Lynx.RedirectionService.Application.Common.Errors
{
    public static class ApplicationErrors
    {
        public static Error AliasAlreadyExists => Error.Validation("Url.AiasAlreadyExists", "This alias already exists. Please choose a different one.");
        public static Error UrlDoesNotExist => Error.NotFound("Url.UrlDoesNotExist", "The required url does not exist.");
        public static Error AliasGenerationFailed => Error.Failure("Url.AliasGenerationFailed", "Failed to generate alias for the link.");
    }
}
