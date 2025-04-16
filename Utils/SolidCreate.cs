using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitDoom.Utils
{
    internal static class SolidCreate
    {
        public static Solid CreateCube(XYZ center, double cubeSize = 5.0)
        {
            double num = cubeSize / 2.0;
            XYZ xYZ = new XYZ(center.X + num, center.Y + num, center.Z - num);
            XYZ xYZ2 = new XYZ(center.X - num, center.Y + num, center.Z - num);
            XYZ xYZ3 = new XYZ(center.X - num, center.Y - num, center.Z - num);
            XYZ xYZ4 = new XYZ(center.X + num, center.Y - num, center.Z - num);
            Curve curve = Line.CreateBound(xYZ, xYZ2);
            Curve curve2 = Line.CreateBound(xYZ2, xYZ3);
            Curve curve3 = Line.CreateBound(xYZ3, xYZ4);
            Curve curve4 = Line.CreateBound(xYZ4, xYZ);
            CurveLoop curveLoop = new CurveLoop();
            curveLoop.Append(curve);
            curveLoop.Append(curve2);
            curveLoop.Append(curve3);
            curveLoop.Append(curve4);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, new XYZ(0.0, 0.0, 1.0), cubeSize);
        }
    }
}
