using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using System;
using System.Collections.Generic;

namespace RevitDoom.RevitDrow
{
    /// <summary>
    /// Буфер‑агрегатор: собирает вершины и индексы из нескольких MeshFaceBufferStorage
    /// и делает единственный FlushBuffer для всего кадра.
    /// </summary>
    public class ScreenFaceBufferStorage : BaseBufferStorage
    {
        private readonly List<VertexPositionNormalColored> _vertices = new();
        private readonly List<int> _indices = new();
        private int _vertexOffset;

        public ScreenFaceBufferStorage(DisplayStyle displayStyle) : base(displayStyle)
        {
            BufferPrimitiveType = PrimitiveType.TriangleList;
        }

        public void Add(MeshFaceBufferStorage buffer)
        {
            if (!buffer.TryGetData(out var verts, out var inds) || verts.Length == 0)
                return;

            // На первом буфере копируем рендер‑параметры.
            if (VertexBuffer == null)
            {
                FormatBits = buffer.FormatBits;
                VertexFormat = buffer.VertexFormat;
                EffectInstance = buffer.EffectInstance;
            }

            _vertices.AddRange(verts);
            foreach (var i in inds)
                _indices.Add(i + _vertexOffset);
            _vertexOffset += verts.Length;
        }

        public void Flush()
        {
            BuildBuffers();
            if (VertexBuffer == null || IndexBuffer == null) return;

            DrawContext.FlushBuffer(VertexBuffer, VertexBufferCount,
                IndexBuffer, IndexBufferCount,
                VertexFormat, EffectInstance,
                BufferPrimitiveType, 0, PrimitiveCount);
        }

        public void Clear()
        {
            _vertices.Clear();
            _indices.Clear();
            _vertexOffset = 0;
            VertexBuffer = null;
            IndexBuffer = null;
            VertexBufferCount = 0;
            IndexBufferCount = 0;
            PrimitiveCount = 0;
        }

        private void BuildBuffers()
        {
            if (_vertices.Count == 0 || _indices.Count == 0)
                return;

            VertexBufferCount = _vertices.Count;
            IndexBufferCount = _indices.Count;
            PrimitiveCount = _indices.Count / 3;

            int sizeInFloats = VertexPositionNormalColored.GetSizeInFloats() * VertexBufferCount;
            VertexBuffer = new VertexBuffer(sizeInFloats);
            VertexBuffer.Map(sizeInFloats);
            var vStream = VertexBuffer.GetVertexStreamPositionNormalColored();
            foreach (var v in _vertices)
                vStream.AddVertex(v);
            VertexBuffer.Unmap();

            IndexBuffer = new IndexBuffer(IndexBufferCount);
            IndexBuffer.Map(IndexBufferCount);
            var iStream = IndexBuffer.GetIndexStreamTriangle();
            for (int i = 0; i < _indices.Count; i += 3)
                iStream.AddTriangle(new IndexTriangle((short)_indices[i], (short)_indices[i + 1], (short)_indices[i + 2]));
            IndexBuffer.Unmap();
        }

        #region BaseBufferStorage stubs
        public override void AddVertexPosition() => throw new NotImplementedException();
        public override void AddVertexPositionColored(ColorWithTransparency color) => throw new NotImplementedException();
        public override void AddVertexPositionNormal() => throw new NotImplementedException();
        public override void AddVertexPosition(ColorWithTransparency color) => throw new NotImplementedException();

        public override void AddVertexPositionNormalColored(ColorWithTransparency color)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
