using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Model
{
    public abstract class PerfTool
    {
        public string name { get; set; }
        public string version { get; set; }
        public PerfTool(string Name, string Version)
        {
            this.name = Name;
            this.version = Version;
        }
    }

    public class Tool : PerfTool
    {
        public Tool(string Name, string Version) : base(Name, Version)
        {

        }
    }
}
