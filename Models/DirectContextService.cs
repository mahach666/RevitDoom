using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using System;

namespace RevitDoom.Models
{
    public class DirectContextService : IDirectContextController, IDisposable
    {
        private CastomDirectContextServer _activeServer { get;  set; }

        private readonly UIApplication _uIApplication;
        private readonly UIDocument _uIDocument;
        private readonly RevitService _revitService;
        private readonly IServerFactory _serverFactory;

        public DirectContextService(UIApplication uIApplication,
            ServerFactory serverFactory,
            RevitService revitService)
        {
            _uIApplication = uIApplication;
            _uIDocument = uIApplication.ActiveUIDocument;
            _revitService = revitService;
            _serverFactory = serverFactory;
        }

        public void RegisterServer<T>(Quality quality) 
            where T : CastomDirectContextServer
        {
            UnregisterAllServers();

            _activeServer = _serverFactory.Create<T>(quality);

            _revitService.RegisterServer(_activeServer);
        }

        public void UnregisterAllServers() => _revitService.UnregisterAllServers();

        public void Update(byte[] buffer, int width, int height)
        {
            _activeServer.SetPixels(buffer, width, height);
        }
        public void Dispose() => UnregisterAllServers();

    }
}
