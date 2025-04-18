using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using System;
using System.Collections.Generic;

namespace RevitDoom.RevitDrow;

public class MeshFaceBufferStorage : BaseBufferStorage
{
    public readonly List<IndexTriangle> IndexTriangles = new();
    public readonly List<Tuple<XYZ, XYZ>> VerticesWithNormals = new();
    public MeshData Mesh;


    public MeshFaceBufferStorage(DisplayStyle displayStyle, MeshData meshData)
        : base(displayStyle)
    {
        BufferPrimitiveType = PrimitiveType.TriangleList;
        DisplayStyle = displayStyle;
        Mesh = meshData;
        PrimitiveCount = meshData.Triangles.Count;

        int vertexOffset = 0;
        for (int i = 0; i < meshData.Triangles.Count; i++)
        {
            var tri = meshData.Triangles[i];
            var v0 = meshData.Vertices[tri.Index0];
            var v1 = meshData.Vertices[tri.Index1];
            var v2 = meshData.Vertices[tri.Index2];

            // Нормаль треугольника
            XYZ normal = (v1 - v0).CrossProduct(v2 - v0).Normalize();

            VerticesWithNormals.Add(new Tuple<XYZ, XYZ>(v0, normal));
            VerticesWithNormals.Add(new Tuple<XYZ, XYZ>(v1, normal));
            VerticesWithNormals.Add(new Tuple<XYZ, XYZ>(v2, normal));

            IndexTriangles.Add(new IndexTriangle(vertexOffset, vertexOffset + 1, vertexOffset + 2));
            vertexOffset += 3;
        }

        VertexBufferCount = VerticesWithNormals.Count;
    }

    public override void AddVertexPositionNormalColored(ColorWithTransparency color)
    {
        FormatBits = VertexFormatBits.PositionNormalColored;
        VertexFormat = new VertexFormat(FormatBits);
        EffectInstance = new EffectInstance(FormatBits);

        EffectInstance.SetColor(color.GetColor());
        EffectInstance.SetDiffuseColor(color.GetColor());
        EffectInstance.SetTransparency((double)color.GetTransparency() / 255.0);
        if (DisplayStyle == DisplayStyle.HLR)
        {
            EffectInstance.SetSpecularColor(color.GetColor());
            EffectInstance.SetAmbientColor(color.GetColor());
            EffectInstance.SetEmissiveColor(color.GetColor());
        }

        int sizeInFloats = VertexPositionNormalColored.GetSizeInFloats() * VertexBufferCount;
        VertexBuffer = new VertexBuffer(sizeInFloats);
        VertexBuffer.Map(sizeInFloats);
        VertexStreamPositionNormalColored stream = VertexBuffer.GetVertexStreamPositionNormalColored();

        foreach (var (position, normal) in VerticesWithNormals)
        {
            stream.AddVertex(new VertexPositionNormalColored(position, normal, color));
        }

        VertexBuffer.Unmap();

        IndexBufferCount = PrimitiveCount * IndexTriangle.GetSizeInShortInts();
        IndexBuffer = new IndexBuffer(IndexBufferCount);
        IndexBuffer.Map(IndexBufferCount);
        IndexStreamTriangle indexStream = IndexBuffer.GetIndexStreamTriangle();
        foreach (var tri in IndexTriangles)
        {
            indexStream.AddTriangle(tri);
        }

        IndexBuffer.Unmap();
    }
    //public override void AddVertexPositionNormalColored(ColorWithTransparency color)
    //{
    //    FormatBits = VertexFormatBits.PositionNormalColored;
    //    VertexFormat = new VertexFormat(FormatBits);
    //    EffectInstance = new EffectInstance(FormatBits);

    //    // ... ваш существующий код настройки EffectInstance ...

    //    int sizeInFloats = VertexPositionNormalColored.GetSizeInFloats() * VertexBufferCount;
    //    VertexBuffer = new VertexBuffer(sizeInFloats);
    //    VertexBuffer.Map(sizeInFloats);
    //    var stream = VertexBuffer.GetVertexStreamPositionNormalColored();

    //    // ⬇️ 1. формируем временные списки, чтобы потом сохранить их в SourceVertices
    //    var tempVerts = new List<VertexPositionNormalColored>();
    //    var tempIndices = new List<int>();

    //    foreach (var (position, normal) in VerticesWithNormals)
    //    {
    //        var v = new VertexPositionNormalColored(position, normal, color);
    //        stream.AddVertex(v);
    //        tempVerts.Add(v);
    //    }
    //    VertexBuffer.Unmap();

    //    IndexBufferCount = PrimitiveCount * IndexTriangle.GetSizeInShortInts();
    //    IndexBuffer = new IndexBuffer(IndexBufferCount);
    //    IndexBuffer.Map(IndexBufferCount);
    //    var indexStream = IndexBuffer.GetIndexStreamTriangle();

    //    foreach (var tri in IndexTriangles)
    //    {
    //        indexStream.AddTriangle(tri);
    //        // ⬇️ 2. сохраняем индексы как int
    //        tempIndices.Add(tri.Index0);
    //        tempIndices.Add(tri.Index1);
    //        tempIndices.Add(tri.Index2);
    //    }
    //    IndexBuffer.Unmap();

    //    // ⬇️ 3. НАКОНЕЦ‑ТО запоминаем исходные данные для TryGetData
    //    SourceVertices = tempVerts.ToArray();
    //    SourceIndices = tempIndices.ToArray();
    //}

    //public  void UpdateVertexColors(ColorWithTransparency color)
    //{
    //    if (VertexBuffer == null || VertexBufferCount == 0) return;

    //    // 1) обновляем кеш в памяти – это главное для ScreenFaceBufferStorage
    //    for (int i = 0; i < SourceVertices.Length; i++)
    //    {
    //        var v = SourceVertices[i];
    //        SourceVertices[i] = new VertexPositionNormalColored(v.Position, v.Normal, color);
    //    }

    //    // 2) перекладываем в Revit‑буфер, чтобы одиночные Flush() тоже имели правильные цвета
    //    VertexBuffer.Map(VertexPositionNormalColored.GetSizeInFloats() * VertexBufferCount);
    //    var stream = VertexBuffer.GetVertexStreamPositionNormalColored();
    //    foreach (var vtx in SourceVertices)
    //        stream.AddVertex(vtx);
    //    VertexBuffer.Unmap();

    //    // 3) обновляем EffectInstance – влияет на освещение в HLR
    //    EffectInstance.SetColor(color.GetColor());
    //    EffectInstance.SetDiffuseColor(color.GetColor());
    //    EffectInstance.SetTransparency((double)color.GetTransparency() / 255.0);
    //    if (DisplayStyle == DisplayStyle.HLR)
    //    {
    //        EffectInstance.SetSpecularColor(color.GetColor());
    //        EffectInstance.SetAmbientColor(color.GetColor());
    //        EffectInstance.SetEmissiveColor(color.GetColor());
    //    }
    //}


    public void UpdateVertexColors(ColorWithTransparency color)
    {
        if (VertexBuffer == null || VertexBufferCount == 0)
            return;

        VertexBuffer.Map(VertexPositionNormalColored.GetSizeInFloats() * VertexBufferCount);

        var stream = VertexBuffer.GetVertexStreamPositionNormalColored();

        foreach (var (position, normal) in VerticesWithNormals)
        {
            stream.AddVertex(new VertexPositionNormalColored(position, normal, color));
        }

        VertexBuffer.Unmap();

        // Также обновим цвета в EffectInstance
        EffectInstance.SetColor(color.GetColor());
        EffectInstance.SetDiffuseColor(color.GetColor());
        EffectInstance.SetTransparency((double)color.GetTransparency() / 255.0);

        if (DisplayStyle == DisplayStyle.HLR)
        {
            EffectInstance.SetSpecularColor(color.GetColor());
            EffectInstance.SetAmbientColor(color.GetColor());
            EffectInstance.SetEmissiveColor(color.GetColor());
        }
    }

    public VertexPositionNormalColored[] SourceVertices { get; private set; }
    public int[] SourceIndices { get; private set; }

    public bool TryGetData(out VertexPositionNormalColored[] vertices, out int[] indices)
    {
        vertices = SourceVertices;
        indices = SourceIndices;
        return vertices != null && indices != null;
    }



    public override void AddVertexPosition()
    {
        throw new NotImplementedException();
    }

    public override void AddVertexPositionColored(ColorWithTransparency color)
    {
        throw new NotImplementedException();
    }

    public override void AddVertexPositionNormal()
    {
        throw new NotImplementedException();
    }

    public override void AddVertexPosition(ColorWithTransparency color)
    {
        throw new NotImplementedException();
    }

}
