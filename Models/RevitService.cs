using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RevitDoom.Models
{
    internal class RevitService
    {
        private static UIApplication _uIApplication;

        public RevitService(UIApplication uIApplication)
        {
            _uIApplication = uIApplication;
        }

        public void RegisterServer(IDirectContext3DServer revitServer)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;
            IList<Guid> activeServerIds = multiServerService.GetActiveServerIds();
            service.AddServer(revitServer);
            activeServerIds.Add(revitServer.GetServerId());
            multiServerService.SetActiveServers(activeServerIds);
        }

        public void RegisterMultiServer(List<IDirectContext3DServer> serverList)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;
            IList<Guid> activeServerIds = multiServerService.GetActiveServerIds();
            foreach (var revitServer in serverList)
            {
                service.AddServer(revitServer);
                activeServerIds.Add(revitServer.GetServerId());
            }
        }

        public void UnregisterAllServers()
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;

            foreach (Guid registeredServerId in multiServerService.GetRegisteredServerIds())
            {
                if (multiServerService.GetServer(registeredServerId) is IDirectContext3DServer sr)
                {
                    multiServerService.RemoveServer(registeredServerId);
                }
            }

            foreach (Document document in _uIApplication.Application.Documents)
            {
                if (document.IsValidObject)
                {
                    new UIDocument(document).UpdateAllOpenViews();
                }
            }
        }
    }
}
