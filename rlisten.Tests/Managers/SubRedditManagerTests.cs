using Moq;
using Xunit;
using rlisten.Managers;
using rlisten.Models;
using rlisten.Services;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace rlisten.Tests.Managers;

public class SubRedditManagerTests
{
    private readonly Mock<IRedditService> mockRedditService;
    private readonly Mock<ISubRedditStatService> mockSubRedditStatService;
    private readonly SubRedditStatServiceFactory mockSubRedditStatServiceFactory;
    private readonly SubRedditManager subRedditManager;

    public SubRedditManagerTests()
    {
        mockRedditService = new Mock<IRedditService>();
        mockSubRedditStatService = new Mock<ISubRedditStatService>();
        mockSubRedditStatServiceFactory = (redditService, subredditName) => mockSubRedditStatService.Object;

        subRedditManager = new SubRedditManager(mockRedditService.Object, mockSubRedditStatServiceFactory);
    }

    [Fact]
    public void Add_ShouldAddSubRedditToMonitorList()
    {
        // Act
        var result = subRedditManager.Add("testSubreddit");

        // Assert
        Assert.Equal("Started monitoring /r/testSubreddit.", result);
        Assert.Equal(1, subRedditManager.Count);
        mockSubRedditStatService.Verify(m => m.SubscribeToChanges(), Times.Once);
    }

    [Fact]
    public void Add_ShouldReturnAlreadyMonitoredMessage_WhenSubRedditIsAlreadyMonitored()
    {
        // Arrange
        subRedditManager.Add("testSubreddit");

        // Act
        var result = subRedditManager.Add("testSubreddit");

        // Assert
        Assert.Equal("/r/testSubreddit is already being monitored.", result);
        Assert.Equal(1, subRedditManager.Count);
    }

    [Fact]
    public void Remove_ShouldRemoveSubRedditFromMonitorList()
    {
        // Arrange
        subRedditManager.Add("testSubreddit");

        // Act
        var result = subRedditManager.Remove("testSubreddit");

        // Assert
        Assert.Equal("Stopped monitoring /r/testSubreddit.", result);
        Assert.Equal(0, subRedditManager.Count);
        mockSubRedditStatService.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void Remove_ShouldReturnNotMonitoredMessage_WhenSubRedditIsNotMonitored()
    {
        // Act
        var result = subRedditManager.Remove("nonexistentSubreddit");

        // Assert
        Assert.Equal("/r/nonexistentSubreddit is not being monitored.", result);
    }

    [Fact]
    public void Menu_ShouldReturnListOfMonitoredSubReddits_WithTime()
    {
        // Arrange
        mockSubRedditStatService.Setup(m => m.started).Returns(DateTime.Now);
        subRedditManager.Add("testSubreddit");

        // Act
        var menu = subRedditManager.Menu();

        // Assert
        Assert.Single(menu);
        Assert.Contains("testSubreddit", menu.First());
    }

    [Fact]
    public void Menu_ShouldReturnListOfMonitoredSubReddits_WithoutTime()
    {
        // Arrange
        subRedditManager.Add("testSubreddit");

        // Act
        var menu = subRedditManager.Menu(withTime: false);

        // Assert
        Assert.Single(menu);
        Assert.Contains("testSubreddit", menu.First());
    }

    [Fact]
    public void Dispose_ShouldDisposeAllMonitoredSubReddits()
    {
        // Arrange
        subRedditManager.Add("testSubreddit");

        // Act
        subRedditManager.Dispose();

        // Assert
        Assert.Equal(0, subRedditManager.Count);
        mockSubRedditStatService.Verify(m => m.Dispose(), Times.Once);
    }
}
