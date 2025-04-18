using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDoom.Utils;
using RevitDoom.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static DoomNetFrameworkEngine.DoomEntity.World.StatusBar;
using Face = Autodesk.Revit.DB.Face;

namespace RevitDoom
{
    [Transaction(TransactionMode.Manual)]
    public class DoomExCommand : IExternalCommand
    {
        public static FlatFaceServer Server;

        static public UIApplication UiApp;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UiApp = commandData.Application;
            var uidoc = UiApp.ActiveUIDocument;
            var doc = uidoc.Document;

            var serverList = new List<SolidServer>();

            var boxSize = 0.1;
            var acumY = 0.0;
            var acumX = 0.0;

            //for (int y = 0; y < 50; y++)
            //{
            //    for (int x = 0; x < 80; x++)
            //    {
            //var solid = SolidCreate.CreateCube(new XYZ(acumX, acumY, 0), boxSize);

            //var server = new SolidServer(uidoc, solid, true, new ColorWithTransparency(255, 165, 0, 0), new ColorWithTransparency(255, 255, 255, 0));
            //var server = new SolidServer(uidoc, 320, 200,0.1);

            var wadPath = UserSelect.GetWad();

            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;


            Server = new FlatFaceServer(uidoc, 160, 100, 0.1);
            //Server = new FlatPointServer(uidoc, 80, 50, 0.02);



            //        serverList.Add(server);

            //        acumX += boxSize;
            //    }
            //    acumY += boxSize;
            //    acumX = 0;
            //}

            //uidoc.UpdateAllOpenViews();

            RevitServices.RegisterServer(Server, uidoc, new HashSet<Document>() { doc });

            //RevitServices.UnregisterAllServers(new HashSet<Document>() { doc });



            //var myFloor = doc.GetElement(new ElementId(264847));
            //Reference faceRef = HostObjectUtils.GetSideFaces((HostObject)myFloor, ShellLayerType.Interior).First();
            //Face face = ((HostObject)myFloor).get_Geometry(new Options()).OfType<Solid>()
            //    .SelectMany(s => s.Faces.Cast<Face>()).FirstOrDefault();


            //          using (Transaction t = new Transaction(uidoc.Document, "Nudge view"))
            //          {
            //              t.Start();
            //              SpatialFieldManager sfm
            //= SpatialFieldManager.GetSpatialFieldManager(
            //  view);

            //              if (null != sfm && 0 < AnalysisService._sfp_index)
            //              {
            //                  sfm.RemoveSpatialFieldPrimitive(
            //                    AnalysisService._sfp_index);

            //                  AnalysisService._sfp_index = -1;
            //              }

            //              AnalysisService.SetAnalysisDisplayStyle(doc);
            //              t.Commit();
            //          }





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
            //    .WithUIDocument(uidoc)
            //    .WithReferenceObj(faceRef)
            //    .WithFaceObj(face);

            //var builder = new AppBuilder();
            //builder.SetIwad(wadPath)
            //    .EnableHighResolution(false)
            //    .WithArgs("-skill", "3")
            //    .WithScale(1)
            //    .WithDocument(doc)
            //    .WithUIDocument(uidoc)
            //    .WithReferenceObj(faceRef)
            //    .WithFaceObj(face);

            var builder = new AppBuilder();
            builder.SetIwad(wadPath)
                .EnableHighResolution(false)
                .WithArgs("-skill", "3")
                .WithScale(1)
                .WithDocument(doc)
                .WithUIDocument(uidoc);

            var dapp = builder.Build();
            //dapp.Run();

            //MessageBox.Show("тут");

            //RevitServices.UnregisterAllServers(new HashSet<Document>() { doc });
            var win = new Window1(dapp);
            win.Show();

            return Result.Succeeded;
        }
    }
}
