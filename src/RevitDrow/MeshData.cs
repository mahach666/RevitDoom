using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB;
using System.Collections.Generic;

public class MeshData
{
    public List<XYZ> Vertices = new();
    public List<IndexTriangle> Triangles = new();
}
