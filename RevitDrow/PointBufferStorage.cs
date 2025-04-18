using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitDoom.RevitDrow;

public class PointBufferStorage : BaseBufferStorage
{
    public List<VertexPositionColored> Vertices = new();

    public PointBufferStorage(DisplayStyle displayStyle)
        : base(displayStyle)
    {
        BufferPrimitiveType = PrimitiveType.PointList; // 1
        DisplayStyle = displayStyle;
    }

    public void AddVertex(XYZ position, ColorWithTransparency color)
    {
        Vertices.Add(new VertexPositionColored(position, color));
        VertexBufferCount++;
        PrimitiveCount++; // каждая вершина — одна точка
    }

    public override void AddVertexPosition(ColorWithTransparency color)
    {
        FormatBits = VertexFormatBits.PositionColored;
        VertexFormat = new VertexFormat(FormatBits);
        EffectInstance = new EffectInstance(FormatBits);
        EffectInstance.SetColor(color.GetColor());
        EffectInstance.SetTransparency(color.GetTransparency() / 255.0);

        if (DisplayStyle == DisplayStyle.HLR)
        {
            EffectInstance.SetSpecularColor(color.GetColor());
            EffectInstance.SetAmbientColor(color.GetColor());
            EffectInstance.SetEmissiveColor(color.GetColor());
        }

        int size = VertexPositionColored.GetSizeInFloats() * VertexBufferCount;
        VertexBuffer = new VertexBuffer(size);
        VertexBuffer.Map(size);
        VertexBuffer.GetVertexStreamPositionColored().AddVertices(Vertices);
        VertexBuffer.Unmap();

        // 👇 ЭТО НУЖНО: создать indexBuffer, даже если он пустой
        IndexBufferCount = VertexBufferCount; // по одной точке на индекс
        IndexBuffer = new IndexBuffer(IndexBufferCount * IndexLine.GetSizeInShortInts());
        IndexBuffer.Map(IndexBufferCount * IndexLine.GetSizeInShortInts());

        var stream = IndexBuffer.GetIndexStreamPoint();
        for (ushort i = 0; i < VertexBufferCount; i++)
            stream.AddPoint(new IndexPoint(i));

        IndexBuffer.Unmap();
    }


    public override void AddVertexPosition() => throw new System.NotImplementedException();
    public override void AddVertexPositionColored(ColorWithTransparency color) => throw new System.NotImplementedException();
    public override void AddVertexPositionNormal() => throw new System.NotImplementedException();
    public override void AddVertexPositionNormalColored(ColorWithTransparency color) => throw new System.NotImplementedException();
}
