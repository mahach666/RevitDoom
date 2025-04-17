using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace RevitDoom.Video
{
    public static class RevitRenderer
    {
        public static void ApplyBGRAToRegions(Document doc, byte[] buffer, int width, int height, IList<XYZ> regions, uint scale = 1)
        {
            if (regions.Count < width * height)
                throw new ArgumentException("Недостаточно регионов для заданных размеров");

            //using (var trans = new Transaction(doc, "Update FilledRegion Colors"))
            //{
            //    trans.Start();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    int i = (x * height + y) * 4;

                    byte r = buffer[i + 0];
                    byte g = buffer[i + 1];
                    byte b = buffer[i + 2];

                    var color = new Color(r, g, b);
                    //ApplyColorToRegion(doc, regions[index], color);
                }
            }
                    //ExApp.appInstance.ServerStateMachine.DrawPointsCube(doc,
                    //                                        regions.ToList(),
                    //                                        0.5,
                    //                                        new ColorWithTransparency(75, 75, 75, 0),
                    //                                        new ColorWithTransparency(75, 75, 75, 0));

            //trans.Commit();
            //}
        }



        public static void RefreshActiveViewByNudge(this UIDocument uidoc, XYZ nudge)
        {
            View view = uidoc.ActiveView;
            using (Transaction t = new Transaction(uidoc.Document, "Nudge view"))
            {
                t.Start();
                ElementTransformUtils.MoveElement(uidoc.Document, view.Id, nudge);
                ElementTransformUtils.MoveElement(uidoc.Document, view.Id, -nudge);
                t.Commit();
            }
        }
    }

}
