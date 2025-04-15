using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using RevitDoom.Video;
using System;
using System.Collections.Generic;

namespace RevitDoom
{
    [Transaction(TransactionMode.Manual)]
    public class DoomExCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uidoc = uiApp.ActiveUIDocument;
            var doc = uidoc.Document;


            var serverList = new List<SolidServer>();

            var boxSize = 1;
            var acumY = 0;
            var acumX = 0;

            for (int y = 0; y < 320; y++)
            {
                for (int x = 0; x < 1; x++)
                {
                    var solid = CreateCube(new XYZ(acumX, acumY, 0), boxSize);

                    var server = new SolidServer(uidoc, solid, true, new ColorWithTransparency(255, 165, 0, 0), new ColorWithTransparency(255, 255, 255, 0));

                    serverList.Add(server);

                    server.RenderScene(doc.ActiveView, DisplayStyle.Wireframe);

                    acumX += boxSize; 
                }
                acumY += boxSize;
                acumX = 0;
            }
            uidoc.UpdateAllOpenViews();

            //RegisterMultiServer(serverList, uidoc, new HashSet<Document>() { doc });

            //UnregisterAllServers(new HashSet<Document>() { doc });


            //var wadPath = UserSelect.GetWad();

            //if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;


            //var pixels = new List<XYZ>(320 * 200);

            //for (int y = 0; y < 200; y++)
            //{
            //    for (int x = 0; x < 320; x++)
            //    {
            //        pixels.Add(new XYZ(x, y, 0));
            //    }
            //}


            //var builder = new AppBuilder();
            //builder.SetIwad(wadPath)
            //    .EnableHighResolution(false)
            //    .WithArgs("-skill", "3")
            //    .WithScale(1)
            //    .WithPixels(pixels)
            //    .WithDocument(doc)
            //    .WithUIDocument(uidoc);

            //var dapp = builder.Build();
            //dapp.Run();


            return Result.Succeeded;
        }


        public static Solid CreateCube(XYZ center, double cubeSize = 5.0)
        {
            double num = cubeSize / 2.0;
            XYZ xYZ = new XYZ(center.X + num, center.Y + num, center.Z - num);
            XYZ xYZ2 = new XYZ(center.X - num, center.Y + num, center.Z - num);
            XYZ xYZ3 = new XYZ(center.X - num, center.Y - num, center.Z - num);
            XYZ xYZ4 = new XYZ(center.X + num, center.Y - num, center.Z - num);
            Curve curve = Line.CreateBound(xYZ, xYZ2);
            Curve curve2 = Line.CreateBound(xYZ2, xYZ3);
            Curve curve3 = Line.CreateBound(xYZ3, xYZ4);
            Curve curve4 = Line.CreateBound(xYZ4, xYZ);
            CurveLoop curveLoop = new CurveLoop();
            curveLoop.Append(curve);
            curveLoop.Append(curve2);
            curveLoop.Append(curve3);
            curveLoop.Append(curve4);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, new XYZ(0.0, 0.0, 1.0), cubeSize);
        }




        private static List<SolidServer> solidServers = new List<SolidServer>();

        public static void RegisterServer(SolidServer revitServer, UIDocument uidoc, HashSet<Document> documentList)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;
            IList<Guid> activeServerIds = multiServerService.GetActiveServerIds();
            service.AddServer(revitServer);
            //solidServers.Add(revitServer);
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

        public static void UnregisterServers<T>(HashSet<Document> documentList, List<T> serverList) where T : IDirectContext3DServer
        {
            if (!(ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService) is MultiServerService multiServerService))
            {
                return;
            }

            foreach (Guid registeredServerId in multiServerService.GetRegisteredServerIds())
            {
                if (multiServerService.GetServer(registeredServerId) is T)
                {
                    multiServerService.RemoveServer(registeredServerId);
                }
            }

            serverList.Clear();
            foreach (Document document in documentList)
            {
                if (document.IsValidObject)
                {
                    new UIDocument(document).UpdateAllOpenViews();
                }
            }

            documentList.Clear();
        }

        public static void UnregisterAllServers(HashSet<Document> documentList)
        {
            ExternalService service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            MultiServerService multiServerService = service as MultiServerService;

            foreach (Guid registeredServerId in multiServerService.GetRegisteredServerIds())
            {
                if (multiServerService.GetServer(registeredServerId) is IDirectContext3DServer)
                {
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
