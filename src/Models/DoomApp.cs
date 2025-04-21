using Autodesk.Revit.DB;
using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.MathUtils;
using DoomNetFrameworkEngine.Video;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using RevitDoom.UserInput;
using RevitDoom.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevitDoom.Models
{
    public class DoomApp : IDisposable
    {
        private DoomAppOptions _options;
        private IDirectContextController _directContextService;
        private CastomUserInput _input;
        private Config _config;

        private Doom _doom;
        private Renderer _renderer;
        private byte[] _buffer;
        private int _width;
        private int _height;
        private ExEvent _task;
        private Quality _quality;


        public DoomApp(DoomAppOptions doomAppOptions,
            IDirectContextController directContextService,
            CastomUserInput userInput,
            Config config)
        {
            _options = doomAppOptions;
            _directContextService = directContextService;
            _input = userInput;
            _config = config;
            Initialize();
        }

        private void Initialize()
        {
            _task = new ExEvent();

            var argsList = new[] { "-iwad", _options.IwadPath };
            if (_options.ExtraArgs.Length > 0)
            {
                argsList = new string[] { "-iwad", _options.IwadPath }.Concat(_options.ExtraArgs).ToArray();
            }

            var cmdArgs = new CommandLineArgs(argsList);
            _config.video_highresolution = _options.HighResolution;
            var content = new GameContent(cmdArgs);

            _input.RegisterAppEvent(e => _doom?.PostEvent(e));
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

        public event Action<double> FpsUpdated;

        private int _frameDelay = 1;
        private bool _isValid = true;

        public async Task RunAsync(Quality quality, CancellationToken token)
        {
            SetQuality(quality);
            int frameCount = 0;
            var lastFpsTime = DateTime.Now;

            GlobalKeyboardHook.Install();

            try
            {
                while (!token.IsCancellationRequested && _isValid)
                {
                    NextFrame();

                    await _task.Run(app =>
                    {
                        if (!_directContextService.IsValid(_quality))
                            _directContextService.RegisterServer<FlatFaceServer>(_quality);

                        using (Transaction t = new Transaction(app.ActiveUIDocument.Document, "Graphics update"))
                        {
                            t.Start();
                            app.ActiveUIDocument.RefreshActiveView();
                            t.Commit();
                        }
                    });

                    frameCount++;
                    var now = DateTime.Now;
                    if ((now - lastFpsTime).TotalSeconds >= 1.0)
                    {
                        double fps = frameCount / (now - lastFpsTime).TotalSeconds;
                        FpsUpdated?.Invoke(fps);
                        frameCount = 0;
                        lastFpsTime = now;
                    }

                    await Task.Delay(_frameDelay);
                }
            }
            finally
            {
                GlobalKeyboardHook.Uninstall();
                if (!_isValid)
                {
                    Dispose();
                }
            }
        }

        public void SetQuality(Quality quality)
        {
            if (_quality != quality)
            {
                _quality = quality;
            }
        }

        public void Dispose()
        {
            _isValid = false;
            _input?.Dispose();
            _directContextService?.UnregisterAllServers();
            GlobalKeyboardHook.Uninstall();
        }
    }
}
