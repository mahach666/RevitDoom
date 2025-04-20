using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RevitDoom.Dooms
{
    public class DoomAppBuilder
    {
        private string _iwadPath = "DOOM1.WAD";
        private bool _highResolution = false;
        private string[] _extraArgs = Array.Empty<string>();
        private uint _scale = 1;
        private UIDocument _uidoc = null;
        private IList<XYZ> _pixels;
        private Reference _referenceObj;
        private Face _faceObj;

        public DoomAppBuilder SetIwad(string path)
        {
            _iwadPath = path;
            return this;
        }

        public DoomAppBuilder EnableHighResolution(bool enable = true)
        {
            _highResolution = enable;
            return this;
        }

        public DoomAppBuilder WithArgs(params string[] args)
        {
            _extraArgs = args;
            return this;
        }
        public DoomAppBuilder WithScale(uint scale)
        {
            _scale = scale;
            return this;
        }

        public DoomAppBuilder WithPixels(IList<XYZ> pixels)
        {
            _pixels = pixels;
            return this;
        }

        public DoomAppBuilder WithUIDocument(UIDocument uIDocument)
        {
            _uidoc = uIDocument;
            return this;
        }

        public DoomAppBuilder WithReferenceObj(Reference referenceObj)
        {
            _referenceObj = referenceObj;
            return this;
        }

        public DoomAppBuilder WithFaceObj(Face faceObj)
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

