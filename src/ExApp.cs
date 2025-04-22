using Autodesk.Revit.UI;
using System;
using System.Windows.Media.Imaging;

namespace RevitDoom
{
    internal class ExApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            BuildRibbonPanel(application);

            return Result.Succeeded;
        }

        private static void BuildRibbonPanel(UIControlledApplication application)
        {
            var assembly = typeof(ExApp).Assembly;

            var APP_NAME = "DOOM";

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(APP_NAME);

            PushButtonData buttonData = new PushButtonData(
                            APP_NAME,
                            "DOOM",
                            assembly.Location,
                            typeof(DoomExCommand).FullName)
            {
                LargeImage = new BitmapImage(new Uri(@"/RevitDoom;component/Icons/doomlogo.png", UriKind.RelativeOrAbsolute))
            };

            buttonData.AvailabilityClassName = typeof(DoomExCommand).FullName;

            ribbonPanel.AddItem(buttonData);
        }
    }
}
