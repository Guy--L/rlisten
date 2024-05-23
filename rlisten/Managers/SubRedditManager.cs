using rlisten.Models;
using rlisten.Services;
using rlisten.Wrappers;
using Reddit.Controllers;

namespace rlisten.Managers
{
    public interface ISubRedditManager: IDisposable
    {
        static Action<Post> TopPostChanged;
        static Action<TopUser> TopUserChanged;
        int Count { get; }
         
        string Add(string name);
        List<string> Menu(bool withTime = true);
        string Remove(string name);

        void Dispose();
    }

    public class SubRedditManager : ISubRedditManager, IDisposable
    {
        private readonly Dictionary<string, ISubRedditStatService> monitors = new Dictionary<string, ISubRedditStatService>();
        private readonly SubRedditStatServiceFactory _subRedditStatServiceFactory;
        private readonly IRedditService _reddit;

        public static Action<IPost> TopPostChanged;
        public static Action<TopUser> TopUserChanged;

        public int Count => monitors.Count;

        public SubRedditManager(IRedditService redditService, SubRedditStatServiceFactory subRedditStatServiceFactory)
        {
            _reddit = redditService;
            _subRedditStatServiceFactory = subRedditStatServiceFactory;
        }

        public string Add(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            var exists = monitors.TryGetValue(name, out var subreddit);
            if (exists)
                return $"/r/{name} is already being monitored.";

            ISubRedditStatService monitor;
            try
            {
                monitor = _subRedditStatServiceFactory(_reddit, name);
                monitors.Add(name, monitor);
            }
            catch (Exception e)
            {
                return $"Failed to monitor /r/{name} because of error: {e.Message}.";
            }
            monitor.TopUserChanged += TopUserChanged;
            monitor.TopPostChanged += TopPostChanged;
            monitor.SubscribeToChanges();

            return $"Started monitoring /r/{name}.";
        }

        public string Remove(string name)
        {
            ISubRedditStatService monitor;
            if (string.IsNullOrEmpty(name))
                return "";

            if (int.TryParse(name, out int choice) && choice < monitors.Count)
            {
                var entry = monitors.ElementAt(choice);
                monitor = entry.Value;
                name = entry.Key;
            }
            else if (!monitors.TryGetValue(name, out monitor))
                return $"/r/{name} is not being monitored.";

            monitor.TopUserChanged -= TopUserChanged;
            monitor.TopPostChanged -= TopPostChanged;
            monitor.Dispose();
            monitors.Remove(name);
            return $"Stopped monitoring /r/{name}.";
        }

        public List<string> Menu(bool withTime = true)
        {
            if (withTime)
            {
                var now = DateTime.Now;
                return monitors.Select((m, i) => $"{i} - {m.Key} ({(int)(now - m.Value.started).TotalSeconds}s)").ToList();
            }

            return monitors.Select(m => $"{m.Key}").ToList();
        }

        public void Dispose()
        {
            monitors.Values.ToList().ForEach(m =>
            {
                m.TopUserChanged -= TopUserChanged;
                m.TopPostChanged -= TopPostChanged;
                m.Dispose();
            });
            monitors.Clear();
        }
    }
}
