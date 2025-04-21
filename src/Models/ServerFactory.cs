using RevitDoom.Contracts;
using RevitDoom.Enums;
using System;
using System.Collections.Generic;

namespace RevitDoom.Models
{
    public sealed class ServerFactory : IServerFactory
    {
        private readonly QualityConverter _qualityConverter;
        public ServerFactory(QualityConverter qualityConverter)
        {
            _qualityConverter = qualityConverter;
        }

        private static readonly Dictionary<Type,
            Func<int, int, double, CastomDirectContextServer>> _registry
            = new()
            {
                [typeof(FlatFaceServer)] = (w, h, c) => new FlatFaceServer(w, h, c),
            };

        public CastomDirectContextServer Create<TServer>(Quality quality)
            where TServer : CastomDirectContextServer
        {
            _qualityConverter.Convert(quality,
                out var height,
                out var width,
                out var cellSize);

            if (_registry.TryGetValue(typeof(TServer), out var ctor))
                return ctor(width, height, cellSize);

            return null;
        }
    }
}
