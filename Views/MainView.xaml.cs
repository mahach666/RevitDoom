using RevitDoom.ViewModels;
using Window = System.Windows.Window;

namespace RevitDoom.Views
{
    public partial class MainView : Window
    {
        public MainView(MainVM mainVM)
        {
            DataContext = mainVM;
            InitializeComponent();
            Activate();
        }
    }
}
