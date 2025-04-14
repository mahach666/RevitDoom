using RevitDoom.Utils;
using System;

namespace RevitDoom
{
    public static class App
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var wadPath = UserSelect.GetWad();

            var builder = new AppBuilder();
            builder.SetIwad(wadPath)
                .EnableHighResolution(false)
                .WithArgs("-skill", "3")
                .WithScale(1);

            var app = builder.Build();
            app.Run();
        }
    }
}
