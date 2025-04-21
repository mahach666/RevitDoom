using System.Collections.Generic;

namespace RevitDoom.Contracts
{
    public interface IRevitDirectServerController
    {
        public void RegisterServer(CastomDirectContextServer revitServer);
        public void RegisterMultiServer(List<CastomDirectContextServer> serverList);
        public void UnregisterAllServers();       
    }
}
