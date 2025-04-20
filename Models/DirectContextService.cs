using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using System;

namespace RevitDoom.Models
{
    internal class DirectContextService :  IDirectContextController, IDisposable
    {
        public IDirectContext3DServer ActiveServer { get; private set; }

        private readonly UIApplication _uIApplication;
        private readonly UIDocument _uIDocument;
        private readonly RevitService _revitService;
        private readonly ServerFactory _serverFactory;

        DirectContextService(UIApplication uIApplication,
            ServerFactory serverFactory,
            RevitService revitService)
        {
            _uIApplication = uIApplication;
            _uIDocument = uIApplication.ActiveUIDocument;
            _revitService = revitService;
            _serverFactory = serverFactory;

            RegisterServer<FlatFaceServer>(Quality.Medium);
        }

        public void RegisterServer<T>(Quality quality) where T : IDirectContext3DServer
        {

            _revitService.UnregisterAllServers();

            ActiveServer = _serverFactory.Create<T>(quality);

            _revitService.RegisterServer(ActiveServer);
        }

        public void UnregisterAllServers()
        {
            _revitService.UnregisterAllServers();
        }

        public void Dispose()
        {
            _revitService.UnregisterAllServers();
        }
    }
}
