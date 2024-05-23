using Xunit;
using rlisten.Models;
using System;

namespace rlisten.Tests.Models;

public class RedditOAuthTokensTests
{
    [Fact]
    public void AccessToken_SetUpdatesDirty()
    {
        // Arrange
        var tokens = new RedditOAuthTokens();
        string originalAccess = tokens.AccessToken;

        // Act
        tokens.AccessToken = "newAccessToken";

        // Assert
        Assert.NotEqual(originalAccess, tokens.AccessToken);
        Assert.True(tokens.NeedsWrite());
    }

    [Fact]
    public void RefreshToken_SetUpdatesDirty()
    {
        // Arrange
        var tokens = new RedditOAuthTokens();
        string originalRefresh = tokens.RefreshToken;

        // Act
        tokens.RefreshToken = "newRefreshToken";

        // Assert
        Assert.NotEqual(originalRefresh, tokens.RefreshToken);
        Assert.True(tokens.NeedsWrite());
    }
}
