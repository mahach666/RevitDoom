using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using System;
using System.Collections.Generic;

namespace RevitDoom.RevitDrow;

public class MeshEdgeBufferStorage : BaseBufferStorage
{
    public readonly List<VertexPosition> Vertices = new();
    public readonly List<short> Indices = new();

    public MeshData Mesh;

    public MeshEdgeBufferStorage(DisplayStyle displayStyle, MeshData meshData)
        : base(displayStyle)
    {
        Mesh = meshData;
        DisplayStyle = displayStyle;
        BufferPrimitiveType = PrimitiveType.LineList;

        // Добавим рёбра — все уникальные
        var edgeSet = new HashSet<(XYZ, XYZ)>(new EdgeComparer());

        foreach (var tri in meshData.Triangles)
        {
            var v0 = meshData.Vertices[tri.Index0];
            var v1 = meshData.Vertices[tri.Index1];
            var v2 = meshData.Vertices[tri.Index2];

            edgeSet.Add(NormalizeEdge(v0, v1));
            edgeSet.Add(NormalizeEdge(v1, v2));
            edgeSet.Add(NormalizeEdge(v2, v0));
        }

        short index = 0;
        foreach (var (start, end) in edgeSet)
        {
            Vertices.Add(new VertexPosition(start));
            Vertices.Add(new VertexPosition(end));
            Indices.Add(index++);
            Indices.Add(index++);
        }

        VertexBufferCount = Vertices.Count;
        PrimitiveCount = Indices.Count / 2;
    }

    public override void AddVertexPosition(ColorWithTransparency color)
    {
        FormatBits = VertexFormatBits.Position;
        VertexFormat = new VertexFormat(FormatBits);
        EffectInstance = new EffectInstance(FormatBits);

        EffectInstance.SetColor(color.GetColor());
        EffectInstance.SetTransparency((double)color.GetTransparency() / 255.0);

        if (DisplayStyle == DisplayStyle.HLR)
        {
            EffectInstance.SetSpecularColor(color.GetColor());
            EffectInstance.SetAmbientColor(color.GetColor());
            EffectInstance.SetEmissiveColor(color.GetColor());
        }

        int sizeInFloats = VertexPosition.GetSizeInFloats() * VertexBufferCount;
        VertexBuffer = new VertexBuffer(sizeInFloats);
        VertexBuffer.Map(sizeInFloats);
        VertexBuffer.GetVertexStreamPosition().AddVertices(Vertices);
        VertexBuffer.Unmap();

        IndexBufferCount = Indices.Count;
        IndexBuffer = new IndexBuffer(IndexBufferCount);
        IndexBuffer.Map(IndexBufferCount);
        //IndexBuffer.GetIndexStreamLine().AddLine(Indices.ToArray());
        IndexStreamLine stream = IndexBuffer.GetIndexStreamLine();
        for (int i = 0; i < Indices.Count; i += 2)
        {
            stream.AddLine(new IndexLine(Indices[i], Indices[i + 1]));
        }

        IndexBuffer.Unmap();
    }

    public override void AddVertexPositionNormalColored(ColorWithTransparency color)
    {
        throw new NotImplementedException();
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

    private static (XYZ, XYZ) NormalizeEdge(XYZ a, XYZ b)
    {
        // Для хеширования и уникальности
        return (a.IsAlmostEqualTo(b)) ? (a, b) : (b, a);
    }

    private class EdgeComparer : IEqualityComparer<(XYZ, XYZ)>
    {
        public bool Equals((XYZ, XYZ) e1, (XYZ, XYZ) e2)
        {
            return (e1.Item1.IsAlmostEqualTo(e2.Item1) && e1.Item2.IsAlmostEqualTo(e2.Item2)) ||
                   (e1.Item1.IsAlmostEqualTo(e2.Item2) && e1.Item2.IsAlmostEqualTo(e2.Item1));
        }

        public int GetHashCode((XYZ, XYZ) edge)
        {
            return edge.Item1.GetHashCode() ^ edge.Item2.GetHashCode();
        }
    }
}
