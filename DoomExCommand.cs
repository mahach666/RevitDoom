using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DoomNetFrameworkEngine;
using RevitDoom.Contracts;
using RevitDoom.Dooms;
using RevitDoom.Models;
using RevitDoom.UserInput;
using RevitDoom.Utils;
using RevitDoom.ViewModels;
using RevitDoom.Views;
using SimpleInjector;

namespace RevitDoom
{
    [Transaction(TransactionMode.Manual)]
    public class DoomExCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Resolver.Resolve();

            var uiApp = commandData.Application;

            var wadPath = UserSelect.GetWad();
            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;

            var options = new DoomAppOptions
            {
                IwadPath = wadPath,
                HighResolution = false,
                ExtraArgs = new[] { "-skill", "3" }
            };

            var container = ConfigureContainer(uiApp, options);

            var window = container
         .GetInstance<MainView>();

            window.Show();
            //Server = new FlatFaceServer(uidoc, 80, 50, 0.1);
            //var server = new FlatFaceServer(uidoc, 160, 100, 0.05);
            //Server = new FlatFaceServer(uidoc, 320, 200, 0.025);

            //RevitService.RegisterServer(server, uidoc, new HashSet<Document>() { doc });

            //var builder = new DoomAppBuilder();
            //builder.SetIwad(wadPath)
            //    .EnableHighResolution(false)
            //    .WithArgs("-skill", "3")
            //    .WithScale(1);

            //var doomApp = builder.Build();

            //var win = new Window1(doomApp);
            //win.Show();

            return Result.Succeeded;
        }

        private static Container ConfigureContainer(UIApplication uIApplication,
            DoomAppOptions doomAppOptions)
        {
            var container = new Container();
            var config = new Config();

            container.RegisterInstance(uIApplication);
            container.RegisterInstance(doomAppOptions);
            container.RegisterInstance(config);

            container.Register<QualityConverter>(Lifestyle.Singleton);
            container.Register<IRevitDirectServerController, RevitService>(Lifestyle.Singleton);
            container.Register<IServerFactory, ServerFactory>(Lifestyle.Singleton);
            container.Register<IDirectContextController, DirectContextService>(Lifestyle.Singleton);
            //container.Register<Config>();
            container.Register<WpfUserInput>(Lifestyle.Singleton);
            container.Register<DoomApp>(Lifestyle.Singleton);
            container.Register<MainVM>();
            container.Register<MainView>();

            return container;
        }
    }
}
