using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

using rlisten.Models;
using rlisten.Wrappers;

namespace rlisten.Services;

public interface ISubRedditStatService
{
    event Action<IPost> TopPostChanged;
    event Action<TopUser> TopUserChanged;

    DateTime started { get; }
    string Name { get; }

    IPost? GetTopPost();
    void Dispose();
    void SubscribeToChanges();
}

public class SubRedditStatService : ISubRedditStatService
{
    private readonly IRedditClient redditClient;
    private ISubreddit subreddit;

    // Cache for this subreddit monitoring updates
    private IPost? topPost;
    private TopUser topUser;
    private CancellationTokenSource votePoll = new CancellationTokenSource();

    // Define the events for updating top post and top user
    public event Action<IPost> TopPostChanged;
    public event Action<TopUser> TopUserChanged;

    public DateTime started { private set; get; }
    public string Name => subreddit.Name;

    public SubRedditStatService(IRedditService redditService, string name)
    {
        redditClient = redditService.GetRedditClient();
        try
        {
            subreddit = redditClient.Subreddit(name);
            started = DateTime.Now;

            topPost = GetTopPost();
            topUser = GetTopUser();
        }
        catch (Exception)    // subreddit not found, etc.
        {
            throw;
        }
    }

    public IPost? GetTopPost() => subreddit.Posts
                             .GetTop(t: "all", limit: 10)
                             .OrderByDescending(p => p.UpVotes)
                             .FirstOrDefault();

    public TopUser GetTopUser()
    {
        var posts = subreddit?.Posts.GetNew(limit: 1000);
        if (posts.Count()==0)
            return new TopUser(subreddit.Name);

        Dictionary<string, int> userPosts = new Dictionary<string, int>();

        int count;
        posts.ToList().ForEach(p =>
        {
            userPosts[p.Author] = userPosts.TryGetValue(p.Author, out count) ? count + 1 : 1;
        });

        return new TopUser(userPosts.Aggregate((l, r) => l.Value > r.Value ? l : r), subreddit.Name);
    }

    /// <summary>
    /// Poll every 10 seconds for a new top upvoted post
    /// PostScoreUpdated event published in reddit.net does not work so polling is implemented
    /// </summary>
    /// <returns>Task</returns>
    private async Task MonitorTopPost()
    {
        while (!votePoll.IsCancellationRequested)
        {
            var newTop = GetTopPost();
            if (newTop != null)
            {
                if (topPost == null || topPost.Id != newTop.Id || topPost.UpVotes != newTop.UpVotes)
                {
                    topPost = newTop;
                    TopPostChanged?.Invoke(topPost);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(10), votePoll.Token);
        }
    }

    // Listen to subreddit relying on reddit.net's request throttling system
    public async void SubscribeToChanges()
    {
        if (subreddit == null)
            return;

        await Task.Delay(1000);

        // Report initial stats
        TopPostChanged?.Invoke(topPost);
        TopUserChanged?.Invoke(topUser);

        subreddit.Posts.NewUpdated += Posts_NewUpdated;
        subreddit.Posts.MonitorNew();

        _ = Task.Run(() => MonitorTopPost(), votePoll.Token);
    }

    private void Posts_NewUpdated(object? sender, PostsUpdateEventArgs e)
    {
        Dictionary<string, int> userPosts = new Dictionary<string, int>();

        e.NewPosts.ForEach(p =>
        {
            userPosts[p.Author] = userPosts.TryGetValue(p.Author, out var count) ? count + 1 : 1;
        });
        var newTopUser = userPosts.Aggregate((l, r) => l.Value > r.Value ? l : r);

        if (topUser.User != newTopUser.Key || topUser.PostCount != newTopUser.Value)
        {
            topUser.User = newTopUser.Key;
            topUser.PostCount = newTopUser.Value;
            TopUserChanged?.Invoke(topUser);
        }
    }

    public void Dispose()
    {
        if (subreddit == null)
            return;

        subreddit.Posts.NewUpdated -= Posts_NewUpdated;
        subreddit.Posts.MonitorNew();

        votePoll.Cancel();
    }
}

