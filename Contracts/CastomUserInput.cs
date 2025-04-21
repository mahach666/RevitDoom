using DoomNetFrameworkEngine.DoomEntity.Event;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.UserInput;
using System;

namespace RevitDoom.Contracts
{
    public abstract class CastomUserInput : IUserInput, IDisposable
    {
        public abstract int MaxMouseSensitivity { get; }
        public abstract int MouseSensitivity { get; set; }
        public abstract void BuildTicCmd(TicCmd cmd);
        public abstract void Dispose();
        public abstract void GrabMouse();
        public abstract void ReleaseMouse();
        public abstract void Reset();
        public abstract void RegisterAppEvent(Action<DoomEvent> postEvent);
        public abstract void PollMenuKeys();
    }
}
