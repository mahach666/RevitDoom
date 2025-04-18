using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System;
using RevitDoom.RevitDrow;
using System.Linq;
using System.Windows.Forms;
using View = Autodesk.Revit.DB.View;

public class FlatFaceServer : IDirectContext3DServer, IExternalServer
{
    private Guid m_guid = Guid.NewGuid();

    private UIDocument m_uiDocument;

    private List<FaceData> faces = new();

    public FlatFaceServer(UIDocument uiDoc, int width, int height, double cellSize)
    {
        m_uiDocument = uiDoc;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                XYZ origin = new XYZ(x * cellSize, y * cellSize, 0);

                MeshData mesh = CreateQuad(origin, cellSize);
                var color = new ColorWithTransparency(0, 0, 0, 0); // чёрный непрозрачный
                //var edgeColor = new ColorWithTransparency(255, 255, 255, 0); // белые края

                faces.Add(new FaceData
                {
                    Mesh = mesh,
                    FaceColor = color,
                    //EdgeColor = edgeColor
                });
            }
        }
    }

    public void SetPixel(int x, int y, int width, ColorWithTransparency color)
    {
        int index = y * width + x;
        if (index >= 0 && index < faces.Count)
        {
            faces[index].FaceColor = color;
            //faces[index].EdgeColor = color;
            faces[index].FaceBuffer = null;
            //faces[index].EdgeBuffer = null;
        }
    }

    //public void SetPixels(byte[] buffer, int width, int height)
    //{
    //    int stride = 4; // BGRA
    //    for (int y = 0; y < height; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            int index = (y * width + x) * stride;
    //            if (index + 3 >= buffer.Length) continue;

    //            byte b = buffer[index + 0];
    //            byte g = buffer[index + 1];
    //            byte r = buffer[index + 2];
    //            byte a = buffer[index + 3];

    //            var color = new ColorWithTransparency(r, g, b, (byte)(255 - a));
    //            SetPixel(x, height - y - 1, width, color); // y перевернутый
    //        }
    //    }
    //}


    public void SetPixels(byte[] buffer, int width, int height)
    {
        int scaleX = 1;
        int scaleY = 1;
        int targetHeight = height;
        int targetWidth = width;

        int srcHeight = height;

        if (faces.Count * 4 == buffer.Length / 4)
        {
            scaleX = 2;
            scaleY = 2;
            targetHeight = height / 2;
            targetWidth = width / 2;
        }
        else if (faces.Count * 16 == buffer.Length / 4)
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


    //public void SetPixels(byte[] buffer, int width, int height)
    //{
    //    for (int y = 0; y < height; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            // Doom отдаёт данные "по столбцам", а не по строкам
    //            int i = (x * height + y) * 4;
    //            if (i + 2 >= buffer.Length) continue;

    //            byte r = buffer[i + 0];
    //            byte g = buffer[i + 1];
    //            byte b = buffer[i + 2];
    //            // byte a = buffer[i + 3]; // Можно игнорировать или использовать

    //            var color = new ColorWithTransparency(r, g, b, 0);

    //            SetPixel(x, height - y - 1, width, color); // Y инвертируем для Revit
    //        }
    //    }
    //}



    private MeshData CreateQuad(XYZ origin, double size)
    {
        var mesh = new MeshData();

        XYZ p0 = origin;
        XYZ p1 = origin + new XYZ(size, 0, 0);
        XYZ p2 = origin + new XYZ(size, size, 0);
        XYZ p3 = origin + new XYZ(0, size, 0);

        mesh.Vertices.Add(p0); // 0
        mesh.Vertices.Add(p1); // 1
        mesh.Vertices.Add(p2); // 2
        mesh.Vertices.Add(p3); // 3

        mesh.Triangles.Add(new IndexTriangle(0, 1, 2));
        mesh.Triangles.Add(new IndexTriangle(0, 2, 3));

        return mesh;
    }


    public void RenderScene(View view, DisplayStyle style)
    {
        try
        {
            //var count = 1;
            foreach (var data in faces)
            {
                if (data.FaceBuffer == null)
                {
                    data.FaceBuffer = new MeshFaceBufferStorage(style, data.Mesh);
                    data.FaceBuffer.AddVertexPositionNormalColored(data.FaceColor);
                }

                //if (data.EdgeBuffer == null)
                //{
                //    data.EdgeBuffer = new MeshEdgeBufferStorage(style, data.Mesh);
                //    data.EdgeBuffer.AddVertexPosition(data.EdgeColor);
                //}

                DrawContext.FlushBuffer(data.FaceBuffer.VertexBuffer
                    , data.FaceBuffer.VertexBufferCount,
                    data.FaceBuffer.IndexBuffer
                    , data.FaceBuffer.IndexBufferCount
                    , data.FaceBuffer.VertexFormat,
                    data.FaceBuffer.EffectInstance
                    , data.FaceBuffer.BufferPrimitiveType
                    , 0
                    , data.FaceBuffer.PrimitiveCount);

                //DrawContext.FlushBuffer(data.EdgeBuffer.VertexBuffer, data.EdgeBuffer.VertexBufferCount,
                //    data.EdgeBuffer.IndexBuffer, data.EdgeBuffer.IndexBufferCount, data.EdgeBuffer.VertexFormat,
                //    data.EdgeBuffer.EffectInstance, data.EdgeBuffer.BufferPrimitiveType, 0,
                //    data.EdgeBuffer.PrimitiveCount);
                //count++;
            }
            //MessageBox.Show("RenderScene");
        }
        catch (Exception e)
        {
            //MessageBox.Show(e.Message);

        }
        finally
        {

        }
    }

    public Outline GetBoundingBox(View dBView)
    {
        if (faces.Count == 0) return new Outline(XYZ.Zero, XYZ.Zero);

        XYZ min = faces[0].Mesh.Vertices.Cast<XYZ>().Aggregate((a, b) => new XYZ(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));
        XYZ max = faces[0].Mesh.Vertices.Cast<XYZ>().Aggregate((a, b) => new XYZ(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));

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
    public string GetName() => "Flat Screen Server";
    public string GetDescription() => "Draws flat quads instead of cubes";
    public string GetApplicationId() => "";
    public string GetSourceId() => "";
}
