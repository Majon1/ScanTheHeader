//********************************************************************
// * 
// * File   : File.cs
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
