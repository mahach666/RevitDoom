using RevitDoom.Contracts;
using RevitDoom.UserInput;
using RevitDoom.ViewModels;
using System.Windows.Input;
using Window = System.Windows.Window;

namespace RevitDoom.Views
{
    public partial class MainView : Window
    {
        private CastomUserInput _input;

        public MainView(MainVM mainVM,
            CastomUserInput userInput)
        {
            DataContext = mainVM;

            _input = userInput;

            InitializeComponent();
            Activate();
            //Focus();
            //_input.AttachWindow(this);
            //Keyboard.Focus(this);
        }
    }
}
