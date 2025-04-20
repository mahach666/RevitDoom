using Autodesk.Revit.DB.DirectContext3D;
using RevitDoom.Enums;
using System.Collections.Generic;

namespace RevitDoom.Contracts
{
    public interface IDirectContextController
    {
        void RegisterServer<T>(Quality quality) 
            where T : CastomDirectContextServer;
        void UnregisterAllServers();
        void Update(byte[] buffer, int width, int height);
        bool IsValid(Quality quality);
    }
}
