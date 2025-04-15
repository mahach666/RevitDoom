using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using RevitDoom.RevitDrow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDoom.Video;

public class SolidServer : IDirectContext3DServer, IExternalServer
{
    private Guid m_guid;

    private UIDocument m_uiDocument;

    private Solid Solid;

    private SolidFaceBufferStorage SolidFaceBufferStorage;

    private SolidFaceBufferStorage SolidFaceBufferStorage_Transparent;

    private SolidEdgeBufferStorage SolidEdgeBufferStorage;

    private SolidEdgeBufferStorage SolidEdgeBufferStorage_Transparent;

    private ColorWithTransparency EdgeColor;

    private ColorWithTransparency FaceColor;

    private bool UseNormals;

    public Document Document
    {
        get
        {
            if (m_uiDocument == null)
            {
                return null;
            }

            return m_uiDocument.Document;
        }
    }

    public SolidServer(UIDocument uiDoc, Solid solid, bool useNormals = false, ColorWithTransparency faceColor = null, ColorWithTransparency edgeColor = null)
    {
        m_guid = Guid.NewGuid();
        m_uiDocument = uiDoc;
        Solid = SolidUtils.Clone(solid);
        FaceColor = faceColor;
        EdgeColor = edgeColor;
        UseNormals = useNormals;
    }

    public Guid GetServerId()
    {
        return m_guid;
    }

    public string GetVendorId()
    {
        return "GenFusions";
    }

    public ExternalServiceId GetServiceId()
    {
        return ExternalServices.BuiltInExternalServices.DirectContext3DService;
    }

    public string GetName()
    {
        return "Line Server";
    }

    public string GetDescription()
    {
        return "Line Server";
    }

    public string GetApplicationId()
    {
        return "";
    }

    public string GetSourceId()
    {
        return "";
    }

    public bool UsesHandles()
    {
        return false;
    }

    public bool UseInTransparentPass(View view)
    {
        return true;
    }

    public bool CanExecute(View view)
    {
        return true;
    }

    public Outline GetBoundingBox(View dBView)
    {
        List<XYZ> list = new List<XYZ>();
        foreach (Edge edge in Solid.Edges)
        {
            IList<XYZ> collection = edge.Tessellate();
            list.AddRange(collection);
        }

        double num = list.Min((XYZ x) => x.X);
        double num2 = list.Min((XYZ x) => x.Y);
        double num3 = list.Min((XYZ x) => x.Z);
        double num4 = list.Max((XYZ x) => x.X);
        double num5 = list.Max((XYZ x) => x.Y);
        double num6 = list.Max((XYZ x) => x.Z);
        return new Outline(new XYZ(num - 1.0, num2 - 1.0, num3 - 1.0), new XYZ(num4 + 1.0, num5 + 1.0, num6 + 1.0));
    }

    public void RenderScene(View dBView, DisplayStyle displayStyle)
    {
        try
        {
            if (SolidFaceBufferStorage == null || SolidFaceBufferStorage.needsUpdate(displayStyle))
            {
                if (!UseNormals)
                {
                    SolidFaceBufferStorage = new SolidFaceBufferStorage(displayStyle, Solid);
                    SolidFaceBufferStorage_Transparent = new SolidFaceBufferStorage(displayStyle, Solid);
                    SolidFaceBufferStorage.AddVertexPosition(FaceColor);
                    SolidFaceBufferStorage_Transparent.AddVertexPosition(FaceColor);
                }
                else
                {
                    SolidFaceBufferStorage = new SolidFaceBufferStorage(displayStyle, Solid);
                    SolidFaceBufferStorage_Transparent = new SolidFaceBufferStorage(displayStyle, Solid);
                    SolidFaceBufferStorage.AddVertexPositionNormalColored(FaceColor);
                    SolidFaceBufferStorage_Transparent.AddVertexPositionNormalColored(FaceColor);
                }
            }

            if (SolidEdgeBufferStorage == null || SolidEdgeBufferStorage.needsUpdate(displayStyle))
            {
                SolidEdgeBufferStorage = new SolidEdgeBufferStorage(displayStyle, Solid);
                SolidEdgeBufferStorage_Transparent = new SolidEdgeBufferStorage(displayStyle, Solid);
                SolidEdgeBufferStorage.AddVertexPosition(EdgeColor);
                SolidEdgeBufferStorage_Transparent.AddVertexPosition(EdgeColor);
            }

            bool flag = DrawContext.IsTransparentPass();
            bool flag2 = FaceColor.GetTransparency() != 0;
            if (!flag && !flag2)
            {
                if (SolidFaceBufferStorage.PrimitiveCount > 0 && displayStyle != DisplayStyle.Wireframe)
                {
                    DrawContext.FlushBuffer(SolidFaceBufferStorage.VertexBuffer, SolidFaceBufferStorage.VertexBufferCount, SolidFaceBufferStorage.IndexBuffer, SolidFaceBufferStorage.IndexBufferCount, SolidFaceBufferStorage.VertexFormat, SolidFaceBufferStorage.EffectInstance, SolidFaceBufferStorage.BufferPrimitiveType, 0, SolidFaceBufferStorage.PrimitiveCount);
                }
            }
            else if (flag && flag2 && SolidFaceBufferStorage_Transparent.PrimitiveCount > 0 && displayStyle != DisplayStyle.Wireframe)
            {
                DrawContext.FlushBuffer(SolidFaceBufferStorage_Transparent.VertexBuffer, SolidFaceBufferStorage_Transparent.VertexBufferCount, SolidFaceBufferStorage_Transparent.IndexBuffer, SolidFaceBufferStorage_Transparent.IndexBufferCount, SolidFaceBufferStorage_Transparent.VertexFormat, SolidFaceBufferStorage_Transparent.EffectInstance, SolidFaceBufferStorage_Transparent.BufferPrimitiveType, 0, SolidFaceBufferStorage_Transparent.PrimitiveCount);
            }

            if (SolidEdgeBufferStorage.PrimitiveCount > 0)
            {
                DrawContext.FlushBuffer(SolidEdgeBufferStorage.VertexBuffer, SolidEdgeBufferStorage.VertexBufferCount, SolidEdgeBufferStorage.IndexBuffer, SolidEdgeBufferStorage.IndexBufferCount, SolidEdgeBufferStorage.VertexFormat, SolidEdgeBufferStorage.EffectInstance, SolidEdgeBufferStorage.BufferPrimitiveType, 0, SolidEdgeBufferStorage.PrimitiveCount);
            }
        }
        catch (Exception ex)
        {
            _ = ex.Message;
        }
    }
}
