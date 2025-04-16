using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDoom.Video
{
    public static class RevitAVFRenderer
    {
        public static void ApplyBGRAToAnalysisFace(
            Document doc,
            View view,
            Face face,
            Reference faceRef,
            byte[] buffer,
            int width,
            int height,
            uint scale = 1)
        {
           
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
