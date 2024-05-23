namespace rlisten.Tests.Services;

using Moq;
using Xunit;
using rlisten.Managers;
using rlisten.Models;
using rlisten.Wrappers;
using Reddit.AuthTokenRetriever.EventArgs;
using rlisten.Services;
using System.Diagnostics;
using System.Threading.Tasks;

public class AuthServiceTests
{
    private readonly Mock<ISettingsManager> mockSettingsManager;
    private readonly Mock<IAuthTokenRetriever> mockAuthTokenRetriever;
    private readonly Mock<IProcessWrapper> mockProcessWrapper;
    private readonly AuthService authService;
    private readonly RedditOAuthTokens testTokens;

    public AuthServiceTests()
    {
        mockSettingsManager = new Mock<ISettingsManager>();
        mockAuthTokenRetriever = new Mock<IAuthTokenRetriever>();
        mockProcessWrapper = new Mock<IProcessWrapper>();
        testTokens = new RedditOAuthTokens { AccessToken = "test_access", RefreshToken = "test_refresh" };

        mockSettingsManager.Setup(sm => sm.Read<RedditOAuthTokens>()).Returns(testTokens);

        authService = new AuthService(mockSettingsManager.Object, mockAuthTokenRetriever.Object, mockProcessWrapper.Object);
    }

    [Fact]
    public async Task ReauthorizeAsync_ShouldInvokeAuthTokenRetrieverAndOpenBrowser()
    {
        // Arrange
        mockAuthTokenRetriever.SetupGet(x => x.AuthURL).Returns("http://auth.url");
        mockAuthTokenRetriever.SetupGet(x => x.BrowserPath).Returns("path/to/browser");

        // Act
        var reauthorizeTask = authService.ReauthorizeAsync();

        // Simulate successful authentication
        mockAuthTokenRetriever.Raise(x => x.AuthSuccess += null, null, new AuthSuccessEventArgs { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" });

        await reauthorizeTask;

        // Assert
        mockAuthTokenRetriever.Verify(x => x.AwaitCallback(), Times.Once);
        mockProcessWrapper.Verify(x => x.Start(It.Is<ProcessStartInfo>(psi => psi.FileName == "http://auth.url" && psi.UseShellExecute == true)), Times.Once);
        mockSettingsManager.Verify(sm => sm.Write(It.Is<RedditOAuthTokens>(tokens => tokens.AccessToken == "new_access_token" && tokens.RefreshToken == "new_refresh_token")), Times.Once);
    }

    [Fact]
    public async Task ReauthorizeAsync_ShouldFallbackToBrowserPath_WhenDefaultBrowserFails()
    {
        // Arrange
        mockAuthTokenRetriever.SetupGet(x => x.AuthURL).Returns("http://auth.url");
        mockAuthTokenRetriever.SetupGet(x => x.BrowserPath).Returns("path/to/browser");

        // Setup to throw on the first call and succeed on the second call
        var callCount = 0;
        mockProcessWrapper.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .Callback<ProcessStartInfo>(psi =>
            {
                if (callCount == 0)
                {
                    callCount++;
                    throw new System.ComponentModel.Win32Exception();
                }
            });

        // Act
        var reauthorizeTask = authService.ReauthorizeAsync();

        // Simulate successful authentication
        mockAuthTokenRetriever.Raise(x => x.AuthSuccess += null, null, new AuthSuccessEventArgs { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" });

        await reauthorizeTask;

        // Assert
        // Verify that Start was attempted and failed on the first call
        mockProcessWrapper.Verify(x => x.Start(It.Is<ProcessStartInfo>(psi => psi.Arguments == "http://auth.url")), Times.Once);
        // Verify that the second call used the fallback browser path
        mockProcessWrapper.Verify(x => x.Start(It.Is<ProcessStartInfo>(psi => psi.FileName == "path/to/browser" && psi.Arguments == "http://auth.url")), Times.Once);
    }


    [Fact]
    public void OnAuthSuccessful_ShouldUpdateTokensAndSetTaskResult()
    {
        // Arrange
        var authSuccessEventArgs = new AuthSuccessEventArgs { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" };
        var tcs = new TaskCompletionSource<bool>();
        authService.GetType().GetField("authCompletionSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(authService, tcs);

        // Act
        authService.GetType().GetMethod("OnAuthSuccessful", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(authService, new object[] { null, authSuccessEventArgs });

        // Assert
        Assert.Equal("new_access_token", testTokens.AccessToken);
        Assert.Equal("new_refresh_token", testTokens.RefreshToken);
        Assert.True(tcs.Task.IsCompletedSuccessfully);
    }
}
