using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Application.Features.Urls.Commands.CreateUrl;
using Lynx.RedirectionService.Application.UnitTests.MockBuilders;
using Lynx.RedirectionService.Domain.Urls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests
{
    public class CreateUrlCommandTests
    {
        private readonly Mock<ILogger<CreateUrlCommandHandler>> _logger = new(MockBehavior.Loose);
        private readonly FakeTimeProvider _timeProvider;
        private CreateUrlCommandHandler? _handler;

        public CreateUrlCommandTests()
        {
            _timeProvider = new();
            _timeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 24, 3, 47, 21, TimeSpan.Zero));
        }

        private void CreateHandler(IUrlRepository urlRepo, IGenerateAliasService aliasService)
        {
            _handler = new CreateUrlCommandHandler(_logger.Object, urlRepo, aliasService, _timeProvider);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, 5)]
        [InlineData("custom alias", null)]
        [InlineData("custom alias", 5)]
        public async Task Handler_Should_ReturnUrlId_When_AllParametersAreValid(string? alias, int? expirationInDays)
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string idempotencyKey = Guid.NewGuid().ToString();
            const string longUrl = "long url";
            const string generatedAlias = "generated alias";
            string finalAlias = alias ?? generatedAlias;
            CreateUrlCommand command = new()
            {
                LongUrl = longUrl,
                UserId = userId,
                CustomAlias = alias,
                ExpirationInDays = expirationInDays,
                IdempotencyKey = idempotencyKey
            };
            var generateAliasMock = new GenerateAliasServiceMockBuilder().WithAlias(generatedAlias);
            var urlRepoMock = new UrlRepositoryMockBuilder()
                .WithAliasUnique(finalAlias)
                .WithSuccessfullDatabaseAdd();
            CreateHandler(urlRepoMock.Object, generateAliasMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var id = result.Value;
            urlRepoMock.Mock.Verify(repo => repo.AliasExistsAsync(finalAlias, It.IsAny<CancellationToken>()), Times.Once());

            if (alias is null)
            {
                generateAliasMock.Mock.Verify(service => service.Generate(), Times.Once());
            }
            else
            {
                generateAliasMock.Mock.Verify(service => service.Generate(), Times.Never());
            }

            urlRepoMock.Mock.Verify(repo => repo.AddAsync(
                It.Is<Url>(url =>
                    url.Alias == finalAlias &&
                    url.LongUrl == longUrl &&
                    url.UserId == userId &&
                    url.ExpirationDate == _timeProvider.GetUtcNow().AddDays(expirationInDays ?? 30)
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnAliasAlreadyExists_When_CustomAliasAlreadyExists()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string idempotencyKey = Guid.NewGuid().ToString();
            const string longUrl = "long url";
            const string customAlias = "custom alias";
            const int expirationInDays = 3;
            CreateUrlCommand command = new()
            {
                LongUrl = longUrl,
                UserId = userId,
                CustomAlias = customAlias,
                ExpirationInDays = expirationInDays,
                IdempotencyKey = idempotencyKey
            };
            var generateAliasMock = new GenerateAliasServiceMockBuilder();
            var urlRepoMock = new UrlRepositoryMockBuilder().WithAliasExists(customAlias);
            CreateHandler(urlRepoMock.Object, generateAliasMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.AliasAlreadyExists.Code);
            urlRepoMock.Mock.Verify(repo => repo.AliasExistsAsync(customAlias, It.IsAny<CancellationToken>()), Times.Once());
            generateAliasMock.Mock.Verify(service => service.Generate(), Times.Never());
            urlRepoMock.Mock.Verify(repo => repo.AddAsync(
                It.IsAny<Url>(),
                It.IsAny<CancellationToken>()
            ), Times.Never());
        }

        [Fact]
        public async Task Handler_Should_ReturnAliasGenerationFailed_When_AliasGenerationAlwaysExists()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string idempotencyKey = Guid.NewGuid().ToString();
            const string longUrl = "long url";
            const string generatedAlias = "custom alias";
            const int expirationInDays = 3;
            CreateUrlCommand command = new()
            {
                LongUrl = longUrl,
                UserId = userId,
                CustomAlias = null,
                ExpirationInDays = expirationInDays,
                IdempotencyKey = idempotencyKey
            };
            var generateAliasMock = new GenerateAliasServiceMockBuilder().WithAlias(generatedAlias);
            var urlRepoMock = new UrlRepositoryMockBuilder().WithAliasExists(generatedAlias);
            CreateHandler(urlRepoMock.Object, generateAliasMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.AliasGenerationFailed.Code);
            urlRepoMock.Mock.Verify(repo => repo.AliasExistsAsync(generatedAlias, It.IsAny<CancellationToken>()), Times.Exactly(3));
            generateAliasMock.Mock.Verify(service => service.Generate(), Times.Exactly(4));
            urlRepoMock.Mock.Verify(repo => repo.AddAsync(
                It.IsAny<Url>(),
                It.IsAny<CancellationToken>()
            ), Times.Never());
        }

        [Fact]
        public async Task Handler_Should_ReturnUrlId_When_AliasGenerationFailesTwoTimesOnly()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string idempotencyKey = Guid.NewGuid().ToString();
            const string longUrl = "long url";
            const string generatedAlias = "generated alias";
            const int expirationInDays = 3;
            CreateUrlCommand command = new()
            {
                LongUrl = longUrl,
                UserId = userId,
                CustomAlias = null,
                ExpirationInDays = expirationInDays,
                IdempotencyKey = idempotencyKey
            };
            var generateAliasMock = new GenerateAliasServiceMockBuilder().WithAlias(generatedAlias);
            var urlRepoMock = new UrlRepositoryMockBuilder().WithSuccessfullDatabaseAdd();
            urlRepoMock.Mock.SetupSequence(repo => repo.AliasExistsAsync(generatedAlias, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            CreateHandler(urlRepoMock.Object, generateAliasMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            urlRepoMock.Mock.Verify(repo => repo.AliasExistsAsync(generatedAlias, It.IsAny<CancellationToken>()), Times.Exactly(3));
            generateAliasMock.Mock.Verify(service => service.Generate(), Times.Exactly(3));
            urlRepoMock.Mock.Verify(repo => repo.AddAsync(
                It.IsAny<Url>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }
    }
}
