using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDoomNetPort;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDoom
{
    public class AppBuilder
    {
        private string _iwadPath = "DOOM1.WAD";
        private bool _highResolution = false;
        private string[] _extraArgs = Array.Empty<string>();
        private uint _scale = 1;
        private Document _doc = null;
        private UIDocument _uidoc = null;
        private IList<FilledRegion> _pixels;

        public AppBuilder SetIwad(string path)
        {
            _iwadPath = path;
            return this;
        }

        public AppBuilder EnableHighResolution(bool enable = true)
        {
            _highResolution = enable;
            return this;
        }

        public AppBuilder WithArgs(params string[] args)
        {
            _extraArgs = args;
            return this;
        }
        public AppBuilder WithScale(uint scale)
        {
            _scale = scale;
            return this;
        }

        public AppBuilder WithPixels(IList<FilledRegion> pixels)
        {
            _pixels = pixels;
            _doc = _pixels.FirstOrDefault()?.Document;
            return this;
        }

        public AppBuilder WithUIDocument(UIDocument uIDocument)
        {
            _uidoc = uIDocument;
            return this;
        }

        public DoomApp Build()
        {
            return new DoomApp()
            {
                IwadPath = _iwadPath,
                HighResolution = _highResolution,
                ExtraArgs = _extraArgs,
                Scale = _scale,
                Doc = _doc,
                Uidoc = _uidoc,
                Pixels = _pixels
            };
        }
    }
}

