﻿using System;

namespace Program
{
    public class MenuItem
    {
        public string FileType { get; set; }
        public string Title { get; set; }
        public string Target { get; set; }
        public Guid ComServer { get; set; }
    };
}
