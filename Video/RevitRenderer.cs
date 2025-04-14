using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RevitDoom.Video
{
    public static class RevitRenderer
    {
        public static void ApplyBGRAToRegions(Document doc, byte[] buffer, int width, int height, IList<FilledRegion> regions, uint scale = 1)
        {
            if (regions.Count < width * height)
                throw new ArgumentException("Недостаточно регионов для заданных размеров");

            using (var trans = new Transaction(doc, "Update FilledRegion Colors"))
            {
                trans.Start();
                //var indexer = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        //int index = y + x;

                        int i = (x * height + y) * 4;

                        byte b = buffer[i + 0];
                        byte g = buffer[i + 1];
                        byte r = buffer[i + 2];

                        var color = new Color(r, g, b);
                        ApplyColorToRegion(doc, regions[index], color);
                        //indexer++;
                    }
                }

                trans.Commit();
            }
        }

        private static void ApplyColorToRegion(Document doc, FilledRegion region, Color color)
        {
            var overrideSettings = new OverrideGraphicSettings();
            overrideSettings.SetSurfaceForegroundPatternColor(color);
            overrideSettings.SetSurfaceForegroundPatternId(GetSolidFillPatternId(doc));

            doc.ActiveView.SetElementOverrides(region.Id, overrideSettings);
        }

        private static ElementId GetSolidFillPatternId(Document doc)
        {
            var collector = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement));
            foreach (FillPatternElement patternElem in collector)
            {
                var pattern = patternElem.GetFillPattern();
                if (pattern.IsSolidFill)
                    return patternElem.Id;
            }

            throw new InvalidOperationException("Не найден Solid Fill Pattern.");
        }
    }

}
