using System;
using System.Diagnostics;
using Reddit;
using Reddit.AuthTokenRetriever;
using Reddit.AuthTokenRetriever.EventArgs;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

namespace rlisten.Wrappers;

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
