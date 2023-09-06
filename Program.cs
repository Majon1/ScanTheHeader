//********************************************************************
// * 
// * File   : Program.cs
// * Author : Mathilda Nynäs <mathilda.nynas@gmail.com>
// *
// * Copyright (C) (2023) Centria University of Applied Sciences.
// * All rights reserved.
// *
// * Unauthorized copying of this file, via any medium is strictly
// * prohibited.
// *
// ********************************************************************
//

using LibGit2Sharp;
using System.IO;

namespace Scanner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string r = "";
            string path;
            while (true)
            {
                Console.WriteLine("Git or Mercurial?");
                string? input = Console.ReadLine();
                if (input == "Git")
                {
                    break;
                }
                if (input == "Mercurial")
                {
                    r = "mercurial";
                    break;
                }
            }
            while (true)
            {
                Console.WriteLine("Give address to repository/directory:");
                path = Console.ReadLine()!;
                if (Repository.IsValid(path))
                {
                    break;
                }
                else if (Directory.Exists(path + "/.hg"))
                {
                    Console.WriteLine("hg found");
                    break;
                }
            }

            if (r == "mercurial")
            {
                string gitInit = "/C cd " + path + " && git init gitrepo";
                Console.WriteLine(gitInit);
                string hgBookMark = "/C cd " + path + " && hg bookmarks hg";
                Console.WriteLine(hgBookMark);
                string hgPush = "/C cd " + path + " && hg push gitrepo";
                Console.WriteLine(hgPush);
                string checkOut = "/C cd " + path + "/gitrepo" + " && git checkout hg";
                Console.WriteLine(checkOut);

                System.Diagnostics.Process.Start("CMD.exe", gitInit).WaitForExit();
                Console.WriteLine("1");
                System.Diagnostics.Process.Start("CMD.exe", hgBookMark).WaitForExit();
                Console.WriteLine("2");
                System.Diagnostics.Process.Start("CMD.exe", hgPush).WaitForExit();
                Console.WriteLine("3");
                System.Diagnostics.Process.Start("CMD.exe", checkOut).WaitForExit();
                Console.WriteLine("4");

                path = path + "/gitrepo";
            }
            Repository repo = new(path);

            List<Files> filelist = new();
            List<string> subs = new();
            List<Files> subFiles = new();
            List<string> affixes =
                new()
                {
                    ".cs",
                    ".c",
                    ".cpp",
                    ".h",
                    ".hpp",
                    ".py",
                    ".rs",
                    ".js",
                    ".java",
                    ".css",
                    ".html",
                    ".php",
                    ".ts"
                };
            foreach (
                StatusEntry item in repo.RetrieveStatus(
                    new StatusOptions() { IncludeUnaltered = true | false }
                )
            )
            { //iterates the final version of the repository
                foreach (string affix in affixes) // only save relevant files to filelist
                {
                    if (item.FilePath.EndsWith(affix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (item.FilePath.Contains('/'))
                        {
                            int index = item.FilePath.LastIndexOf("/");
                            string file = item.FilePath.Substring(index + 1);
                            subs.Add(file); //saves names of the file without /folder/folder/ etc
                            subFiles.Add(new Files(item.FilePath, item, item.State));
                            //subsFiles contains full names to List<string> subs filenames
                        }
                        else
                        {
                            filelist.Add(new Files(item.FilePath, item, item.State));
                            //if files are in main "folder" of repository
                        }
                    }
                }
            }
            List<string> names = new(); //adds names found in tree of the commit
            List<CommitInfo> commitInfo = new();
            CommitFilter? filter = new() { SortBy = CommitSortStrategies.Topological };
            IEnumerable<Commit> commits = repo.Commits.QueryBy(filter);
            foreach (Commit commit in commits)
            {
                Tree tree = commit.Tree; //get repository at said commit
                IterateTree(tree, names);
                CommitInfo a = new(commit.Committer, commit.Committer.When, commit.Sha);
                a.AddToNameList(names); //add all names in commit, both main and sub folder
                commitInfo.Add(a);
                names.Clear(); //clear list for next commit
            }
            List<Combine> combined = new(); // combine info from commitinfo and filelist
            CombineFiles(path, filelist, combined, subs, commitInfo);
            CombineFiles(path, subFiles, combined, subs, commitInfo);

            foreach (Combine c in combined)
            {
                c.ReadThisFile(path);
                if (!c.complete)
                {
                    c.SendToTemplate(path);
                }
            }
            if (r == "m")
            {
                string del =
                    "/C cd "
                    + path.Substring(0, path.LastIndexOf("/gitrepo"))
                    + " && rmdir /s /q gitrepo";
                System.Diagnostics.Process.Start("CMD.exe", del).WaitForExit();
            }
            Console.WriteLine("all done!");
        }

        //METHODS------------------------------------------------------------------------------------------------------
        public static void IterateTree(Tree tree, List<string> names)
        {
            foreach (TreeEntry item in tree)
            {
                names.Add(item.Name);
                if (item.TargetType == TreeEntryTargetType.Tree)
                {
                    Tree subTree = (Tree)item.Target;
                    IterateTree(subTree, names);
                }
            }
        }

        public static void CombineFiles(
            string path,
            List<Files> files,
            List<Combine> combined,
            List<string> subs,
            List<CommitInfo> commitInfo
        )
        {
            List<Signature> authors = new();
            List<DateTimeOffset> dates = new();
            List<string> sha = new();
            List<string> f = files.Select(file => file.filePath).ToList(); //get filename as string
            foreach (string name in f)
            {
                Combine h = new(Convert.ToString(path + "\\" + name));

                for (int j = 0; j < commitInfo.Count; j++)
                {
                    if (commitInfo[j].filenames.Contains(name))
                    {
                        authors.Add(commitInfo[j].Author);
                        dates.Add(commitInfo[j].Date);
                        sha.Add(commitInfo[j].Sha);
                    }
                    else if (name.Contains('/'))
                    {
                        foreach (string sub in subs)
                        {
                            if (commitInfo[j].filenames.Contains(sub))
                            {
                                authors.Add(commitInfo[j].Author);
                                dates.Add(commitInfo[j].Date);
                                sha.Add(commitInfo[j].Sha);
                            }
                        }
                    }
                }
                h.AddAuthors(authors);
                h.AddDates(dates);
                h.AddSha(sha);
                combined.Add(h);
                authors.Clear();
                dates.Clear();
                sha.Clear();
            }
        }
    }
}
