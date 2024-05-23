using System;
namespace rlisten.Models
{
    public class SubredditList
    {
        public List<string> Subreddits { get; set; }

        public SubredditList()
        {
            Subreddits = new List<string>();
        }

        public SubredditList(List<string> names)
        {
            Subreddits = names;
        }
    }
}

