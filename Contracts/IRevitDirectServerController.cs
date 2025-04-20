using Autodesk.Revit.DB.DirectContext3D;
using System.Collections.Generic;

namespace RevitDoom.Contracts
{
    internal interface IRevitDirectServerController
    {
        public void RegisterServer(CastomDirectContextServer revitServer);
        public void RegisterMultiServer(List<CastomDirectContextServer> serverList);
        public void UnregisterAllServers();       
    }
}
