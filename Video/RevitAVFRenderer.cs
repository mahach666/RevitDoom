using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using RevitDoom.Utils;
using System;
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

            RemoveEveryNth(pts, 2);
            RemoveEveryNth(valuesAtPoints, 2);



            //valuesAtPoints =valuesAtPoints.Reverse().ToList();

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

            double umin = bb.Min.U + 0.1;
            double umax = bb.Max.U - 0.1;
            double ustep = (umax - umin) / data.Width;
            double u = umin;

            double v = bb.Min.V + 0.1;
            double vmax = bb.Max.V - 0.1;
            double vstep = (vmax - v) / data.Height;

            List<double> values = new List<double>(1);

            for (int y = 0; y < data.Height; ++y)
            {
                double v_current = v + y * vstep;

                Debug.Assert(v_current < vmax, "expected v to remain within bounds");

                u = umin;

                for (int x = 0; x < data.Width; ++x, u += ustep)
                {
                    Debug.Assert(u < umax, "expected u to remain within bounds");

                    // Зеркальное обращение по вертикали:
                    double brightness = data.GetBrightnessAt(x, data.Height - 1 - y);

                    UV uv = new UV(u, v_current);
                    pts.Add(uv);

                    values.Clear();
                    values.Add(brightness);
                    valuesAtPoints.Add(new ValueAtPoint(values));
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

        public static void RemoveEveryNth<T>(IList<T> list, int n)
        {
            if (n <= 0)
                throw new ArgumentException("n must be greater than 0");

            int count = 0;

            // Проходим с конца, чтобы индексы не сдвигались при удалении
            for (int i = list.Count - 1; i >= 0; i--)
            {
                count++;

                if (count % n == 0)
                {
                    list.RemoveAt(i);
                }
            }
        }


    }
}
