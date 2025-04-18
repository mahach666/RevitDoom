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
        public static WpfUserInput Input;

        private Doom _doom;
        private Renderer _renderer;
        private byte[] _buffer;
        private int _width;
        private int _height;

        public void Initialize()
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

            Input = null;
            _doom = null;

            Input = new WpfUserInput(config, e => _doom?.PostEvent(e));
            _doom = new Doom(cmdArgs, config, content, null, null, null, Input);

            _renderer = new Renderer(config, content);

            _width = _renderer.Width;
            _height = _renderer.Height;
            _buffer = new byte[4 * _width * _height];

            for (int i = 0; i < 250; i++)
            {
                _doom.Update();
            }
        }

        public async Task RunAsync()
        {
            var task = new RevitTask();

            try
            {
                await task.Run(app =>
                 Run()
                );
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

                WpfUserInput input = null;
                Doom doom = null;

                input = new WpfUserInput(config, e => doom?.PostEvent(e));
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


                    //RevitAVFRenderer.ApplyBGRAToAnalysisFace(Doc, Doc.ActiveView, FaceObj, ReferenceObj, buffer, width, height, Scale);

                    DoomExCommand.Server.SetPixels(buffer, width, height);

                    //var view = Uidoc.ActiveView;
                    //using (Transaction t = new Transaction(Doc, "Trigger view update"))
                    //{
                    //    t.Start();
                    //    int s = view.Scale;
                    //    view.Scale = s == 100 ? 101 : 100;
                    //    view.Scale = s;
                    //    t.Commit();
                    //}


                    //Uidoc.UpdateAllOpenViews();
                    //Uidoc.RefreshActiveView();

                    //DoomExCommand.UiApp.Application.InvalidateDocumentGraphics(doc);



                    //using (Transaction t = new Transaction(Doc, "Trigger graphics update"))
                    //{
                    //    t.Start();

                    //    // Временно меняем параметр вида (например, масштаб)
                    //    int oldScale = Doc.ActiveView.Scale;
                    //    Doc.ActiveView.Scale = oldScale == 100 ? 101 : 100;

                    //    t.Commit();
                    //}



                    //DoomExCommand.UiApp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.ActivateView));

                    //var commandId = RevitCommandId.LookupPostableCommandId("ID_ZOOM_TO_FIT");
                    //uiApp.PostCommand(commandId);


                    //Uidoc.ShowElements(ReferenceObj.ElementId);

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
        public void NextStep()
        {
            try
            {

                if (_doom.Menu.Active || _doom.State != DoomState.Game)
                {
                    //input.PollMenuKeys();
                }

                _doom.Update();
                _renderer.Render(_doom, _buffer, Fixed.Zero);


                DoomExCommand.Server.SetPixels(_buffer, _width, _height);
                //RevitAVFRenderer.ApplyBGRAToAnalysisFace(Doc, Doc.ActiveView, FaceObj, ReferenceObj, _buffer, _width, _height, Scale);



            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
