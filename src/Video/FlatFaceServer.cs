using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using RevitDoom.Contracts;
using RevitDoom.RevitDrow;
using System;
using System.Collections.Generic;
using System.Linq;
using View = Autodesk.Revit.DB.View;

public class FlatFaceServer : CastomDirectContextServer
{
    private Guid m_guid = Guid.NewGuid();

    private List<FaceData> faces = new();

    public FlatFaceServer( int width, int height, double cellSize)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                XYZ origin = new XYZ(0, x * cellSize, y * cellSize);

                MeshData mesh = CreateMetaData(origin, cellSize);
                var color = new ColorWithTransparency(0, 0, 0, 0);

                faces.Add(new FaceData
                {
                    Mesh = mesh,
                    FaceColor = color,
                });
            }
        }
    }

    public override void SetPixel(int x, int y, int width, ColorWithTransparency color)
    {
        int index = y * width + x;
        if (index >= 0 && index < faces.Count)
        {
            faces[index].FaceColor = color;
        }
    }

    public override void SetPixels(byte[] buffer, int width, int height)
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
        else if (faces.Count * 64 == buffer.Length / 4)
        {
            scaleX = 8;
            scaleY = 8;
            targetHeight = height / 8;
            targetWidth = width / 8;
        }

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = x * scaleX;
                int srcY = y * scaleY;

                int i = (srcX * srcHeight + srcY) * 4; 

                if (i + 2 >= buffer.Length) continue;

                byte r = buffer[i + 0];
                byte g = buffer[i + 1];
                byte b = buffer[i + 2];

                var color = new ColorWithTransparency(r, g, b, 0);
                SetPixel(x, targetHeight - y - 1, targetWidth, color); 
            }
        }
    }

    private  MeshData CreateMetaData(XYZ origin, double size)
    {
        size = size * 2;
        var mesh = new MeshData();

        XYZ p0 = origin;
        XYZ p1 = origin + new XYZ(0, 0, size);
        XYZ p2 = origin + new XYZ(0, size, size);
        XYZ p3 = origin + new XYZ(0, size, 0);

        mesh.Vertices.Add(p0); 
        mesh.Vertices.Add(p1); 
        mesh.Vertices.Add(p2); 
        mesh.Vertices.Add(p3); 

        mesh.Triangles.Add(new IndexTriangle(0, 2, 3));

        return mesh;
    }

    public override void RenderScene(View view, DisplayStyle style)
    {

        try
        {
            foreach (var data in faces)
            {
                if (data.FaceBuffer == null)
                {
                    data.FaceBuffer = new MeshFaceBufferStorage(style, data.Mesh);
                    data.FaceBuffer.AddVertexPositionNormalColored(data.FaceColor);
                }
                else
                {
                    data.FaceBuffer.UpdateVertexColors(data.FaceColor);
                }


                DrawContext.FlushBuffer(data.FaceBuffer.VertexBuffer
                    , data.FaceBuffer.VertexBufferCount,
                    data.FaceBuffer.IndexBuffer
                    , data.FaceBuffer.IndexBufferCount
                    , data.FaceBuffer.VertexFormat,
                    data.FaceBuffer.EffectInstance
                    , data.FaceBuffer.BufferPrimitiveType
                    , 0
                    , data.FaceBuffer.PrimitiveCount);
            }
        }
        catch
        {

        }
    }

    public override Outline GetBoundingBox(View dBView)
    {
        if (faces.Count == 0) return new Outline(XYZ.Zero, XYZ.Zero);

        XYZ min = faces[0].Mesh.Vertices.Cast<XYZ>().Aggregate((a, b) => new XYZ(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));
        XYZ max = faces[0].Mesh.Vertices.Cast<XYZ>().Aggregate((a, b) => new XYZ(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));

        return new Outline(min, max);
    }

    public override bool UsesHandles() => false;
    public override bool UseInTransparentPass(View view) => false;
    public override bool CanExecute(View view) => true;
    public override Guid GetServerId() => m_guid;
    public override string GetVendorId() => "DOOM";
    public override ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
    public override string GetName() => "DOOM Screen Server";
    public override string GetDescription() => "Draws DOOM in triangles";
    public override string GetApplicationId() => "";
    public override string GetSourceId() => "";
}
