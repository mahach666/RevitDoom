using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDoom.RevitDrow;

public class SolidEdgeBufferStorage : BaseBufferStorage
{
    public readonly List<IndexLine> IndexLines;

    public readonly List<VertexPosition> Vertices;

    public Solid Solid;

    public EdgeArray EdgeArray;

    public SolidEdgeBufferStorage(DisplayStyle displayStyle, Solid solid)
        : base(displayStyle)
    {
        base.BufferPrimitiveType = PrimitiveType.LineList;
        Solid = solid;
        base.DisplayStyle = displayStyle;
        IndexLines = new List<IndexLine>();
        Vertices = new List<VertexPosition>();
        int num = 0;
        EdgeArray = solid.Edges;
        foreach (Edge item in EdgeArray)
        {
            List<XYZ> list = item.Tessellate().ToList();
            List<VertexPosition> collection = (from XYZ p in list
                                               select new VertexPosition(p)).ToList();
            Vertices.AddRange(collection);
            base.PrimitiveCount += list.Count - 1;
            base.VertexBufferCount += list.Count;
            for (int i = 0; i < list.Count - 1; i++)
            {
                IndexLines.Add(new IndexLine(num, num + 1));
                num += 2;
            }
        }
    }


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
        base.IndexBufferCount = base.PrimitiveCount * IndexLine.GetSizeInShortInts();
        int indexBufferCount = base.IndexBufferCount;
        base.IndexBuffer = new IndexBuffer(indexBufferCount);
        base.IndexBuffer.Map(indexBufferCount);
        base.IndexBuffer.GetIndexStreamLine().AddLines(IndexLines);
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

    public override void AddVertexPositionNormalColored(ColorWithTransparency color)
    {
        throw new NotImplementedException();
    }
}
