using Autodesk.Revit.DB;
using RevitDoom.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RevitDoom.Views
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private DoomApp _doomApp;
        private RevitTask _task;

        public Window1(DoomApp doomApp)
        {
            _doomApp = doomApp;
            this.Closed += MainWindow_Closed;
            _task = new RevitTask();

            //for (int i = 0; i < 100; i++)
            //{
            //    _doomApp.NextStep();
            //}

            InitializeComponent();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            RevitServices.UnregisterAllServers(new HashSet<Document>() { _doomApp.Doc });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                _doomApp.NextStep();

                await _task.Run(app =>
                {
                    using (Transaction t = new Transaction(_doomApp.Doc, "Trigger graphics update"))
                    {
                        t.Start();

                        // Временно меняем параметр вида (например, масштаб)
                        int oldScale = _doomApp.Doc.ActiveView.Scale;
                        _doomApp.Doc.ActiveView.Scale = oldScale == 100 ? 101 : 100;

                        t.Commit();
                    }
                }
                    );
            }
        }


    }
}
