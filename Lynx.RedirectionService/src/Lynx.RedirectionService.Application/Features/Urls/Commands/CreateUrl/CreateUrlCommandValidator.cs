using FluentValidation;
using Lynx.RedirectionService.Domain.Urls;

namespace Lynx.RedirectionService.Application.Features.Urls.Commands.CreateUrl
{
    public class CreateUrlCommandValidator : AbstractValidator<CreateUrlCommand>
    {
        public CreateUrlCommandValidator()
        {
            RuleFor(command => command.LongUrl)
                .NotEmpty()
                    .WithErrorCode(UrlErrors.UrlRequired.Code)
                    .WithMessage(UrlErrors.UrlRequired.Description)
                .Must(BeAValidUrl)
                    .WithErrorCode(UrlErrors.UrlInvalid.Code)
                    .WithMessage(UrlErrors.UrlInvalid.Description);
        }
        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var outUri)
                   && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
