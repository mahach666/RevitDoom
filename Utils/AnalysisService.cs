using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RevitDoom.Utils
{
    internal class AnalysisService
    {
        static DateTime _lastUpdate = DateTime.Now.Subtract(_interval);
        static TimeSpan _interval = new TimeSpan(0, 0, 0, 0, 5000);
        const int _width = 320;
        const int _height = 200;
        static byte[] _lastHash = null;

        static byte[] _bufer = null;

        static int _sfp_index = -1;
        static Reference _faceReference = null;


        void SetAnalysisDisplayStyle(Document doc)
        {
            AnalysisDisplayStyle analysisDisplayStyle;

            const string styleName
              = "Revit Webcam Display Style";

            // extract existing display styles with specific name

            FilteredElementCollector a
              = new FilteredElementCollector(doc);

            IList<Element> elements = a
              .OfClass(typeof(AnalysisDisplayStyle))
              .Where(x => x.Name.Equals(styleName))
              .Cast<Element>()
              .ToList();

            if (0 < elements.Count)
            {
                // use the existing display style

                analysisDisplayStyle = elements[0]
                  as AnalysisDisplayStyle;
            }
            else
            {
                // create new display style:

                // coloured surface settings:

                AnalysisDisplayColoredSurfaceSettings
                  coloredSurfaceSettings
                    = new AnalysisDisplayColoredSurfaceSettings();

                coloredSurfaceSettings.ShowGridLines = false;

                // color settings:

                AnalysisDisplayColorSettings colorSettings
                  = new AnalysisDisplayColorSettings();

                colorSettings.MaxColor = new Color(255, 255, 255);
                colorSettings.MinColor = new Color(0, 0, 0);

                // legend settings:

                AnalysisDisplayLegendSettings legendSettings
                  = new AnalysisDisplayLegendSettings();

                legendSettings.NumberOfSteps = 10;
                legendSettings.Rounding = 0.05;
                legendSettings.ShowDataDescription = false;
                legendSettings.ShowLegend = true;

                // extract legend text:

                a = new FilteredElementCollector(doc);

                elements = a
                  .OfClass(typeof(TextNoteType))
                  .Where(x => x.Name == "LegendText")
                  .Cast<Element>()
                  .ToList();

                if (0 < elements.Count)
                {
                    // if LegendText exists, use it for this display style

                    TextNoteType textType = elements[0] as TextNoteType;

                    legendSettings.TextTypeId = textType.Id;
                }

                // create the analysis display style:

                analysisDisplayStyle = AnalysisDisplayStyle
                  .CreateAnalysisDisplayStyle(
                    doc, styleName, coloredSurfaceSettings,
                    colorSettings, legendSettings);
            }

            // assign the display style to the active view

            doc.ActiveView.AnalysisDisplayStyleId
              = analysisDisplayStyle.Id;
        }


        static void GetFieldPointsAndValues(
      ref IList<UV> pts,
      ref IList<ValueAtPoint> valuesAtPoints,
      ref GreyscaleBitmapData data,
      Face face)
        {
            BoundingBoxUV bb = face.GetBoundingBox();

            double umin = bb.Min.U;
            double umax = bb.Max.U;
            double ustep = (umax - umin) / data.Width;
            double u = umin;

            double v = bb.Min.V;
            double vmax = bb.Max.V;
            double vstep = (vmax - v) / data.Height;

            List<double> values = new List<double>(1);

            for (int y = 0; y < data.Height; ++y, v += vstep)
            {
                Debug.Assert(v < vmax,
                  "expected v to remain within bounds");

                u = umin;

                for (int x = 0; x < data.Width; ++x, u += ustep)
                {
                    Debug.Assert(u < umax,
                      "expected u to remain within bounds");

                    double brightness = data.GetBrightnessAt(
                      x, y);

                    UV uv = new UV(u, v);
                    pts.Add(uv);

                    values.Clear();
                    values.Add(brightness);
                    valuesAtPoints.Add(new ValueAtPoint(
                      values));
                }
            }
        }
        static int CompareBytes(byte[] a, byte[] b)
        {
            int n = a.Length;
            int d = n - b.Length;

            if (0 == d)
            {
                for (int i = 0; i < n && 0 == d; ++i)
                {
                    d = a[i] - b[i];
                }
            }
            return d;
        }
        static void Log(string msg)
        {
            string dt = DateTime.Now.ToString("u");
            Debug.Print(dt + " " + msg);
        }
        static void OnIdling(
      object sender,
      IdlingEventArgs e)
        {
            if (DateTime.Now.Subtract(_lastUpdate)
              > _interval)
            {
                Log("OnIdling");

                GreyscaleBitmapData data
                  = new GreyscaleBitmapData(
                    _width, _height, _bufer);

                byte[] hash = data.HashValue;

                if (null == _lastHash
                  || 0 != CompareBytes(hash, _lastHash))
                {
                    _lastHash = hash;

                    // access active document from sender:

                    Application app = sender as Application;

                    Debug.Assert(null != app,
                      "expected a valid Revit application instance");

                    UIApplication uiapp = new UIApplication(app);
                    UIDocument uidoc = uiapp.ActiveUIDocument;
                    Document doc = uidoc.Document;

                    Log("OnIdling image changed, active document "
                      + doc.Title);

                    Transaction transaction
                      = new Transaction(doc, "Revit Webcam Update");

                    transaction.Start();

                    View view = doc.ActiveView; // maybe has to be 3D

                    SpatialFieldManager sfm
                      = SpatialFieldManager.GetSpatialFieldManager(
                        view);

                    if (null == sfm)
                    {
                        sfm = SpatialFieldManager
                          .CreateSpatialFieldManager(view, 1);
                    }

                    if (0 > _sfp_index)
                    {
                        _sfp_index = sfm.AddSpatialFieldPrimitive(
                          _faceReference);
                    }

                    int nPoints = data.Width * data.Height;

                    IList<UV> pts = new List<UV>(nPoints);

                    IList<ValueAtPoint> valuesAtPoints
                      = new List<ValueAtPoint>(nPoints);

                    //Face face = _faceReference.GeometryObject
                    //  as Face;
                    var myFloor = doc.GetElement(_faceReference.ElementId);

                    Face face = ((HostObject)myFloor).get_Geometry(new Options()).OfType<Solid>()
                .SelectMany(s => s.Faces.Cast<Face>()).FirstOrDefault();

                    GetFieldPointsAndValues(ref pts,
                      ref valuesAtPoints, ref data, face);

                    AnalysisResultSchema resultSchema = new AnalysisResultSchema("My Webcam Data", "Webcam");

                    int resultIndex = sfm.RegisterResult(resultSchema);


                    FieldDomainPointsByUV fieldPoints
                      = new FieldDomainPointsByUV(pts);

                    FieldValues fieldValues
                      = new FieldValues(valuesAtPoints);

                    sfm.UpdateSpatialFieldPrimitive(
                      _sfp_index, fieldPoints, fieldValues, resultIndex);

                    doc.Regenerate();
                    transaction.Commit();

                    _lastUpdate = DateTime.Now;
                }
            }
        }

    }
}
