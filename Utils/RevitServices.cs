using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RevitDoom.Utils
{
    internal static class RevitServices
    {
        public static void RegisterServer(IDirectContext3DServer revitServer, UIDocument uidoc, HashSet<Document> documentList)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;
            IList<Guid> activeServerIds = multiServerService.GetActiveServerIds();
            service.AddServer(revitServer);
            activeServerIds.Add(revitServer.GetServerId());
            multiServerService.SetActiveServers(activeServerIds);
            if (!documentList.Contains(uidoc.Document))
            {
                documentList.Add(uidoc.Document);
            }

            uidoc.UpdateAllOpenViews();
        }

        public static void RegisterMultiServer(List<SolidServer> serverList, UIDocument uidoc, HashSet<Document> documentList)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;
            IList<Guid> activeServerIds = multiServerService.GetActiveServerIds();
            foreach (var revitServer in serverList)
            {
                service.AddServer(revitServer);
                activeServerIds.Add(revitServer.GetServerId());
            }
            multiServerService.SetActiveServers(activeServerIds);
            if (!documentList.Contains(uidoc.Document))
            {
                documentList.Add(uidoc.Document);
            }

            uidoc.UpdateAllOpenViews();
        }

        //public static void UnregisterServers<T>(HashSet<Document> documentList, List<T> serverList) where T : IDirectContext3DServer
        //{
        //    if (!(ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService) is MultiServerService multiServerService))
        //    {
        //        return;
        //    }

        //    foreach (Guid registeredServerId in multiServerService.GetRegisteredServerIds())
        //    {
        //        if (multiServerService.GetServer(registeredServerId) is T)
        //        {
        //            multiServerService.RemoveServer(registeredServerId);
        //        }
        //    }

        //    serverList.Clear();
        //    foreach (Document document in documentList)
        //    {
        //        if (document.IsValidObject)
        //        {
        //            new UIDocument(document).UpdateAllOpenViews();
        //        }
        //    }

        //    documentList.Clear();
        //}

        public static void UnregisterAllServers(HashSet<Document> documentList)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;

            //var a = new List<IExternalServer>();

            foreach (Guid registeredServerId in multiServerService.GetRegisteredServerIds())
            {
                if (multiServerService.GetServer(registeredServerId) is IDirectContext3DServer sr)
                {
                    //a.Add(sr);
                    multiServerService.RemoveServer(registeredServerId);
                }
            }

            foreach (Document document in documentList)
            {
                if (document.IsValidObject)
                {
                    new UIDocument(document).UpdateAllOpenViews();
                }
            }
        }
    }
}
