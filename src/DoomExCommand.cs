using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DoomNetFrameworkEngine;
using RevitDoom.Contracts;
using RevitDoom.Models;
using RevitDoom.UserInput;
using RevitDoom.Utils;
using RevitDoom.ViewModels;
using RevitDoom.Views;
using SimpleInjector;

namespace RevitDoom
{
    [Transaction(TransactionMode.Manual)]
    public class DoomExCommand : IExternalCommand, IExternalCommandAvailability
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Resolver.Resolve();

            DoomFileLoader.LoadAndFocusDoom(commandData.Application
                , new XYZ(-1, 0, 0)
                , new XYZ(0, 0, 1));

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
            container.Register<CastomUserInput, LowUserInput>(Lifestyle.Singleton);
            container.Register<DoomApp>(Lifestyle.Singleton);
            container.Register<MainVM>();
            container.Register<MainView>();

            return container;
        }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        => true;
    }
}
