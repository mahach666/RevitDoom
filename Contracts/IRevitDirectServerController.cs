using Autodesk.Revit.DB.DirectContext3D;
using System.Collections.Generic;

namespace RevitDoom.Contracts
{
    internal interface IRevitDirectServerController
    {
        public void RegisterServer(IDirectContext3DServer revitServer);
        public void RegisterMultiServer(List<IDirectContext3DServer> serverList);
        public void UnregisterAllServers();       
    }
}
