using Autodesk.Revit.DB;

namespace RevitDoom.Contracts
{
    public interface ICastomDirectContextServer
    {
        void SetPixel(int x, int y, int width, ColorWithTransparency color);
        void SetPixels(byte[] buffer, int width, int height);
    }
}
