using RevitDoom.Enums;

namespace RevitDoom.Models
{
    public class QualityConverter
    {
        public QualityConverter() { }

        public void Convert(Quality quality,
        out int height,
        out int width,
        out double cellSize)
        { 
            height = 0;
            width = 0;
            cellSize = 0.0;

            switch (quality)
            {
                case Quality.Low:
                    height = 50;
                    width = 80;
                    cellSize = 0.1;
                    break;
                case Quality.Medium:
                    height = 100;
                    width = 160;
                    cellSize = 0.05;
                    break;
                case Quality.High:
                    height = 200;
                    width = 320;
                    cellSize = 0.025;
                    break;
            }
        }
    }
}
