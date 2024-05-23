using rlisten.Wrappers;

public class MockPost : IPost
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public DateTime Created { get; set; }
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public string Permalink { get; set; }
    public string Subreddit { get; set; }

    public MockPost(string author, int upvotes)
    {
        Author = author;
        UpVotes = upvotes;
    }

    public void Downvote() { }
    public void Hide() { }
    public void Upvote() { }
}