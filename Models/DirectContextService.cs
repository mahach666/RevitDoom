using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using System;

namespace RevitDoom.Models
{
    public class DirectContextService : IDirectContextController, IDisposable
    {
        public IDirectContext3DServer ActiveServer { get; private set; }

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
            where T : IDirectContext3DServer
        {
            UnregisterAllServers();

            ActiveServer = _serverFactory.Create<T>(quality);

            _revitService.RegisterServer(ActiveServer);
        }

        public void UnregisterAllServers() => _revitService.UnregisterAllServers();

        public void Dispose() => UnregisterAllServers();
    }
}
