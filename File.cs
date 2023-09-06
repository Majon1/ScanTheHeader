using LibGit2Sharp;

namespace Scanner
{
    public class Files
    {
        public string filePath { get; }
        public StatusEntry se { get; }
        public FileStatus state { get; }

        public Files(string filePath, StatusEntry se, FileStatus state)
        {
            this.filePath = filePath;
            this.se = se;
            this.state = state;
        }
    }
}