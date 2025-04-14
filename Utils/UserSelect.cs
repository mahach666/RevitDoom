using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

namespace RevitDoom.Utils
{
    internal static class UserSelect
    {
        public static string GetWad()
        {
            Application.EnableVisualStyles();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select WAD-файл";
            dialog.Filter = "WAD files (*.wad)|*.wad|All files (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }
            else
            {
               return null;
            }
        }
    }
}
