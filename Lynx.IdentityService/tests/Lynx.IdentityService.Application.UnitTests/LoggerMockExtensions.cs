using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public static class LoggerMockExtensions
    {
        public static void VerifyLogWithException<T>(
            this Mock<ILogger<T>> loggerMock,
            LogLevel expectedLogLevel,
            string? expectedMessage,
            Exception expectedException,
            Times times
        )
        {
            loggerMock.Verify(
                x => x.Log
                (
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => expectedMessage == null || v.ToString()!.Contains(expectedMessage)),
                    It.Is<Exception>(e => e.Equals(expectedException)),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),times
            );
        }
    }
}
