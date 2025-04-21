using System;

namespace RevitDoom.Models
{
    public class DoomAppOptions
    {
        public string IwadPath { get; set; } = "";
        public bool HighResolution { get; set; }
        public string[] ExtraArgs { get; set; } = Array.Empty<string>();
    }
}
