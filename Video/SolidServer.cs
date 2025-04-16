using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDoom.RevitDrow;
using System.Collections.Generic;
using System;
using System.Linq;

public class SolidServer : IDirectContext3DServer, IExternalServer
{
    private Guid m_guid = Guid.NewGuid();
    private UIDocument m_uiDocument;

    private List<SolidData> solids = new();

    public SolidServer(UIDocument uiDoc, int width, int height, double cubeSize)
    {
        m_uiDocument = uiDoc;

        // Генерируем "экран" из кубиков
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                XYZ basePoint = new XYZ(x * cubeSize, y * cubeSize, 0);
                Solid cube = CreateCube(basePoint, cubeSize);
                var color = new ColorWithTransparency(0, 0, 0, 0); // черный, непрозрачный

                solids.Add(new SolidData
                {
                    Solid = cube,
                    FaceColor = color,
                    EdgeColor = color
                });
            }
        }
    }

    public void SetPixel(int x, int y, int width, ColorWithTransparency color)
    {
        int index = y * width + x;
        if (index >= 0 && index < solids.Count)
        {
            solids[index].FaceColor = color;
            solids[index].EdgeColor = color;

            solids[index].FaceBuffer = null;
            solids[index].EdgeBuffer = null;
        }
    }

    private Solid CreateCube(XYZ basePoint, double size)
    {
        XYZ p0 = basePoint;
        XYZ p1 = basePoint + new XYZ(size, 0, 0);
        XYZ p2 = basePoint + new XYZ(size, size, 0);
        XYZ p3 = basePoint + new XYZ(0, size, 0);

        XYZ p4 = basePoint + new XYZ(0, 0, size);
        XYZ p5 = basePoint + new XYZ(size, 0, size);
        XYZ p6 = basePoint + new XYZ(size, size, size);
        XYZ p7 = basePoint + new XYZ(0, size, size);

        List<CurveLoop> loops = new();
        // We'll just make a cube from a solid box (box utility)
        return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>
        {
            CurveLoop.Create(new List<Curve>
            {
                Line.CreateBound(p0, p1),
                Line.CreateBound(p1, p2),
                Line.CreateBound(p2, p3),
                Line.CreateBound(p3, p0),
            })
        }, XYZ.BasisZ, size);
    }

    public void RenderScene(View view, DisplayStyle style)
    { 
        foreach (var data in solids)
        {
            if (data.FaceBuffer == null)
            {
                data.FaceBuffer = new SolidFaceBufferStorage(style, data.Solid);
                data.FaceBuffer.AddVertexPositionNormalColored(data.FaceColor);
            }

            if (data.EdgeBuffer == null)
            {
                data.EdgeBuffer = new SolidEdgeBufferStorage(style, data.Solid);
                data.EdgeBuffer.AddVertexPosition(data.EdgeColor);
            }

            DrawContext.FlushBuffer(data.FaceBuffer.VertexBuffer, data.FaceBuffer.VertexBufferCount, data.FaceBuffer.IndexBuffer, data.FaceBuffer.IndexBufferCount, data.FaceBuffer.VertexFormat, data.FaceBuffer.EffectInstance, data.FaceBuffer.BufferPrimitiveType, 0, data.FaceBuffer.PrimitiveCount);

            DrawContext.FlushBuffer(data.EdgeBuffer.VertexBuffer, data.EdgeBuffer.VertexBufferCount, data.EdgeBuffer.IndexBuffer, data.EdgeBuffer.IndexBufferCount, data.EdgeBuffer.VertexFormat, data.EdgeBuffer.EffectInstance, data.EdgeBuffer.BufferPrimitiveType, 0, data.EdgeBuffer.PrimitiveCount);
        }
    }

    // Всё остальное (IDirectContext3DServer) оставим без изменений, кроме BoundingBox:

    public Outline GetBoundingBox(View dBView)
    {
        if (solids.Count == 0)
            return new Outline(XYZ.Zero, XYZ.Zero);

        XYZ min = solids[0].Solid.Edges.Cast<Edge>().SelectMany(e => e.Tessellate()).Aggregate((a, b) => new XYZ(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));
        XYZ max = solids[0].Solid.Edges.Cast<Edge>().SelectMany(e => e.Tessellate()).Aggregate((a, b) => new XYZ(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));

        return new Outline(min, max);
    }

    public bool UsesHandles() => false;
    public bool UseInTransparentPass(View view) => true;
    public bool CanExecute(View view) => true;
    public Guid GetServerId() => m_guid;
    public string GetVendorId() => "GenFusions";
    public ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
    public string GetName() => "Cube Screen Server";
    public string GetDescription() => "Draws a screen of cubes";
    public string GetApplicationId() => "";
    public string GetSourceId() => "";
}




//using Autodesk.Revit.DB;
//using Autodesk.Revit.DB.DirectContext3D;
//using Autodesk.Revit.DB.ExternalService;
//using Autodesk.Revit.UI;
//using RevitDoom.RevitDrow;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace RevitDoom.Video;

//public class SolidServer : IDirectContext3DServer, IExternalServer
//{
//    private Guid m_guid;

//    private UIDocument m_uiDocument;

//    private Solid Solid;

//    private SolidFaceBufferStorage SolidFaceBufferStorage;

//    private SolidFaceBufferStorage SolidFaceBufferStorage_Transparent;

//    private SolidEdgeBufferStorage SolidEdgeBufferStorage;

//    private SolidEdgeBufferStorage SolidEdgeBufferStorage_Transparent;

//    private ColorWithTransparency EdgeColor;

//    private ColorWithTransparency FaceColor;

//    private bool UseNormals;

//    public Document Document
//    {
//        get
//        {
//            if (m_uiDocument == null)
//            {
//                return null;
//            }

//            return m_uiDocument.Document;
//        }
//    }

//    public SolidServer(UIDocument uiDoc, Solid solid, bool useNormals = false, ColorWithTransparency faceColor = null, ColorWithTransparency edgeColor = null)
//    {
//        m_guid = Guid.NewGuid();
//        m_uiDocument = uiDoc;
//        Solid = SolidUtils.Clone(solid);
//        FaceColor = faceColor;
//        EdgeColor = edgeColor;
//        UseNormals = useNormals;
//    }

//    public Guid GetServerId()
//    {
//        return m_guid;
//    }

//    public string GetVendorId()
//    {
//        return "GenFusions";
//    }

//    public ExternalServiceId GetServiceId()
//    {
//        return ExternalServices.BuiltInExternalServices.DirectContext3DService;
//    }

//    public string GetName()
//    {
//        return "Line Server";
//    }

//    public string GetDescription()
//    {
//        return "Line Server";
//    }

//    public string GetApplicationId()
//    {
//        return "";
//    }

//    public string GetSourceId()
//    {
//        return "";
//    }

//    public bool UsesHandles()
//    {
//        return false;
//    }

//    public bool UseInTransparentPass(View view)
//    {
//        return true;
//    }

//    public bool CanExecute(View view)
//    {
//        return true;
//    }

//    public Outline GetBoundingBox(View dBView)
//    {
//        List<XYZ> list = new List<XYZ>();
//        foreach (Edge edge in Solid.Edges)
//        {
//            IList<XYZ> collection = edge.Tessellate();
//            list.AddRange(collection);
//        }

//        double num = list.Min((XYZ x) => x.X);
//        double num2 = list.Min((XYZ x) => x.Y);
//        double num3 = list.Min((XYZ x) => x.Z);
//        double num4 = list.Max((XYZ x) => x.X);
//        double num5 = list.Max((XYZ x) => x.Y);
//        double num6 = list.Max((XYZ x) => x.Z);
//        return new Outline(new XYZ(num - 1.0, num2 - 1.0, num3 - 1.0), new XYZ(num4 + 1.0, num5 + 1.0, num6 + 1.0));
//    }

//    public void RenderScene(View dBView, DisplayStyle displayStyle)
//    {
//        try
//        {
//            if (SolidFaceBufferStorage == null || SolidFaceBufferStorage.needsUpdate(displayStyle))
//            {
//                if (!UseNormals)
//                {
//                    SolidFaceBufferStorage = new SolidFaceBufferStorage(displayStyle, Solid);
//                    SolidFaceBufferStorage_Transparent = new SolidFaceBufferStorage(displayStyle, Solid);
//                    SolidFaceBufferStorage.AddVertexPosition(FaceColor);
//                    SolidFaceBufferStorage_Transparent.AddVertexPosition(FaceColor);
//                }
//                else
//                {
//                    SolidFaceBufferStorage = new SolidFaceBufferStorage(displayStyle, Solid);
//                    SolidFaceBufferStorage_Transparent = new SolidFaceBufferStorage(displayStyle, Solid);
//                    SolidFaceBufferStorage.AddVertexPositionNormalColored(FaceColor);
//                    SolidFaceBufferStorage_Transparent.AddVertexPositionNormalColored(FaceColor);
//                }
//            }

//            if (SolidEdgeBufferStorage == null || SolidEdgeBufferStorage.needsUpdate(displayStyle))
//            {
//                SolidEdgeBufferStorage = new SolidEdgeBufferStorage(displayStyle, Solid);
//                SolidEdgeBufferStorage_Transparent = new SolidEdgeBufferStorage(displayStyle, Solid);
//                SolidEdgeBufferStorage.AddVertexPosition(EdgeColor);
//                SolidEdgeBufferStorage_Transparent.AddVertexPosition(EdgeColor);
//            }

//            bool flag = DrawContext.IsTransparentPass();
//            bool flag2 = FaceColor.GetTransparency() != 0;
//            if (!flag && !flag2)
//            {
//                if (SolidFaceBufferStorage.PrimitiveCount > 0 && displayStyle != DisplayStyle.Wireframe)
//                {
//                    DrawContext.FlushBuffer(SolidFaceBufferStorage.VertexBuffer, SolidFaceBufferStorage.VertexBufferCount, SolidFaceBufferStorage.IndexBuffer, SolidFaceBufferStorage.IndexBufferCount, SolidFaceBufferStorage.VertexFormat, SolidFaceBufferStorage.EffectInstance, SolidFaceBufferStorage.BufferPrimitiveType, 0, SolidFaceBufferStorage.PrimitiveCount);
//                }
//            }
//            else if (flag && flag2 && SolidFaceBufferStorage_Transparent.PrimitiveCount > 0 && displayStyle != DisplayStyle.Wireframe)
//            {
//                DrawContext.FlushBuffer(SolidFaceBufferStorage_Transparent.VertexBuffer, SolidFaceBufferStorage_Transparent.VertexBufferCount, SolidFaceBufferStorage_Transparent.IndexBuffer, SolidFaceBufferStorage_Transparent.IndexBufferCount, SolidFaceBufferStorage_Transparent.VertexFormat, SolidFaceBufferStorage_Transparent.EffectInstance, SolidFaceBufferStorage_Transparent.BufferPrimitiveType, 0, SolidFaceBufferStorage_Transparent.PrimitiveCount);
//            }

//            if (SolidEdgeBufferStorage.PrimitiveCount > 0)
//            {
//                DrawContext.FlushBuffer(SolidEdgeBufferStorage.VertexBuffer, SolidEdgeBufferStorage.VertexBufferCount, SolidEdgeBufferStorage.IndexBuffer, SolidEdgeBufferStorage.IndexBufferCount, SolidEdgeBufferStorage.VertexFormat, SolidEdgeBufferStorage.EffectInstance, SolidEdgeBufferStorage.BufferPrimitiveType, 0, SolidEdgeBufferStorage.PrimitiveCount);
//            }
//        }
//        catch (Exception ex)
//        {
//            _ = ex.Message;
//        }
//    }
//}
