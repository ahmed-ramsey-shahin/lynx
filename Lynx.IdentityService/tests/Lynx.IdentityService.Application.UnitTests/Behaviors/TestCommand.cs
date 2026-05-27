using FluentValidation;
using Lynx.IdentityService.Application.Common.Interfaces;

namespace Lynx.IdentityService.Application.UnitTests.Behaviors
{
    public record TestResponse(int Id);

    public record TestCommand(string Name, string IdempotencyKey) : ICachedQuery<TestResponse>, IIdempotentCommand
    {
        public string CacheKey => "TestCacheKey";

        public TimeSpan Expiration => TimeSpan.FromMinutes(15);
    }


    public class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .WithMessage("Name is required.");
        }
    }
}
