using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        private IList<XYZ> _pixels;
        private Reference _referenceObj;
        private Face _faceObj;

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

        public AppBuilder WithPixels(IList<XYZ> pixels)
        {
            _pixels = pixels;
            return this;
        }

        public AppBuilder WithUIDocument(UIDocument uIDocument)
        {
            _uidoc = uIDocument;
            return this;
        }

        public AppBuilder WithDocument(Document uIDocument)
        {
            _doc = uIDocument;
            return this;
        }
        public AppBuilder WithReferenceObj(Reference referenceObj)
        {
            _referenceObj = referenceObj;
            return this;
        }

        public AppBuilder WithFaceObj(Face faceObj)
        {
            _faceObj = faceObj;
            return this;
        }

        public DoomApp Build()
        {
            var app = new DoomApp()
            {
                IwadPath = _iwadPath,
                HighResolution = _highResolution,
                ExtraArgs = _extraArgs,
                Scale = _scale,
                Doc = _doc,
                Uidoc = _uidoc,
                Pixels = _pixels,
                ReferenceObj = _referenceObj,
                FaceObj = _faceObj
            };

            app.Initialize();
            return app;
        }
    }
}

