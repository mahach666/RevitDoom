using Autodesk.Revit.DB;
using RevitDoom.RevitDrow;

public class SolidData
{
    public Solid Solid;
    public ColorWithTransparency FaceColor;
    public ColorWithTransparency EdgeColor;

    public SolidFaceBufferStorage FaceBuffer;
    public SolidEdgeBufferStorage EdgeBuffer;
}
