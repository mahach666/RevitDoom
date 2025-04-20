using Autodesk.Revit.DB.DirectContext3D;
using RevitDoom.Enums;

namespace RevitDoom.Contracts
{
    public interface IServerFactory
    {
        IDirectContext3DServer Create<TServer>(Quality quality)
            where TServer : IDirectContext3DServer;
    }
}
