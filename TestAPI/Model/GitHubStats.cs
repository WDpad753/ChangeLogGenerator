namespace TestAPI.Model
{
    public class GitHubStats
    {
        public string url { get; set; } = default!;
        public string sha { get; set; } = default!;
        public string node_id { get; set; } = default!;
        public string html_url { get; set; } = default!;
        public string comments_url { get; set; } = default!;
        public GithubCommit commit { get; set; } = default!;
        public GitHubUser? author { get; set; }
        public GitHubUser? committer { get; set; }
        public GitParent[] parents { get; set; } = Array.Empty<GitParent>();
        public Stats? stats { get; set; }
        public GitFile[] files { get; set; } = Array.Empty<GitFile>();
    }

    public class GithubCommit
    {
        public string url { get; set; } = default!;
        public CommitPerson author { get; set; } = default!;
        public CommitPerson committer { get; set; } = default!;
        public string message { get; set; } = default!;
        public GitTree tree { get; set; } = default!;
        public int comment_count { get; set; }
        public GitVerification verification { get; set; } = default!;
    }

    public class CommitPerson
    {
        public string name { get; set; } = default!;
        public string email { get; set; } = default!;
        public DateTime date { get; set; }
    }

    public class GitTree
    {
        public string url { get; set; } = default!;
        public string sha { get; set; } = default!;
    }

    public class GitVerification
    {
        public bool verified { get; set; }
        public string reason { get; set; } = default!;
        public object? signature { get; set; }
        public object? payload { get; set; }
        public object? verified_at { get; set; }
    }

    public class GitHubUser
    {
        public string login { get; set; } = default!;
        public int id { get; set; }
        public string node_id { get; set; } = default!;
        public string avatar_url { get; set; } = default!;
        public string gravatar_id { get; set; } = default!;
        public string url { get; set; } = default!;
        public string html_url { get; set; } = default!;
        public string followers_url { get; set; } = default!;
        public string following_url { get; set; } = default!;
        public string gists_url { get; set; } = default!;
        public string starred_url { get; set; } = default!;
        public string subscriptions_url { get; set; } = default!;
        public string organizations_url { get; set; } = default!;
        public string repos_url { get; set; } = default!;
        public string events_url { get; set; } = default!;
        public string received_events_url { get; set; } = default!;
        public string type { get; set; } = default!;
        public bool site_admin { get; set; }
    }

    public class GitParent
    {
        public string url { get; set; } = default!;
        public string sha { get; set; } = default!;
    }

    public class Stats
    {
        public int additions { get; set; }
        public int deletions { get; set; }
        public int total { get; set; }
    }

    public class GitFile
    {
        public string filename { get; set; } = default!;
        public int additions { get; set; }
        public int deletions { get; set; }
        public int changes { get; set; }
        public string status { get; set; } = default!;
        public string raw_url { get; set; } = default!;
        public string blob_url { get; set; } = default!;
        public string? patch { get; set; }
    }
}
