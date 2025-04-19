using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace RevitDoom
{
    internal class ExApp : IExternalApplication
    {
        //public ServerStateMachine ServerStateMachine { get; private set; }
        //public static ExApp appInstance { get; private set; }

        public Result OnShutdown(UIControlledApplication application)
        {


            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //appInstance = this;
                //ServerStateMachine = new ServerStateMachine(this);

                string assemblyLocation = Assembly.GetExecutingAssembly().Location;

                string tabName = "DOOM";
                application.CreateRibbonTab(tabName);

                RibbonPanel doomPanel = application.CreateRibbonPanel(tabName, "DOOM");

                PushButtonData doomBut = new PushButtonData(nameof(DoomExCommand), "DOOM", assemblyLocation, typeof(DoomExCommand).FullName);
                doomPanel.AddItem(doomBut);
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

            return Result.Succeeded;
        }
    }
}
