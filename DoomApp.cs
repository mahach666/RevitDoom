using RevitDoom.UserInput;
using RevitDoom.Utils;
using RevitDoom.Video;
using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.MathUtils;
using DoomNetFrameworkEngine.Video;
using System;
using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitDoomNetPort
{
    public class DoomApp
    {
        public string IwadPath;
        public bool HighResolution;
        public string[] ExtraArgs;
        public uint Scale;
        public Document Doc;
        public IList<FilledRegion> Pixels;

        public void Run()
        {
            try
            {
                //ConsoleHelper.EnableVirtualTerminalProcessing();
                //Console.OutputEncoding = System.Text.Encoding.UTF8;

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

                //Console.Clear();

                var count = 0;
                var countLimit = 300;
                while (true)
                {
                    //Console.SetCursorPosition(0, 0);
                    count++;
                    if (count == countLimit) break;

                    if (doom.Menu.Active || doom.State != DoomState.Game)
                    {
                        input.PollMenuKeys();
                    }

                    doom.Update();
                    renderer.Render(doom, buffer, Fixed.Zero);
                    if (count < countLimit-1) continue;
                    RevitRenderer.ApplyBGRAToRegions(Doc, buffer, width, height, Pixels, Scale);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error: " + e);
            }
        }
    }
}
