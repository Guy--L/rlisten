using Moq;
using rlisten.Services;
using Reddit.Controllers;
using rlisten.Managers;
using rlisten.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using rlisten.Wrappers;

namespace rlisten.Tests.Services;

public class RedditServiceTests
{
    private readonly Mock<ISettingsManager> settingsManagerMock;
    private readonly Mock<IRedditClient> redditClientMock;
    private readonly Mock<IAuthService> authServiceMock;
    private readonly RedditService redditService;
    private readonly RedditOAuthTokens redditOAuthTokens;

    public RedditServiceTests()
    {
        settingsManagerMock = new Mock<ISettingsManager>();
        redditClientMock = new Mock<IRedditClient>();
        authServiceMock = new Mock<IAuthService>();
        redditOAuthTokens = new RedditOAuthTokens
        {
            AccessToken = "testAccessToken",
            RefreshToken = "testRefreshToken"
        };

        settingsManagerMock.Setup(sm => sm.Read<RedditOAuthTokens>()).Returns(redditOAuthTokens);
        redditClientMock.Setup(rc => rc.Subreddit(It.IsAny<string>()).Posts.GetHot()).Returns(new Mock<List<IPost>>().Object);

        redditService = new RedditService(settingsManagerMock.Object, redditClientMock.Object, authServiceMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_ShouldInitializeClientWithAccessToken()
    {
        // Arrange
        redditClientMock.Setup(rc => rc.InitializeClientAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await redditService.InitializeAsync();

        // Assert
        redditClientMock.Verify(rc => rc.InitializeClientAsync(redditOAuthTokens.AccessToken), Times.AtLeastOnce);
        Assert.Equal("Initialized", RedditService.Status);
    }

    [Fact]
    public async Task InitializeAsync_ShouldInitializeClientWithRefreshTokenOnFailure()
    {
        // Arrange
        redditClientMock.SetupSequence(rc => rc.InitializeClientAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception())
            .Returns(Task.CompletedTask);
        redditClientMock.Setup(rc => rc.InitializeClientWithRefreshTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await redditService.InitializeAsync();

        // Assert
        redditClientMock.Verify(rc => rc.InitializeClientAsync(redditOAuthTokens.AccessToken), Times.Exactly(2));
        redditClientMock.Verify(rc => rc.InitializeClientWithRefreshTokenAsync(redditOAuthTokens.RefreshToken), Times.Once);
        Assert.Equal("Refreshed", RedditService.Status);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReauthorizeAndInitializeClientOnFailure()
    {
        // Arrange
        authServiceMock.Setup(auth => auth.ReauthorizeAsync()).ReturnsAsync("newAccessToken");
        redditClientMock.SetupSequence(rc => rc.InitializeClientAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception())
            .Returns(Task.CompletedTask);
        redditClientMock.SetupSequence(rc => rc.InitializeClientWithRefreshTokenAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception())
            .Returns(Task.CompletedTask);
        redditClientMock.Setup(rc => rc.InitializeClientAsync("newAccessToken")).Returns(Task.CompletedTask);

        // Act
        await redditService.InitializeAsync();

        // Assert
        authServiceMock.Verify(auth => auth.ReauthorizeAsync(), Times.Once);
        redditClientMock.Verify(rc => rc.InitializeClientAsync("newAccessToken"), Times.Once);
        Assert.Equal("Reauthorized", RedditService.Status);
    }

    [Fact]
    public async Task InitializeAsync_ShouldSetStatusToFailedOnAllFailures()
    {
        // Arrange
        redditClientMock.Setup(rc => rc.InitializeClientAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
        redditClientMock.Setup(rc => rc.InitializeClientWithRefreshTokenAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
        authServiceMock.Setup(auth => auth.ReauthorizeAsync()).ThrowsAsync(new Exception());

        // Act
        await Assert.ThrowsAsync<Exception>(async () => await redditService.InitializeAsync());

        // Assert
        authServiceMock.Verify(auth => auth.ReauthorizeAsync(), Times.Once);
        Assert.NotEqual("Initialized", RedditService.Status);
        Assert.NotEqual("Refreshed", RedditService.Status);
        Assert.NotEqual("Reauthorized", RedditService.Status);
    }
}
