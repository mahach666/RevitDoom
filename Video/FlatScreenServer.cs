using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDoom.RevitDrow
{
    /// <summary>
    /// DirectContext3D‑сервер, полностью перешедший на ScreenFaceBufferStorage.
    /// Хранит только массив буферов + текущие цвета.
    /// </summary>
    public class FlatScreenServer : IDirectContext3DServer, IExternalServer
    {
        private readonly Guid _guid = Guid.NewGuid();
        private readonly UIDocument _uiDoc;
        private readonly int _width;
        private readonly int _height;
        private readonly List<MeshFaceBufferStorage> _buffers = new();
        private readonly List<ColorWithTransparency> _colors = new();
        private ScreenFaceBufferStorage _screenBuffer;

        public FlatScreenServer(UIDocument uiDoc, int width, int height, double cellSize)
        {
            _uiDoc = uiDoc;
            _width = width;
            _height = height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    XYZ origin = new XYZ(x * cellSize, y * cellSize, 0);
                    MeshData mesh = CreateQuad(origin, cellSize);

                    var buf = new MeshFaceBufferStorage(DisplayStyle.Shading, mesh);
                    _buffers.Add(buf);
                    _colors.Add(new ColorWithTransparency(0, 0, 0, 0));
                }
            }

            _screenBuffer = new ScreenFaceBufferStorage(DisplayStyle.Shading);
        }

        #region Pixel ops
        public void SetPixel(int x, int y, ColorWithTransparency color)
        {
            int index = y * _width + x;
            if (index < 0 || index >= _colors.Count) return;

            // Только фиксируем новый цвет; сами буферы обновляем
            // внутри RenderScene() — это безопасно с точки зрения Revit API.
            _colors[index] = color;
        }

        public void SetPixels(byte[] buffer, int width, int height)
        {
            if (buffer == null || buffer.Length == 0) return;
            int bytesPerPixel = 4; // RGBA

            int scaleX = width / _width;
            int scaleY = height / _height;
            if (scaleX < 1) scaleX = 1;
            if (scaleY < 1) scaleY = 1;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int srcX = x * scaleX;
                    int srcY = y * scaleY;

                    int srcIndex = (srcX * height + srcY) * bytesPerPixel; // column‑major
                    if (srcIndex + 2 >= buffer.Length) continue;

                    byte r = buffer[srcIndex + 0];
                    byte g = buffer[srcIndex + 1];
                    byte b = buffer[srcIndex + 2];
                    byte a = 0;

                    _colors[y * _width + x] = new ColorWithTransparency(r, g, b, a);
                }
            }
        }

        //public void SetPixels(byte[] buffer, int width, int height)
        //{
        //    if (buffer == null || buffer.Length == 0) return;
        //    int bytesPerPixel = 4; // RGBA

        //    // Определяем масштаб: исходная картинка может быть 320×200, а экран, например, 160×100
        //    int scaleX = width / _width;
        //    int scaleY = height / _height;
        //    if (scaleX < 1) scaleX = 1;
        //    if (scaleY < 1) scaleY = 1;

        //    for (int y = 0; y < _height; y++)
        //    {
        //        for (int x = 0; x < _width; x++)
        //        {
        //            int srcX = x * scaleX;
        //            int srcY = y * scaleY;

        //            // Doom использует column‑major: (col * height + row)
        //            int srcIndex = (srcX * height + srcY) * bytesPerPixel;
        //            if (srcIndex + 2 >= buffer.Length) continue;

        //            byte r = buffer[srcIndex + 0];
        //            byte g = buffer[srcIndex + 1];
        //            byte b = buffer[srcIndex + 2];
        //            byte a = 0;

        //            SetPixel(x, _height - y - 1, new ColorWithTransparency(r, g, b, a));
        //        }
        //    }
        //}
        #endregion

        #region Geometry helpers
        private static MeshData CreateQuad(XYZ origin, double size)
        {
            size *= 2;
            var mesh = new MeshData();
            XYZ p0 = origin;
            XYZ p1 = origin + new XYZ(size, 0, 0);
            XYZ p2 = origin + new XYZ(size, size, 0);
            XYZ p3 = origin + new XYZ(0, size, 0);
            mesh.Vertices.AddRange(new[] { p0, p1, p2, p3 });
            mesh.Triangles.Add(new IndexTriangle(0, 2, 3));
            return mesh;
        }
        #endregion

        #region IDirectContext3DServer
        public void RenderScene(View view, DisplayStyle style)
        {
            try
            {
                if (_screenBuffer.DisplayStyle != style)
                    _screenBuffer = new ScreenFaceBufferStorage(style);
                else
                    _screenBuffer.Clear();

                for (int i = 0; i < _buffers.Count; i++)
                {
                    var buf = _buffers[i];
                    var col = _colors[i];

                    if (buf.VertexBuffer == null)
                        buf.AddVertexPositionNormalColored(col);
                    else
                        buf.UpdateVertexColors(col);

                    _screenBuffer.Add(buf);
                }

                _screenBuffer.Flush();
            }
            catch { /* DirectContext3D может бросать при закрытии Revit – игнор */ }
        }

        public Outline GetBoundingBox(View view)
        {
            if (_buffers.Count == 0) return new Outline(XYZ.Zero, XYZ.Zero);
            var first = _buffers[0].Mesh;
            XYZ min = first.Vertices.Aggregate((a, b) => new XYZ(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));
            XYZ max = first.Vertices.Aggregate((a, b) => new XYZ(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));
            return new Outline(min, max);
        }
        #endregion

        #region IExternalServer metadata
        public bool UsesHandles() => false;
        public bool UseInTransparentPass(View view) => false;
        public bool CanExecute(View view) => true;
        public Guid GetServerId() => _guid;
        public string GetVendorId() => "GenFusions";
        public ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
        public string GetName() => "Flat Screen Server";
        public string GetDescription() => "Draws flat quads with one aggregated ScreenBuffer";
        public string GetApplicationId() => string.Empty;
        public string GetSourceId() => string.Empty;
        #endregion
    }
}
