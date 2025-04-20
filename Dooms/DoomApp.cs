using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.MathUtils;
using DoomNetFrameworkEngine.Video;
using RevitDoom.Models;
using RevitDoom.UserInput;
using System.Linq;

namespace RevitDoom.Dooms
{
    public class DoomApp
    {
        private WpfUserInput _input;

        private DoomAppOptions _options;

        private Doom _doom;
        private Renderer _renderer;
        private byte[] _buffer;
        private int _width;
        private int _height;

        public DoomApp(DoomAppOptions doomAppOptions)
        {
            _options = doomAppOptions;
        }

        public void Initialize()
        {
            var argsList = new[] { "-iwad", _options.IwadPath };
            if (_options.ExtraArgs.Length > 0)
            {
                argsList = new string[] { "-iwad", _options.IwadPath }.Concat(_options.ExtraArgs).ToArray();
            }

            var cmdArgs = new CommandLineArgs(argsList);
            var config = new Config();
            config.video_highresolution = _options.HighResolution;
            var content = new GameContent(cmdArgs);
          
            _doom = new Doom(cmdArgs, config, content, null, null, null, _input);

            _renderer = new Renderer(config, content);

            _width = _renderer.Width;
            _height = _renderer.Height;
            _buffer = new byte[4 * _width * _height];
        }

        public void NextStep()
        {
            try
            {

                if (_doom.Menu.Active || _doom.State != DoomState.Game)
                {
                    _input.PollMenuKeys();
                }

                _doom.Update();
                _renderer.Render(_doom, _buffer, Fixed.Zero);

                DoomExCommand.Server.SetPixels(_buffer, _width, _height);
            }
            catch
            {

            }
        }
    }
}
