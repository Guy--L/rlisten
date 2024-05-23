using System;
using System.Diagnostics;
using Reddit;
using Reddit.AuthTokenRetriever;
using Reddit.AuthTokenRetriever.EventArgs;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

namespace rlisten.Wrappers;

public interface IPost
{
    string Id { get; }
    string Title { get; }
    string Author { get; }
    DateTime Created { get; }
    int UpVotes { get; }
    int DownVotes { get; }
    string Permalink { get; }
    string Subreddit { get; }

    void Upvote();
    void Downvote();
    void Hide();
}

public interface IRedditClient
{
    ISubreddit Subreddit(string name);
    Task InitializeClientAsync(string accessToken);
    Task InitializeClientWithRefreshTokenAsync(string refreshToken);
}

public interface ISubreddit
{
    ISubredditPosts Posts { get; }
    string Name { get; }
}

public interface ISubredditPosts
{
    event EventHandler<PostsUpdateEventArgs> NewUpdated;

    IEnumerable<IPost> GetTop(string t, int limit);
    IEnumerable<IPost> GetNew(int limit);
    IEnumerable<IPost> GetHot();

    bool MonitorNew();
    bool NewPostsIsMonitored();
}

public interface IAuthTokenRetriever
{
    event EventHandler<AuthSuccessEventArgs> AuthSuccess;
    void AwaitCallback();
    string AuthURL { get; }
    string BrowserPath { get; }
    void StopListening();
}

public interface IProcessWrapper
{
    void Start(ProcessStartInfo processStartInfo);
}

public class ProcessWrapper : IProcessWrapper
{
    public void Start(ProcessStartInfo processStartInfo)
    {
        Process.Start(processStartInfo);
    }
}

public class RedditClientWrapper : IRedditClient
{
    private IRedditClient _redditClient;

    public ISubreddit Subreddit(string name) => _redditClient.Subreddit(name);

    public async Task InitializeClientAsync(string accessToken)
    {
        _redditClient = new RedditClientAdapter(accessToken);
        await Task.CompletedTask;
    }

    public async Task InitializeClientWithRefreshTokenAsync(string refreshToken)
    {
        _redditClient = new RedditClientAdapter(refreshToken, isRefreshToken: true);
        await Task.CompletedTask;
    }
}

public class SubredditWrapper : ISubreddit
{
    private readonly ISubreddit _subreddit;
    private readonly ISubredditPosts _posts;

    public SubredditWrapper(ISubreddit subreddit)
    {
        _subreddit = subreddit;
        _posts = _subreddit.Posts;
    }
    public string Name => _subreddit.Name;
    public ISubredditPosts Posts => _posts;
}

public class SubredditPostsWrapper : ISubredditPosts
{
    private readonly ISubredditPosts _subredditPosts;

    public SubredditPostsWrapper(ISubredditPosts subredditPosts)
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

    public IEnumerable<IPost> GetTop(string t, int limit) => _subredditPosts.GetTop(t: t, limit: limit);
    public IEnumerable<IPost> GetNew(int limit) => _subredditPosts.GetNew(limit: limit);
    public IEnumerable<IPost> GetHot() => _subredditPosts.GetHot();
}

public class AuthTokenRetrieverWrapper : IAuthTokenRetriever
{
    private readonly AuthTokenRetrieverLib _authTokenRetriever;
    private readonly string _browserPath;

    public AuthTokenRetrieverWrapper(AuthTokenRetrieverLib authTokenRetriever, string browserPath)
    {
        _authTokenRetriever = authTokenRetriever;
        _browserPath = browserPath;
    }

    public event EventHandler<AuthSuccessEventArgs> AuthSuccess
    {
        add { _authTokenRetriever.AuthSuccess += value; }
        remove { _authTokenRetriever.AuthSuccess -= value; }
    }

    public void AwaitCallback() => _authTokenRetriever.AwaitCallback();
    public void StopListening() => _authTokenRetriever.StopListening();
    public string BrowserPath => _browserPath;
    public string AuthURL => _authTokenRetriever.AuthURL();
}

public class PostWrapper : IPost
{
    private readonly Post _post;

    public PostWrapper(Post post)
    {
        _post = post;
    }

    public string Id => _post.Id;
    public string Title => _post.Title;
    public string Author => _post.Author;
    public DateTime Created => _post.Created;
    public int UpVotes => _post.UpVotes; 
    public int DownVotes => _post.DownVotes;
    public string Permalink => _post.Permalink;
    public string Subreddit => _post.Subreddit;

    public void Upvote() => _post.Upvote();
    public void Downvote() => _post.Downvote();
    public void Hide() => _post.Hide();
}