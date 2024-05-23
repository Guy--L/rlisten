using Reddit;
using Reddit.Things;
using rlisten.Managers;
using rlisten.Models;
using rlisten.Wrappers;

namespace rlisten.Services
{
    public interface IRedditService
    {
        IRedditClient GetRedditClient();
        static string Status { get; }
    }

    public class RedditService : IRedditService
    {
        private readonly IRedditClient redditClient;
        private readonly IAuthService authService;
        private RedditOAuthTokens redditOAuthTokens;

        public static string Status { private set; get; }

        public RedditService(ISettingsManager settingsManager, IRedditClient redditClient, IAuthService authService)
        {
            this.redditClient = redditClient;
            this.authService = authService;
            redditOAuthTokens = settingsManager.Read<RedditOAuthTokens>();

            InitializeAsync().Wait();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await redditClient.InitializeClientAsync(redditOAuthTokens.AccessToken);
                var hotPosts = redditClient.Subreddit("all").Posts.GetHot();
                Status = "Initialized";
            }
            catch (Exception)
            {
                try
                {
                    await redditClient.InitializeClientWithRefreshTokenAsync(redditOAuthTokens.RefreshToken);
                    var hotPosts = redditClient.Subreddit("all").Posts.GetHot();
                    Status = "Refreshed";
                }
                catch (Exception)
                {
                    try
                    {
                        redditOAuthTokens.AccessToken = await authService.ReauthorizeAsync();
                        await redditClient.InitializeClientAsync(redditOAuthTokens.AccessToken);
                        var hotPosts = redditClient.Subreddit("all").Posts.GetHot();
                        Status = "Reauthorized";
                    }
                    catch (Exception ex)
                    {
                        Status = ex.Message;
                        throw;
                    }
                }
            }
        }

        public IRedditClient GetRedditClient()
        {
            return redditClient;
        }
    }
}
