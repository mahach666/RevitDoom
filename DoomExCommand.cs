using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDoom.Utils;
using RevitDoom.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

     

            var wadPath = UserSelect.GetWad();

            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;


            //Server = new FlatFaceServer(uidoc, 80, 50, 0.1);
            Server = new FlatFaceServer(uidoc, 160, 100, 0.05);
            //Server = new FlatFaceServer(uidoc, 320, 200, 0.025);

            RevitServices.RegisterServer(Server, uidoc, new HashSet<Document>() { doc });

            //var myFloor = doc.GetElement(new ElementId(264131));
            //Reference faceRef = HostObjectUtils.GetSideFaces((HostObject)myFloor, ShellLayerType.Interior).First();
            //Face face = ((HostObject)myFloor).get_Geometry(new Options()).OfType<Solid>()
            //    .SelectMany(s => s.Faces.Cast<Face>()).FirstOrDefault();

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

            var win = new Window1(dapp);
            win.Show();

            return Result.Succeeded;
        }

        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";
            string fullPath = Path.Combine(assemblyPath, assemblyName);

            if (File.Exists(fullPath))
            {
                return Assembly.LoadFrom(fullPath);
            }

            return null;
        }
    }
}
