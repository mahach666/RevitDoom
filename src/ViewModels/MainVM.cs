using RevitDoom.Commands;
using RevitDoom.Contracts;
using RevitDoom.Enums;
using RevitDoom.Models;
using RevitDoom.UserInput;
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
        private CancellationTokenSource _cts;

        public MainVM(DoomApp doomApp,
            IDirectContextController directContextService)
        {
            _doomApp = doomApp;
            _directContextService = directContextService;
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
                    SetQuality(_quality);
                }
            }
        }

        public ICommand RunGameCommand => new RelayCommand(RunGame);

        private async void RunGame()
        {
            _cts = new CancellationTokenSource();
            _doomApp.FpsUpdated += fps => FpsText = $"FPS: {fps:F1}";
            await _doomApp.RunAsync(_quality, _cts.Token);
        }
        private void SetQuality(Quality quality)
        {
            _doomApp.SetQuality(quality);
        }
        public ICommand StopGameCommand => new RelayCommand(StopGame);

        private void StopGame()
        {
            _cts?.Cancel();
            GlobalKeyboardHook.Uninstall();
        }

        public ICommand WindowClosingCommand => new RelayCommand(OnWindowClosing);
        private async void OnWindowClosing()
        {
            try
            {
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
                await Task.Delay(2000);
                _doomApp.Dispose();
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
