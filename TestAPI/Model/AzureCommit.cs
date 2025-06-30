namespace TestAPI.Model
{
    public sealed class AzureCommit
    {
        public int Count { get; set; }
        public List<GitCommit> Value { get; set; } = [];
    }

    public sealed class GitCommit
    {
        public string CommitId { get; set; }
        public GitUser Author { get; set; }
        public GitUser Committer { get; set; }
        public string Comment { get; set; }
        public bool CommentTruncated { get; set; }
        public ChangeCounts ChangeCounts { get; set; }
        public string Url { get; set; }
        public string RemoteUrl { get; set; }
    }

    public sealed class GitUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
    }

    public sealed class ChangeCounts
    {
        public int Add { get; set; }
        public int Edit { get; set; }
        public int Delete { get; set; }
    }
}
