using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DoomNetFrameworkEngine.UserInput;
using RevitDoom.Contracts;
using RevitDoom.Dooms;
using RevitDoom.Models;
using RevitDoom.UserInput;
using RevitDoom.Utils;
using RevitDoom.Views;
using SimpleInjector;
using System.Collections.Generic;

namespace RevitDoom
{
    [Transaction(TransactionMode.Manual)]
    public class DoomExCommand : IExternalCommand
    {
        //public static FlatFaceServer Server;
        //static public UIApplication UiApp;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Resolver.Resolve();

            var uiApp = commandData.Application;            

            var wadPath = UserSelect.GetWad();
            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;


            //Server = new FlatFaceServer(uidoc, 80, 50, 0.1);
            //var server = new FlatFaceServer(uidoc, 160, 100, 0.05);
            //Server = new FlatFaceServer(uidoc, 320, 200, 0.025);

            //RevitService.RegisterServer(server, uidoc, new HashSet<Document>() { doc });

            var builder = new DoomAppBuilder();
            builder.SetIwad(wadPath)
                .EnableHighResolution(false)
                .WithArgs("-skill", "3")
                .WithScale(1);

            var doomApp = builder.Build();

            var win = new Window1(doomApp);
            win.Show();

            return Result.Succeeded;
        }

        private static Container ConfigureContainer(
      UIApplication uIApplication,
      DoomApp doomApp)
        {
            var container = new Container();

            container.RegisterInstance(uIApplication);
            container.RegisterInstance(doomApp);

            container.Register<QualityConverter>();
            container.Register<IRevitDirectServerController, RevitService>();
            container.Register<IServerFactory, ServerFactory>();
            container.Register<IDirectContextController, DirectContextService>();
            container.Register<IUserInput, WpfUserInput>();
            container.Register<IPageService, PageService>();
            container.Register<MainVM>();
            container.Register<MainView>();

            return container;
        }      
    }
}
