using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using RevitDoom.Utils;
using System.Collections.Generic;
using System.Diagnostics;
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
            //width = (int)(width / scale);
            //height = (int)(height / scale);

            //byte[] pruned = Downsample(buffer, (int)scale);

            GreyscaleBitmapData data
                   = new GreyscaleBitmapData(
                     width, height, buffer);

            SpatialFieldManager sfm
              = SpatialFieldManager.GetSpatialFieldManager(
                view);

            if (null == sfm)
            {
                sfm = SpatialFieldManager
                  .CreateSpatialFieldManager(view, 1);
            }

            int primitiveIndex = sfm.AddSpatialFieldPrimitive(faceRef);

            int nPoints = data.Width * data.Height;

            IList<UV> pts = new List<UV>(nPoints);

            IList<ValueAtPoint> valuesAtPoints
              = new List<ValueAtPoint>(nPoints);


            GetFieldPointsAndValues(ref pts,
              ref valuesAtPoints, ref data, face);


            var schema = sfm.GetRegisteredResults()
                        .Select(id => sfm.GetResultSchema(id))
                        .FirstOrDefault(s => s.Name == "DOOM_FRAME");

            int schemaIndex = schema != null
                ? sfm.GetRegisteredResults().First(id => sfm.GetResultSchema(id).Name == "DOOM_FRAME")
                : sfm.RegisterResult(new AnalysisResultSchema("DOOM_FRAME", "RevitDoom"));        

            FieldDomainPointsByUV fieldPoints
                  = new FieldDomainPointsByUV(pts);

            FieldValues fieldValues
              = new FieldValues(valuesAtPoints);

            sfm.UpdateSpatialFieldPrimitive(
              primitiveIndex, fieldPoints, fieldValues, schemaIndex);
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
        public static byte[] Downsample(byte[] input, int factor)
        {
            if (factor <= 1 || input.Length == 0)
                return input;

            int newLength = input.Length / factor;
            byte[] result = new byte[newLength];

            for (int i = 0; i < newLength; i++)
            {
                result[i] = input[i * factor];
            }

            return result;
        }

    }
}
