using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using rlisten.Models;
using rlisten.Services;
using rlisten.Wrappers;
using Xunit;

namespace rlisten.Tests.Services;

public class SubRedditStatServiceTests
{
    private readonly Mock<IRedditService> _redditServiceMock;
    private readonly Mock<IRedditClient> _redditClientMock;
    private readonly Mock<ISubreddit> _subredditMock;
    private readonly Mock<ISubredditPosts> _subredditPostsMock;
    private readonly List<MockPost> _posts;

    public SubRedditStatServiceTests()
    {
        _redditServiceMock = new Mock<IRedditService>();
        _redditClientMock = new Mock<IRedditClient>();
        _subredditMock = new Mock<ISubreddit>();
        _subredditPostsMock = new Mock<ISubredditPosts>();

        _redditServiceMock.Setup(rs => rs.GetRedditClient()).Returns(_redditClientMock.Object);
        _redditClientMock.Setup(rc => rc.Subreddit(It.IsAny<string>())).Returns(_subredditMock.Object);
        _subredditMock.Setup(s => s.Posts).Returns(_subredditPostsMock.Object);
        _subredditMock.Setup(s => s.Name).Returns("testsubreddit");

        _posts = new List<MockPost>()
        {
            new ("user a", 10),
            new ("user b", 12),
            new ("user b", 5)
        };
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        _subredditPostsMock.Setup(sp => sp.GetTop(It.IsAny<string>(), It.IsAny<int>())).Returns(_posts);
        _subredditPostsMock.Setup(sp => sp.GetNew(It.IsAny<int>())).Returns(_posts);

        // Act
        var service = new SubRedditStatService(_redditServiceMock.Object, "testsubreddit");

        // Assert
        Assert.Equal("testsubreddit", service.Name);
        Assert.NotNull(service.started);
        Assert.Equal(_posts[1], service.GetTopPost());
        Assert.Equal("user b", service.GetTopUser().User); // Assuming the top user calculation is based on number of posts
        _redditClientMock.Verify(rc => rc.Subreddit(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetTopPost_ShouldReturnTopPost()
    {
        // Arrange
        var topPostMock = new Mock<IPost>();
        topPostMock.Setup(p => p.UpVotes).Returns(100);
        var postList = new List<IPost> { topPostMock.Object };
        _subredditPostsMock.Setup(sp => sp.GetTop(It.IsAny<string>(), It.IsAny<int>())).Returns(postList);

        var service = new SubRedditStatService(_redditServiceMock.Object, "testsubreddit");

        // Act
        var result = service.GetTopPost();

        // Assert
        Assert.Equal(topPostMock.Object, result);
    }

    [Fact]
    public void SubscribeToChanges_ShouldInvokeEvents()
    {
        // Arrange
        _subredditPostsMock.Setup(sp => sp.GetTop(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<IPost> { _posts[1] });
        _subredditPostsMock.Setup(sp => sp.GetNew(It.IsAny<int>())).Returns(_posts);

        var service = new SubRedditStatService(_redditServiceMock.Object, "testsubreddit");
        bool topPostChangedInvoked = false;
        bool topUserChangedInvoked = false;

        service.TopPostChanged += (post) => topPostChangedInvoked = true;
        service.TopUserChanged += (user) => topUserChangedInvoked = true;

        // Act
        service.SubscribeToChanges();
        var postsUpdateEventArgs = new PostsUpdateEventArgs { };
        _subredditPostsMock.Raise(sp => sp.NewUpdated += null, postsUpdateEventArgs);

        Thread.Sleep(11000); // 11 seconds to ensure the polling task runs

        // Assert
        Assert.True(topPostChangedInvoked, "topPostInvoked");
        Assert.True(topUserChangedInvoked, "topUserInvoked");
    }

    [Fact]
    public void Dispose_ShouldStopMonitoring()
    {
        // Arrange
        var service = new SubRedditStatService(_redditServiceMock.Object, "testsubreddit");

        // Act
        service.Dispose();

        // Assert
        _subredditPostsMock.Verify(sp => sp.MonitorNew(), Times.Once);
    }
}
