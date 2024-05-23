using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

namespace rlisten.Wrappers;

public class RedditClientAdapter : IRedditClient
{
    private readonly RedditClient _redditClient;

    public RedditClientAdapter(string accessToken)
    {
        _redditClient = new RedditClient(accessToken: accessToken);
    }

    public RedditClientAdapter(string refreshToken, bool isRefreshToken)
    {
        _redditClient = new RedditClient(refreshToken: refreshToken);
    }

    public ISubreddit Subreddit(string name)
    {
        var subreddit = _redditClient.Subreddit(name);
        return new SubredditAdapter(subreddit);
    }

    public async Task InitializeClientAsync(string accessToken)
    {
        // Initialization is handled in the constructor
        await Task.CompletedTask;
    }

    public async Task InitializeClientWithRefreshTokenAsync(string refreshToken)
    {
        // Initialization is handled in the constructor
        await Task.CompletedTask;
    }
}

public class SubredditAdapter : ISubreddit
{
    private readonly Subreddit _subreddit;

    public SubredditAdapter(Subreddit subreddit)
    {
        _subreddit = subreddit;
    }

    public ISubredditPosts Posts => new SubredditPostsAdapter(_subreddit.Posts);

    public string Name => _subreddit.Name;
}

public class SubredditPostsAdapter : ISubredditPosts
{
    private readonly SubredditPosts _subredditPosts;

    public SubredditPostsAdapter(SubredditPosts subredditPosts)
    {
        _subredditPosts = subredditPosts;
    }

    public event EventHandler<PostsUpdateEventArgs> NewUpdated
    {
        add { _subredditPosts.NewUpdated += value; }
        remove { _subredditPosts.NewUpdated -= value; }
    }

    public bool MonitorNew() => _subredditPosts.MonitorNew();
    public bool NewPostsIsMonitored() => _subredditPosts.NewPostsIsMonitored();

    public IEnumerable<IPost> GetTop(string t, int limit) => _subredditPosts.GetTop(t, limit: limit).Select(p => new PostWrapper(p));
    public IEnumerable<IPost> GetNew(int limit) => _subredditPosts.GetNew(limit: limit).Select(p => new PostWrapper(p));
    public IEnumerable<IPost> GetHot() => _subredditPosts.GetHot().Select(p => new PostWrapper(p));
}
