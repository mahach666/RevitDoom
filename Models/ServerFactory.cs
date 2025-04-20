using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using System;
using System.Collections.Generic;

namespace RevitDoom.Models
{
    public sealed class ServerFactory : IServerFactory
    {
        private readonly UIApplication _uiApplication;
        private readonly QualityConverter _qualityConverter;
        public ServerFactory(UIApplication uIApplication,
            QualityConverter qualityConverter)
        {
            _uiApplication = uIApplication;
            _qualityConverter = qualityConverter;
        }

        private static readonly Dictionary<Type,
            Func<UIDocument, int, int, double, CastomDirectContextServer>> _registry
            = new()
            {
                [typeof(FlatFaceServer)] = (d, w, h, c) => new FlatFaceServer(d, w, h, c),
            };

        public CastomDirectContextServer Create<TServer>(Quality quality)
            where TServer : CastomDirectContextServer
        {
            _qualityConverter.Convert(quality,
                out var height,
                out var width,
                out var cellSize);

            if (_registry.TryGetValue(typeof(TServer), out var ctor))
                return ctor(_uiApplication.ActiveUIDocument, width, height, cellSize);

            return null;
        }
    }

}
