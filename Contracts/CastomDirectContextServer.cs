using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using System;

namespace RevitDoom.Contracts
{
    public abstract class CastomDirectContextServer : IDirectContext3DServer, IExternalServer
    {
        public abstract bool CanExecute(View dBView);

        public abstract string GetApplicationId();

        public abstract Outline GetBoundingBox(View dBView);

        public abstract string GetDescription();
        public abstract string GetName();

        public abstract Guid GetServerId();

        public abstract ExternalServiceId GetServiceId();

        public abstract string GetSourceId();

        public abstract string GetVendorId();

        public abstract void RenderScene(View dBView, DisplayStyle displayStyle);

        public abstract bool UseInTransparentPass(View dBView);

        public abstract bool UsesHandles();

        public abstract void SetPixel(int x, int y, int width, ColorWithTransparency color);
        public abstract void SetPixels(byte[] buffer, int width, int height);
    }
}
