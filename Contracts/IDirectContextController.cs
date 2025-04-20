using Autodesk.Revit.DB.DirectContext3D;
using RevitDoom.Enums;
using System.Collections.Generic;

namespace RevitDoom.Contracts
{
    internal interface IDirectContextController
    {
        void RegisterServer(Quality quality);
        void UnregisterAllServers();
        IDirectContext3DServer ActiveServer { get; }
    }
}
