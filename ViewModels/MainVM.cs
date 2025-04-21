using Autodesk.Revit.DB;
using RevitDoom.Commands;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using RevitDoom.Models;
using RevitDoom.UserInput;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitDoom.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        private DoomApp _doomApp;
        private IDirectContextController _directContextService;
        private RevitTask _task;

        private bool _isClose = false;
        private bool _isRunning = false;


        public MainVM(DoomApp doomApp,
            IDirectContextController directContextService)
        {
            _doomApp = doomApp;
            _directContextService = directContextService;
            _task = new RevitTask();
        }

        private string _fpsText = $"FPS: -";
        public string FpsText
        {
            get => _fpsText;
            set
            {
                if (_fpsText != value)
                {
                    _fpsText = value;
                    OnPropertyChanged();
                }
            }
        }

        private Quality _quality = Quality.Medium;
        public Quality Quality
        {
            get => _quality;
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RunGameCommand => new RelayCommand(RunGame);

        private async void RunGame()
        {

            _isRunning = true;
            var frameCount = 0;
            var lastFpsTime = DateTime.Now;

            try
            {
                while (_isRunning)
                {
                    GlobalKeyboardHook.Install();
                    _doomApp.NextFrame();

                    await _task.Run(app =>
                    {
                        if (!_directContextService.IsValid(_quality))
                        {
                            _directContextService.RegisterServer<FlatFaceServer>(_quality);
                        }
                        using (Transaction t = new Transaction(app.ActiveUIDocument.Document, "Graphics update"))
                        {
                            t.Start();
                            app.ActiveUIDocument.RefreshActiveView();
                            t.Commit();
                        }
                    }
                        );

                    frameCount++;

                    var now = DateTime.Now;
                    if ((now - lastFpsTime).TotalSeconds >= 1.0)
                    {
                        double fps = frameCount / (now - lastFpsTime).TotalSeconds;
                        FpsText = $"FPS: {fps:F1}";
                        frameCount = 0;
                        lastFpsTime = now;
                    }
                    await Task.Delay(1);
                }
            }
            catch
            {
            }
            finally
            {
                _isRunning = false;
                GlobalKeyboardHook.Uninstall();
            }
        }

        public ICommand StopGameCommand => new RelayCommand(StopGame);

        private void StopGame()
        {
            _isRunning = false;
        }

        public ICommand WindowClosingCommand => new RelayCommand(OnWindowClosing);
        private void OnWindowClosing()
        {
            try
            {
                _isRunning = false;
                _isClose = true;
                _directContextService.UnregisterAllServers();
                GlobalKeyboardHook.Uninstall();
            }
            catch
            {
                GlobalKeyboardHook.Uninstall();
                _directContextService.UnregisterAllServers();
            }
            finally
            {
                Thread.Sleep(2000);
                _directContextService.UnregisterAllServers();
                GlobalKeyboardHook.Uninstall();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
