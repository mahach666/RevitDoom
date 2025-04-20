using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDoom.Models;
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
            //var uidoc = UiApp.ActiveUIDocument;
            //var doc = uidoc.Document;     

            var wadPath = UserSelect.GetWad();

            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;


            //Server = new FlatFaceServer(uidoc, 80, 50, 0.1);
            var server = new FlatFaceServer(uidoc, 160, 100, 0.05);
            //Server = new FlatFaceServer(uidoc, 320, 200, 0.025);

            RevitService.RegisterServer(server, uidoc, new HashSet<Document>() { doc });

            var builder = new AppBuilder();
            builder.SetIwad(wadPath)
                .EnableHighResolution(false)
                .WithArgs("-skill", "3")
                .WithScale(1);

            var dapp = builder.Build();

            var win = new Window1(dapp);
            win.Show();

            return Result.Succeeded;
        }

        private static Container ConfigureContainer(
        IObjectsRepository objectsRepository
        , ITabServiceProvider tabServiceProvider
        , StartViewInfo startViewInfo)
        {
            var container = new Container();

            container.RegisterInstance(objectsRepository);
            container.RegisterInstance(tabServiceProvider);
            container.RegisterInstance(startViewInfo);

            container.Register<IRepoService, RepoService>();
            container.Register<ICastomSearchService, SearchService>();
            container.Register<ITabService, TabService>();
            container.Register<IWindowService, WindowService>();
            container.Register<ITreeItemService, TreeItemService>();
            container.Register<IPageService, PageService>();
            container.Register<MainVM>();
            container.Register<MainView>();

            return container;
        }      
    }
}
