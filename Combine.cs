// ********************************************************************
// * 
// * File   : Combine.cs
// * Author : Mathilda Nynäs <mathilda.nynas@gmail.com>
// *         GitHub <noreply@github.com>
// *         Majon1 <mathilda.nynas@gmail.com>
// *
// * Copyright (C) (2023) Centria University of Applied Sciences.
// * All rights reserved.
// *
// * Unauthorized copying of this file, via any medium is strictly
// * prohibited.
// *
// ********************************************************************

using System.Globalization;
using LibGit2Sharp;

namespace Scanner
{
    public class Combine
    {
        public string Name { get; set; }
        public string newName;
        public List<Signature> ListAuthors = new();
        public List<DateTimeOffset> ListDates = new();
        public List<string> ListSha = new();
        public List<string> head = new();
        public int firstYear;
        public int lastYear;
        public bool complete,
            filename,
            author,
            year;

        public Combine(string name)
        {
            this.Name = name;
            this.firstYear = 0;
            this.lastYear = 0;
            this.newName = "";
            this.complete = false;
            this.filename = false;
            this.author = false;
            this.year = false;
        }

        public void AddAuthors(List<Signature> authors)
        {
            ListAuthors.Add(authors[0]);

            IEnumerable<Signature> authorsToAdd = authors.Where(
                a => ListAuthors.Find(la => la.Name == a.Name) == null
            );
            ListAuthors.AddRange(authorsToAdd);
        }

        public void AddDates(List<DateTimeOffset> dates)
        {
            ListDates.Add(dates[0]);
            foreach (DateTimeOffset s in ListDates)
            {
                foreach (DateTimeOffset date in dates)
                {
                    if (s.Year == date.Year)
                    {
                        continue;
                    }
                    else
                    {
                        ListDates.Add(date);
                    }
                }
            }
        }

        public void AddSha(List<string> sha)
        {
            foreach (string s in sha)
            {
                if (ListSha.Contains(s))
                {
                    continue;
                }
                else
                {
                    ListSha.Add(s);
                }
            }
        }

        public void ReadThisFile(string path)
        {
            string filed = Name.Substring(Name.LastIndexOf("\\") + 1);
            if (path.Contains("gitrepo"))
            {
                Name = path.Substring(0, path.LastIndexOf("gitrepo")) + filed;
            }
            if (Name.Contains('/'))
            {
                int index = Name.LastIndexOf("/");
                string file = Name.Substring(index + 1);
                newName = file;
            }
            else
            {
                newName = Name;
            }
            List<int> d = ListDates.Select(a => a.Year).ToList();
            var DistinctDates = d.GroupBy(x => x).Select(y => y.First());
            foreach (int date in DistinctDates)
            {
                if (firstYear == 0)
                {
                    firstYear = date;
                }
                if (date > lastYear)
                {
                    lastYear = date;
                }
                if (date < firstYear)
                {
                    firstYear = date;
                }
            }
            string[] lines = File.ReadAllLines(Name);
            int amount = 0;
            foreach (string l in lines)
            {
                if (l.Contains(newName[(newName.LastIndexOf('\\') + 1)..]))
                {
                    filename = true;
                }
                foreach (Signature name in ListAuthors)
                {
                    if (l.Contains(name.Name) && l.Contains(name.Email))
                    {
                        amount++;
                    }
                    if (amount == ListAuthors.Count)
                    {
                        author = true;
                    }
                }
                if (ListDates.Count > 1)
                {
                    if (l.Contains(firstYear + "-" + lastYear))
                    {
                        year = true;
                    }
                }
                else if (l.Contains("(" + lastYear + ")"))
                {
                    year = true;
                }
            }
            if (filename && author && year)
            {
                complete = true;
            }
        }

        public void SendToTemplate(string path)
        {
            string pathNow = System.IO.Directory.GetCurrentDirectory();
            if (
                Name.EndsWith(".cpp")
                || Name.EndsWith(".h")
                || Name.EndsWith(".c")
                || Name.EndsWith(".cs")
                || Name.EndsWith(".hpp")
                || Name.EndsWith(".java")
                || Name.EndsWith(".js")
                || Name.EndsWith(".rs")
                || Name.EndsWith(".ts")
            )
            {
                string templatePath = pathNow + "\\templates\\cs.template.txt";
                WhereTo(path, templatePath);
            }
            else if (Name.EndsWith(".py"))
            {
                string templatePath = pathNow + "\\templates\\py.template.txt";
                WhereTo(path, templatePath);
            }
            else if (Name.EndsWith(".html"))
            {
                string templatePath = pathNow + "\\templates\\html.template.txt";
                WhereTo(path, templatePath);
            }
            else if (Name.EndsWith(".css"))
            {
                string templatePath = pathNow + "\\templates\\css.template.txt";
                WhereTo(path, templatePath);
            }
            else if (Name.EndsWith(".php"))
            {
                string templatePath = pathNow + "\\templates\\php.template.txt";
                WhereTo(path, templatePath);
            }
        }

        public void WhereTo(string path, string templatePath)
        {
            if (path.Contains("gitrepo"))
            {
                path = path.Substring(0, path.LastIndexOf("gitrepo") - 1);
                AddTemplate(path, templatePath);
            }
            else
            {
                AddTemplate(path, templatePath);
            }
        }

        public void AddTemplate(string path, string templatePath)
        {
            Console.WriteLine("H");
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] files = d.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                string nameOnlyFile = Name.Substring(Name.LastIndexOf("\\") + 1);
                if (file.FullName.EndsWith(nameOnlyFile)) //get name only file name
                {
                    Name = file.FullName;
                }
            }
            if (!complete)
            {
                Console.WriteLine("e");
                string tempfile = Path.GetTempFileName();
                string finalTemp = templatePath;
                string[] cTemplate = File.ReadAllLines(finalTemp);
                List<string> replaced = cTemplate.ToList();
                List<string> newTemp = new List<string>();
                string comment = "";
                if (Name.EndsWith(".py"))
                {
                    comment = "#*";
                }
                if (Name.EndsWith(".html"))
                {
                    comment = "*";
                }
                else
                {
                    comment = "// *";
                }

                for (int i = 0; i < replaced.Count; i++)
                {
                    if (replaced[i].Contains("{filename}"))
                    {
                        replaced[i] = replaced[i].Replace(
                            "{filename}",
                            newName.Substring(newName.LastIndexOf('\\') + 1)
                        );
                        for (int s = 0; s <= i; s++)
                        {
                            newTemp.Add(replaced[s]);
                        }
                    }

                    if (replaced[i].Contains("{author}")) //ISSUE IS HERE!!!!
                    {
                        newTemp.Add(comment + " Author : " + ListAuthors[0].ToString());
                        if (ListAuthors.Count > 1)
                        {
                            for (int j = 1; j < ListAuthors.Count; j++)
                            {
                                newTemp.Add(comment + "         " + ListAuthors[j].ToString());
                            }
                        }
                        newTemp.Add(comment);
                    }
                    if (replaced[i].Contains("{years}"))
                    {
                        if (firstYear == lastYear)
                        {
                            newTemp.Add(replaced[i].Replace("{years}", lastYear.ToString()));
                            for (int s = i + 1; s < replaced.Count; s++)
                            {
                                newTemp.Add(replaced[s]);
                            }
                        }
                        else
                        {
                            newTemp.Add(
                                replaced[i].Replace(
                                    "{years}",
                                    firstYear.ToString() + "-" + lastYear.ToString()
                                )
                            );
                        }
                    }
                }

                Console.WriteLine("now");
                string[] lines = File.ReadAllLines(Name);
                using (StreamWriter writer = new(tempfile))
                using (StreamReader reader = new(Name))
                {
                    int a = 0;
                    if (Name.EndsWith(".py"))
                    {
                        foreach (string line in lines)
                        {
                            if (line.Contains("#"))
                            {
                                a++;
                            }
                            if (line == "")
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (Name.EndsWith(".php"))
                    {
                        foreach (string line in lines)
                        {
                            if (line == "")
                            {
                                continue;
                            }
                            if (line.Contains("<?") && line.Contains("?>"))
                            {
                                a++;
                            }
                            if (line.Contains("<?") && !line.Contains("?>"))
                            {
                                foreach (string l in lines)
                                {
                                    if (l.Contains("?>"))
                                    {
                                        break;
                                    }
                                    a++;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (Name.EndsWith(".html"))
                    {
                        foreach (string line in lines)
                        {
                            if (line == "")
                            {
                                continue;
                            }
                            if (line.Contains("<!--") && line.Contains("-->"))
                            {
                                a++;
                            }
                            if (line.Contains("<!--") && !line.Contains("-->"))
                            {
                                foreach (string l in lines)
                                {
                                    if (l.Contains("-->"))
                                    {
                                        break;
                                    }
                                    a++;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (string line in lines)
                        {
                            if (line.Contains("//"))
                            {
                                a++;
                            }
                            else if (line == "")
                            {
                                continue;
                            }
                            else if (line.Contains("/*"))
                            {
                                foreach (string l in lines)
                                {
                                    if (l.Contains("*/"))
                                    {
                                        break;
                                    }
                                    a++;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    Console.WriteLine("a is " + a);

                    foreach (string l in newTemp)
                    {
                        writer.WriteLine(l);
                    }
                    Console.WriteLine("a equals " + a);

                    for (int i = a; i < lines.Count(); i++)
                    {
                        writer.WriteLine(lines[i]);
                    }
                }
                File.Copy(tempfile, Name, true);
                File.Delete(tempfile);
            }
        }
    }
}
