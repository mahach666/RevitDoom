using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.MathUtils;
using DoomNetFrameworkEngine.Video;
using RevitDoom.UserInput;
using RevitDoom.Utils;
using RevitDoom.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitDoom
{
    public class DoomApp
    {
        public string IwadPath;
        public bool HighResolution;
        public string[] ExtraArgs;
        public uint Scale;
        public Document Doc;
        public UIDocument Uidoc;
        public IList<XYZ> Pixels;
        public Reference ReferenceObj;
        public Face FaceObj;


        public async Task RunAsync()
        {
            var task = new RevitTask();

            try
            {
                await task.Run(app => Run());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Run()
        {
            try
            {
                var argsList = new[] { "-iwad", IwadPath };
                if (ExtraArgs.Length > 0)
                {
                    argsList = new string[] { "-iwad", IwadPath }.Concat(ExtraArgs).ToArray();
                }

                var cmdArgs = new CommandLineArgs(argsList);
                var config = new Config();
                config.video_highresolution = HighResolution;
                var content = new GameContent(cmdArgs);

                ConsoleUserInput input = null;
                Doom doom = null;

                input = new ConsoleUserInput(config, e => doom?.PostEvent(e));
                doom = new Doom(cmdArgs, config, content, null, null, null, input);

                var renderer = new Renderer(config, content);

                int width = renderer.Width;
                int height = renderer.Height;
                var buffer = new byte[4 * width * height];


                var count = 0;
                var countLimit = 300;
                while (true)
                {

                    if (count == countLimit) break;

                    if (doom.Menu.Active || doom.State != DoomState.Game)
                    {
                        input.PollMenuKeys();
                    }

                    doom.Update();
                    renderer.Render(doom, buffer, Fixed.Zero);

                    count++;

                    if (count < countLimit - 10) continue;
                    RevitAVFRenderer.ApplyBGRAToAnalysisFace(Doc, Doc.ActiveView, FaceObj, ReferenceObj, buffer.Reverse().ToArray(), width, height, Scale);
                    //Uidoc.UpdateAllOpenViews();
                    Uidoc.RefreshActiveView();
                    //TaskDialog.Show("Info", "Обновление...");


                    //ExApp.appInstance.ServerStateMachine.ClearSolidServers();
                }
                AnalysisService._bufer = buffer;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
