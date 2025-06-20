using General.Utils.RetryPolicies;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Exceptions;

namespace CheckMade.Tests.Unit.General.Utils;

public sealed class RetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_CatchesInnerDotnetTargetException_AndRetries_WhenWrappedInThirdPartyException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RetryPolicyBase>>();
        var policy = new NetworkRetryPolicy(loggerMock.Object);
        var executionCount = 0;

        // Act
        await policy.ExecuteAsync(async () =>
        {
            executionCount++;
            
            if (executionCount <= 1) // Only throw on the first execution
            {
                var innerException = new HttpRequestException("Inner system/.NET exception");
                throw new RequestException("Outer Telegram exception", innerException);
            }

            await Task.CompletedTask;
        });

        // Assert
        Assert.Equal(2, executionCount);
        
        loggerMock.Verify(
            static x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(static (v, t) => true),
                It.IsAny<RequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}