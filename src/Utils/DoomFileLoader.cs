using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace RevitDoom
{
    public static class DoomFileLoader
    {
        public static UIDocument LoadAndFocusDoom(UIApplication uiapp, XYZ forwardDir, XYZ upDir)
        {
            string asmPath = Assembly.GetExecutingAssembly().Location;
            string asmDir = Path.GetDirectoryName(asmPath);
            string sourcePath = Path.Combine(asmDir, "AddFiles", "DOOM.rvt");
            string destPath = Path.Combine(asmDir, "DOOM.rvt");
            UIDocument uidoc;

            if (uiapp?.ActiveUIDocument?.Document == null
                || !uiapp.ActiveUIDocument.Document.Title.Contains("DOOM"))
            {
                File.Copy(sourcePath, destPath, true);
                ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(destPath);
                OpenOptions openOpts = new OpenOptions();
                uidoc = uiapp.OpenAndActivateDocument(modelPath, openOpts, false);
            }
            else
            {
                uidoc = uiapp.ActiveUIDocument;
            }

            View3D view3D = Ensure3DView(uiapp, uidoc);

            BoundingBoxXYZ bb = view3D.GetSectionBox() != null
                ? view3D.GetSectionBox()
                : view3D.get_BoundingBox(view3D);
            if (bb == null) throw new InvalidOperationException("invalid BoundingBox.");

            XYZ center = (bb.Min + bb.Max) / 2;
            double radius = bb.Min.DistanceTo(bb.Max) / 2;
            XYZ eye = center - forwardDir.Multiply(radius * 1.5);

            SetCameraOrientation(uidoc, view3D, eye, upDir, forwardDir);

            return uidoc;
        }

        private static void SetCameraOrientation(
           UIDocument uidoc, View3D view3D,
           XYZ eye, XYZ up, XYZ forward)
        {
            using (var tx = new Transaction(view3D.Document, "Set 3D Camera"))
            {
                tx.Start();
                var vo = new ViewOrientation3D(eye, up, forward);
                view3D.SetOrientation(vo);

                uidoc.RefreshActiveView();

                var uiViews = uidoc.GetOpenUIViews();
                UIView uiView = uiViews.FirstOrDefault(v => v.ViewId == view3D.Id)
                               ?? uiViews.First();

                uiView.ZoomAndCenterRectangle(new XYZ(1.721217703, 9.428724431, 5.803343094)
                , new XYZ(-5.015588210, -1.235340293, -3.342581203));

                tx.Commit();
            }
            uidoc.RefreshActiveView();
        }

        private static View3D Ensure3DView(UIApplication uiapp, UIDocument uidoc)
        {
            var doc = uidoc.Document;
            if (doc.ActiveView is View3D active3D && !active3D.IsTemplate)
                return active3D;

            var view3d = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate);

            if (view3d == null)
                throw new InvalidOperationException("not some 3D");

            uidoc.RequestViewChange(view3d);
            return view3d;
        }
    }
}
