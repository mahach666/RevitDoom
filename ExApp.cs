using Autodesk.Revit.UI;
using System.Reflection;

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
            try
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;

                string tabName = "DOOM";
                application.CreateRibbonTab(tabName);

                RibbonPanel doomPanel = application.CreateRibbonPanel(tabName, "DOOM");

                PushButtonData doomBut = new PushButtonData(nameof(DoomExCommand), "DOOM", assemblyLocation, typeof(DoomExCommand).FullName);
                doomPanel.AddItem(doomBut);
            }
            catch
            {

            }

            return Result.Succeeded;
        }
    }
}
