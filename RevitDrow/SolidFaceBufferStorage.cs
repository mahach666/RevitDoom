using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using System;
using System.Collections.Generic;

namespace RevitDoom.RevitDrow;

internal class SolidFaceBufferStorage : BaseBufferStorage
{
    public readonly List<IndexTriangle> IndexTriangles;

    public readonly List<VertexPosition> Vertices;

    public readonly List<Tuple<XYZ, XYZ>> VerticesWithNormals;

    public Solid Solid;

    public FaceArray FaceArray;

    public SolidFaceBufferStorage(DisplayStyle displayStyle, Solid solid)
        : base(displayStyle)
    {
        base.BufferPrimitiveType = PrimitiveType.TriangleList;
        Solid = solid;
        base.DisplayStyle = displayStyle;
        IndexTriangles = new List<IndexTriangle>();
        Vertices = new List<VertexPosition>();
        VerticesWithNormals = new List<Tuple<XYZ, XYZ>>();
        int num = 0;
        FaceArray = solid.Faces;
        foreach (Face item in FaceArray)
        {
            Mesh mesh = item.Triangulate();
            XYZ source = item.ComputeNormal(new UV(0.5, 0.5));
            base.PrimitiveCount += mesh.NumTriangles;
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                MeshTriangle meshTriangle = mesh.get_Triangle(i);
                XYZ xYZ = meshTriangle.get_Vertex(0);
                XYZ xYZ2 = meshTriangle.get_Vertex(1);
                XYZ xYZ3 = meshTriangle.get_Vertex(2);
                XYZ xYZ4 = (xYZ2 - xYZ).CrossProduct(xYZ3 - xYZ).Normalize();
                if (xYZ4.DotProduct(source) < 0.0)
                {
                    xYZ4 = xYZ4.Negate();
                }

                Vertices.Add(new VertexPosition(xYZ));
                Vertices.Add(new VertexPosition(xYZ2));
                Vertices.Add(new VertexPosition(xYZ3));
                VerticesWithNormals.Add(new Tuple<XYZ, XYZ>(xYZ, xYZ4));
                VerticesWithNormals.Add(new Tuple<XYZ, XYZ>(xYZ2, xYZ4));
                VerticesWithNormals.Add(new Tuple<XYZ, XYZ>(xYZ3, xYZ4));
                base.VertexBufferCount += 3;
                IndexTriangles.Add(new IndexTriangle(num, num + 1, num + 2));
                num += 3;
            }
        }
    }

    //
    // Сводка:
    //     Effect instance is not considered.
    public override void AddVertexPosition(ColorWithTransparency color)
    {
        base.FormatBits = VertexFormatBits.Position;
        base.VertexFormat = new VertexFormat(base.FormatBits);
        base.EffectInstance = new EffectInstance(base.FormatBits);
        base.EffectInstance.SetColor(color.GetColor());
        base.EffectInstance.SetDiffuseColor(color.GetColor());
        base.EffectInstance.SetTransparency((double)color.GetTransparency() / 255.0);
        if (base.DisplayStyle == DisplayStyle.HLR)
        {
            base.EffectInstance.SetSpecularColor(color.GetColor());
            base.EffectInstance.SetAmbientColor(color.GetColor());
            base.EffectInstance.SetEmissiveColor(color.GetColor());
        }

        int sizeInFloats = VertexPosition.GetSizeInFloats() * base.VertexBufferCount;
        base.VertexBuffer = new VertexBuffer(sizeInFloats);
        base.VertexBuffer.Map(sizeInFloats);
        base.VertexBuffer.GetVertexStreamPosition().AddVertices(Vertices);
        base.VertexBuffer.Unmap();
        base.IndexBufferCount = base.PrimitiveCount * IndexTriangle.GetSizeInShortInts();
        int indexBufferCount = base.IndexBufferCount;
        base.IndexBuffer = new IndexBuffer(indexBufferCount);
        base.IndexBuffer.Map(indexBufferCount);
        base.IndexBuffer.GetIndexStreamTriangle().AddTriangles(IndexTriangles);
        base.IndexBuffer.Unmap();
    }

    //
    // Сводка:
    //     Effect instance is not considered.
    public override void AddVertexPositionNormalColored(ColorWithTransparency color)
    {
        base.FormatBits = VertexFormatBits.PositionNormalColored;
        base.VertexFormat = new VertexFormat(base.FormatBits);
        base.EffectInstance = new EffectInstance(base.FormatBits);
        base.EffectInstance.SetColor(color.GetColor());
        base.EffectInstance.SetDiffuseColor(color.GetColor());
        base.EffectInstance.SetTransparency((double)color.GetTransparency() / 255.0);
        if (base.DisplayStyle == DisplayStyle.HLR)
        {
            base.EffectInstance.SetSpecularColor(color.GetColor());
            base.EffectInstance.SetAmbientColor(color.GetColor());
            base.EffectInstance.SetEmissiveColor(color.GetColor());
        }

        int sizeInFloats = VertexPositionNormalColored.GetSizeInFloats() * base.VertexBufferCount;
        base.VertexBuffer = new VertexBuffer(sizeInFloats);
        base.VertexBuffer.Map(sizeInFloats);
        VertexStreamPositionNormalColored vertexStreamPositionNormalColored = base.VertexBuffer.GetVertexStreamPositionNormalColored();
        foreach (Tuple<XYZ, XYZ> verticesWithNormal in VerticesWithNormals)
        {
            vertexStreamPositionNormalColored.AddVertex(new VertexPositionNormalColored(verticesWithNormal.Item1, verticesWithNormal.Item2, color));
        }

        base.VertexBuffer.Unmap();
        base.IndexBufferCount = base.PrimitiveCount * IndexTriangle.GetSizeInShortInts();
        int indexBufferCount = base.IndexBufferCount;
        base.IndexBuffer = new IndexBuffer(indexBufferCount);
        base.IndexBuffer.Map(indexBufferCount);
        IndexStreamTriangle indexStreamTriangle = base.IndexBuffer.GetIndexStreamTriangle();
        foreach (IndexTriangle indexTriangle in IndexTriangles)
        {
            indexStreamTriangle.AddTriangle(indexTriangle);
        }

        base.IndexBuffer.Unmap();
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
}
