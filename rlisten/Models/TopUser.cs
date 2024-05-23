using System;
namespace rlisten.Models
{
	public class TopUser
	{
		public string SubReddit { get; set; }
		public string User { get; set; }
		public int PostCount { get; set; }
		public DateTime TimeStamp { get; set; }

		public TopUser(KeyValuePair<string, int> ranking, string subreddit)
		{
			User = ranking.Key;
			PostCount = ranking.Value;
			SubReddit = subreddit;
			TimeStamp = DateTime.Now;
        }

		public TopUser(string subreddit)
		{
			SubReddit = subreddit;
			User = "No Posts Yet";
			PostCount = 0;
		}
	}
}

