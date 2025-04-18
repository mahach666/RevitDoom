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

     

            var wadPath = UserSelect.GetWad();

            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;


            Server = new FlatFaceServer(uidoc, 80, 50, 0.2);
            //Server = new FlatFaceServer(uidoc, 160, 100, 0.1);
            //Server = new FlatFaceServer(uidoc, 320, 200, 0.05);           

            RevitServices.RegisterServer(Server, uidoc, new HashSet<Document>() { doc });           

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
    }
}
