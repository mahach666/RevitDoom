using RevitDoom.Contracts;
using RevitDoom.Enums;
using System;

namespace RevitDoom.Models
{
    public class DirectContextService : IDirectContextController, IDisposable
    {
        private CastomDirectContextServer _activeServer { get; set; }
        private readonly IRevitDirectServerController _revitService;
        private readonly IServerFactory _serverFactory;
        private Quality CurrentQuality { get;  set; }

        public DirectContextService(IServerFactory serverFactory,
            IRevitDirectServerController revitService)
        {
            _revitService = revitService;
            _serverFactory = serverFactory;
        }

        public void RegisterServer<T>(Quality quality)
            where T : CastomDirectContextServer
        {
            UnregisterAllServers();

            try
            {
                _activeServer = _serverFactory.Create<T>(quality);
                _revitService.RegisterServer(_activeServer);

                CurrentQuality = quality;
            }
            catch
            {
                UnregisterAllServers();
                CurrentQuality = Quality.Invalid;
            }
        }

        public void UnregisterAllServers()
        {
            _revitService.UnregisterAllServers();
            _activeServer = null;
        }

        public void Update(byte[] buffer, int width, int height)
        {
            _activeServer.SetPixels(buffer, width, height);
        }

        public bool IsValid(Quality quality) => _activeServer != null && quality == CurrentQuality;

        public void Dispose() => UnregisterAllServers();
    }
}
