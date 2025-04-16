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


            //var serverList = new List<SolidServer>();

            //var boxSize = 1;
            //var acumY = 0;
            //var acumX = 0;

            //for (int y = 0; y < 320; y++)
            //{
            //    for (int x = 0; x < 1; x++)
            //    {
            //        var solid = CreateCube(new XYZ(acumX, acumY, 0), boxSize);

            //        var server = new SolidServer(uidoc, solid, true, new ColorWithTransparency(255, 165, 0, 0), new ColorWithTransparency(255, 255, 255, 0));

            //        serverList.Add(server);

            //        acumX += boxSize; 
            //    }
            //    acumY += boxSize;
            //    acumX = 0;
            //}
            //uidoc.UpdateAllOpenViews();



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
    }
}
