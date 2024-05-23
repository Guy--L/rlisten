using rlisten;
using rlisten.Managers;
using rlisten.Models;
using rlisten.Services;
using rlisten.Wrappers;

public class rListener
{
    private static ConsoleColor inputPending = ConsoleColor.Blue;
    private static IRedditClient _redditClient;
    private static IConsoleIO _console;

    private readonly ISettingsManager _settingsManager;
    private readonly ISubRedditManager _subReddits;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public rListener(ISettingsManager settingsManager, IRedditService redditService, ISubRedditManager subRedditManager, IConsoleIO console)
    {
        _console = console;
        _settingsManager = settingsManager;
        _redditClient = redditService.GetRedditClient();
        _console.WriteLine($"{RedditService.Status} to reddit");
        _subReddits = subRedditManager;
        _cancellationTokenSource = new CancellationTokenSource();

        SubRedditManager.TopPostChanged = UpdateTopPost;
        SubRedditManager.TopUserChanged = UpdateTopUser;
    }

    public static void SetConsole(IConsoleIO console)
    {
        _console = console;
    }

    public async Task Run()
    {
        var restore = _settingsManager.Read<SubredditList>();

        restore.Subreddits.ForEach(name => {
            _console.WriteLine(_subReddits.Add(name));
        });

        // Start the background task to keep the application running
        var backgroundTask = Task.Run(() => KeepApplicationRunning(_cancellationTokenSource.Token));
        var originalColor = _console.ForegroundColor;

        _console.ForegroundColor = inputPending;
        _console.WriteLine($"All output is {inputPending} while input is pending.");
        _console.ForegroundColor = originalColor;

        // Main thread listens for new subreddit names and other controls
        ConsoleKey key = restore.Subreddits.Count > 0 ? ConsoleKey.Select : ConsoleKey.Enter;
        do
        {
            switch (key)
            {
                case ConsoleKey.X:
                    _cancellationTokenSource.Cancel();
                    _console.WriteLine("Exiting...");
                    return;

                case ConsoleKey.R:
                    _console.ForegroundColor = inputPending;
                    _console.WriteLine("________________________________");
                    _subReddits.Menu().ForEach(_console.WriteLine);
                    _console.WriteLine("Enter subreddit index or name to remove: ");
                    string remove = _console.ReadLine();
                    _console.ForegroundColor = originalColor;
                    _console.WriteLine(_subReddits.Remove(remove));
                    break;

                case ConsoleKey.Select:
                    break;

                default:
                    _console.ForegroundColor = inputPending;
                    _console.Write("Enter subreddit name to add: /r/");
                    string add = _console.ReadLine();
                    _console.ForegroundColor = originalColor;
                    _console.WriteLine(_subReddits.Add(add));
                    break;
            }
            _console.WriteLine("Press any key to add a new subreddit to monitor collection " +
                         $"(currently {_subReddits.Count})... (except e[X]it, or [R]emove)");
            key = _console.ReadKey(intercept: true).Key;
        } while (true);
    }

    public static async Task KeepApplicationRunning(CancellationToken cancellationToken)
    {
        try
        {
            // Keep the task alive indefinitely or until cancellation is requested
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            _console.WriteLine("Background task was cancelled.");
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Error: {ex.Message}");
        }
    }

    public void UpdateTopPost(IPost topPost)
    {
        _console.WriteLine($"Top Post for /r/{topPost.Subreddit}");
        _console.WriteLine($"                {topPost.Title}");
        _console.WriteLine($"                {topPost.UpVotes} upvotes, by {topPost.Author}");
    }

    public void UpdateTopUser(TopUser topUser)
    {
        _console.WriteLine($"Top User for /r/{topUser.SubReddit}");
        _console.WriteLine($"                {topUser.PostCount} posts, by {topUser.User}");
    }
}
