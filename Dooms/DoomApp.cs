using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.MathUtils;
using DoomNetFrameworkEngine.Video;
using RevitDoom.Contracts;
using RevitDoom.Models;
using RevitDoom.UserInput;
using System.Linq;

namespace RevitDoom.Dooms
{
    public class DoomApp
    {
        private DoomAppOptions _options;
        private IDirectContextController _directContextService;
        private WpfUserInput _input;
        private Config _config;

        private Doom _doom;
        private Renderer _renderer;
        private byte[] _buffer;
        private int _width;
        private int _height;

        public DoomApp(DoomAppOptions doomAppOptions,
            IDirectContextController directContextService,
            WpfUserInput userInput,
            Config config)
        {
            _options = doomAppOptions;
            _directContextService = directContextService;
            _input = userInput;
            _config = config;
            Initialize();
        }

        public void Initialize()
        {
            var argsList = new[] { "-iwad", _options.IwadPath };
            if (_options.ExtraArgs.Length > 0)
            {
                argsList = new string[] { "-iwad", _options.IwadPath }.Concat(_options.ExtraArgs).ToArray();
            }

            var cmdArgs = new CommandLineArgs(argsList);
            _config.video_highresolution = _options.HighResolution;
            var content = new GameContent(cmdArgs);
          
            _doom = new Doom(cmdArgs, _config, content, null, null, null, _input);

            _renderer = new Renderer(_config, content);

            _width = _renderer.Width;
            _height = _renderer.Height;
            _buffer = new byte[4 * _width * _height];
        }

        public void NextFrame()
        {
            try
            {
                if (_doom.Menu.Active || _doom.State != DoomState.Game)
                {
                    _input.PollMenuKeys();
                }

                _doom.Update();
                _renderer.Render(_doom, _buffer, Fixed.Zero);

                _directContextService.Update(_buffer, _width, _height);
            }
            catch
            {

            }
        }
    }
}
