using rlisten.Services;

namespace rlisten.Models
{
    public delegate ISubRedditStatService SubRedditStatServiceFactory(IRedditService redditService, string subredditName);
}