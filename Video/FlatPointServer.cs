using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using View = Autodesk.Revit.DB.View;
using RevitDoom.RevitDrow;

public class FlatPointServer : IDirectContext3DServer, IExternalServer
{
    private Guid m_guid = Guid.NewGuid();
    private UIDocument m_uiDocument;

    private List<PointData> points = new();

    public FlatPointServer(UIDocument uiDoc, int width, int height, double cellSize)
    {
        m_uiDocument = uiDoc;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                XYZ position = new XYZ(x * cellSize, y * cellSize, 0);
                var color = new ColorWithTransparency(0, 0, 0, 0); // чёрный

                points.Add(new PointData
                {
                    Position = position,
                    Color = color,
                    Buffer = null
                });
            }
        }
    }

    public void SetPixel(int x, int y, int width, ColorWithTransparency color)
    {
        int index = y * width + x;
        if (index >= 0 && index < points.Count)
        {
            points[index].Color = color;
            points[index].Buffer = null;
        }
    }

    public void SetPixels(byte[] buffer, int width, int height)
    {
        int scaleX = 1;
        int scaleY = 1;
        int targetHeight = height;
        int targetWidth = width;

        int srcHeight = height;

        if (points.Count * 4 == buffer.Length / 4)
        {
            scaleX = 2;
            scaleY = 2;
            targetHeight = height / 2;
            targetWidth = width / 2;
        }
        else if (points.Count * 16 == buffer.Length / 4)
        {
            scaleX = 4;
            scaleY = 4;
            targetHeight = height / 4;
            targetWidth = width / 4;
        }

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = x * scaleX;
                int srcY = y * scaleY;

                int i = (srcX * srcHeight + srcY) * 4; // Doom: column-major

                if (i + 2 >= buffer.Length) continue;

                byte r = buffer[i + 0];
                byte g = buffer[i + 1];
                byte b = buffer[i + 2];

                var color = new ColorWithTransparency(r, g, b, 0);
                SetPixel(x, targetHeight - y - 1, targetWidth, color); // Y инвертирован
            }
        }
    }

    public void RenderScene(View view, DisplayStyle style)
    {
        try
        {
            foreach (var pt in points)
            {
                if (pt.Buffer == null)
                {
                    var buffer = new PointBufferStorage(style);
                    buffer.AddVertex(pt.Position, pt.Color);
                    buffer.AddVertexPosition(pt.Color);
                    pt.Buffer = buffer;
                }

                DrawContext.FlushBuffer(
                    pt.Buffer.VertexBuffer,
                    pt.Buffer.VertexBufferCount,
                    pt.Buffer.IndexBuffer,
                    0,
                    pt.Buffer.VertexFormat,
                    pt.Buffer.EffectInstance,
                    PrimitiveType.PointList,
                    0,
                    pt.Buffer.VertexBufferCount
                );
            }
        }
        catch { }
    }

    public Outline GetBoundingBox(View view)
    {
        if (points.Count == 0) return new Outline(XYZ.Zero, XYZ.Zero);

        XYZ min = points.Select(p => p.Position).Aggregate((a, b) =>
            new XYZ(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));
        XYZ max = points.Select(p => p.Position).Aggregate((a, b) =>
            new XYZ(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));

        return new Outline(min, max);
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

    public bool UsesHandles() => false;
    public bool UseInTransparentPass(View view) => false;
    public bool CanExecute(View view) => true;
    public Guid GetServerId() => m_guid;
    public string GetVendorId() => "GenFusions";
    public ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
    public string GetName() => "Flat Point Server";
    public string GetDescription() => "Draws pixels as fast points";
    public string GetApplicationId() => "";
    public string GetSourceId() => "";

    private class PointData
    {
        public XYZ Position;
        public ColorWithTransparency Color;
        public PointBufferStorage Buffer;
    }
}
