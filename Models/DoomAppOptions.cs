using System;

namespace RevitDoom.Models
{
    public class DoomAppOptions
    {
        public string IwadPath { get; init; } = "";
        public bool HighResolution { get; init; }
        public string[] ExtraArgs { get; init; } = Array.Empty<string>();
    }
}
