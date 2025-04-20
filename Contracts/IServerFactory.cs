using Autodesk.Revit.DB.DirectContext3D;
using RevitDoom.Enums;

namespace RevitDoom.Contracts
{
    public interface IServerFactory
    {
        CastomDirectContextServer Create<TServer>(Quality quality)
            where TServer : CastomDirectContextServer;
    }
}
