using Moq;
using rlisten.Managers;
using rlisten.Services;
using rlisten.Wrappers;
using rlisten;
using rlisten.Models;

public class rListenerTests
{
    private readonly Mock<ISettingsManager> _mockSettingsManager;
    private readonly Mock<IRedditService> _mockRedditService;
    private readonly Mock<ISubRedditManager> _mockSubRedditManager;
    private readonly Mock<IConsoleIO> _mockConsole;
    private readonly rListener _rListener;

    public rListenerTests()
    {
        _mockSettingsManager = new Mock<ISettingsManager>();
        _mockRedditService = new Mock<IRedditService>();
        _mockSubRedditManager = new Mock<ISubRedditManager>();
        _mockConsole = new Mock<IConsoleIO>();

        _mockRedditService.Setup(s => s.GetRedditClient()).Returns(Mock.Of<IRedditClient>());

        _rListener = new rListener(_mockSettingsManager.Object, _mockRedditService.Object, _mockSubRedditManager.Object, _mockConsole.Object);
        rListener.SetConsole(_mockConsole.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeDependencies()
    {
        Assert.NotNull(_rListener);
    }

    [Fact]
    public async Task Run_ShouldReadSettingsAndAddSubreddits()
    {
        var subredditList = new SubredditList { Subreddits = new List<string> { "testsubreddit" } };
        _mockSettingsManager.Setup(s => s.Read<SubredditList>()).Returns(subredditList);
        _mockSubRedditManager.Setup(m => m.Add(It.IsAny<string>())).Returns("Subreddit added");

        _mockConsole.SetupSequence(c => c.ReadKey(It.IsAny<bool>()))
            .Returns(new ConsoleKeyInfo('\0', ConsoleKey.X, false, false, false));

        await _rListener.Run();

        _mockSettingsManager.Verify(s => s.Read<SubredditList>(), Times.Once);
        _mockSubRedditManager.Verify(m => m.Add("testsubreddit"), Times.Once);
    }

    [Fact]
    public void UpdateTopPost_ShouldOutputCorrectInformation()
    {
        var topPost = new MockPost("Author", 100) { Title = "Top Post", Subreddit = "testsubreddit" };
        _rListener.UpdateTopPost(topPost);

        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Top Post for /r/testsubreddit"))));
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Top Post"))));
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("100 upvotes, by Author"))));
    }

    [Fact]
    public void UpdateTopUser_ShouldOutputCorrectInformation()
    {
        var topUser = new TopUser("testsubreddit") { PostCount = 10, User = "TestUser" };

        _rListener.UpdateTopUser(topUser);

        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Top User for /r/testsubreddit"))));
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("10 posts, by TestUser"))));
    }

    [Fact]
    public async Task KeepApplicationRunning_ShouldCancelTask()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        var task = rListener.KeepApplicationRunning(cancellationTokenSource.Token);

        cancellationTokenSource.Cancel();

        // Wait a moment to ensure the cancellation has been processed
        await Task.Delay(100);

        // Verify that the cancellation message was printed
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Background task was cancelled."))), Times.AtLeastOnce);
    }
}
