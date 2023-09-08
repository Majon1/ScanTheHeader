using LibGit2Sharp;

namespace Scanner
{
    public class CommitInfo
    {
        public List<string> filenames = new();
        public Signature Author { get; }
        public DateTimeOffset Date { get; }
        public string Sha { get; }

        public CommitInfo(Signature author, DateTimeOffset date, string sha)
        {
            this.Author = author;
            this.Date = date;
            this.Sha = sha;
        }

        public void AddToNameList(List<string> names)
        {
            foreach (string name in names)
            {
                if (filenames.Contains(name))
                {
                    continue;
                }
                else
                {
                    filenames.Add(name);
                }
            }
        }
    }
}
