using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity.Event;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.World;
using DoomNetFrameworkEngine.UserInput;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RevitDoom.UserInput
{
    public class ConsoleUserInput : IUserInput
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private Config config;
        private Action<DoomEvent> postEventCallback;
        private readonly HashSet<ConsoleKey> pressedKeys = new();

        public ConsoleUserInput(Config config, Action<DoomEvent> postEvent)
        {
            this.config = config;
            postEventCallback = postEvent;
        }

        private static bool IsKeyDown(ConsoleKey key) => (GetAsyncKeyState((int)key) & 0x8000) != 0;

        public void PostEvent(DoomEvent e) { }

        public void BuildTicCmd(TicCmd cmd)
        {
            cmd.Clear();

            int speed = 1;

            if (IsKeyDown(ConsoleKey.W))
                cmd.ForwardMove += (sbyte)PlayerBehavior.ForwardMove[speed];
            if (IsKeyDown(ConsoleKey.S))
                cmd.ForwardMove -= (sbyte)PlayerBehavior.ForwardMove[speed];
            if (IsKeyDown(ConsoleKey.A))
                cmd.SideMove -= (sbyte)PlayerBehavior.SideMove[speed];
            if (IsKeyDown(ConsoleKey.D))
                cmd.SideMove += (sbyte)PlayerBehavior.SideMove[speed];

            if (IsKeyDown(ConsoleKey.Q))
                cmd.AngleTurn += (short)PlayerBehavior.AngleTurn[speed];
            if (IsKeyDown(ConsoleKey.E))
                cmd.AngleTurn -= (short)PlayerBehavior.AngleTurn[speed];

            if (IsKeyDown(ConsoleKey.Spacebar))
                cmd.Buttons |= TicCmdButtons.Attack;
            if (IsKeyDown(ConsoleKey.F))
                cmd.Buttons |= TicCmdButtons.Use;
            if (IsKeyDown(ConsoleKey.Escape))
                postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Escape));

            for (int i = 1; i <= 7; i++)
            {
                if (IsKeyDown((ConsoleKey)((int)ConsoleKey.D1 + i - 1)))
                {
                    cmd.Buttons |= TicCmdButtons.Change;
                    cmd.Buttons |= (byte)((i - 1) << TicCmdButtons.WeaponShift);
                    break;
                }
            }
        }

        private void SendMenuKey(ConsoleKey key, DoomKey mapped)
        {
            if (IsKeyDown(key))
            {
                if (!pressedKeys.Contains(key))
                {
                    pressedKeys.Add(key);
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, mapped));
                }
            }
            else if (pressedKeys.Remove(key))
            {
                postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, mapped));
            }
        }

        public void Reset() { pressedKeys.Clear(); }
        public void GrabMouse() { }
        public void ReleaseMouse() { }
        public void Dispose() { }

        public int MaxMouseSensitivity => 15;
        public int MouseSensitivity { get => 5; set { } }



        public void PollMenuKeys()
        {
            SendMenuKey(ConsoleKey.UpArrow, DoomKey.Up);
            SendMenuKey(ConsoleKey.W, DoomKey.Up);

            SendMenuKey(ConsoleKey.DownArrow, DoomKey.Down);
            SendMenuKey(ConsoleKey.S, DoomKey.Down);

            SendMenuKey(ConsoleKey.LeftArrow, DoomKey.Left);
            SendMenuKey(ConsoleKey.A, DoomKey.Left);

            SendMenuKey(ConsoleKey.RightArrow, DoomKey.Right);
            SendMenuKey(ConsoleKey.D, DoomKey.Right);

            SendMenuKey(ConsoleKey.Enter, DoomKey.Enter);
            SendMenuKey(ConsoleKey.Spacebar, DoomKey.Enter);

            SendMenuKey(ConsoleKey.Escape, DoomKey.Escape);

            SendMenuKey(ConsoleKey.Y, DoomKey.Y);
            SendMenuKey(ConsoleKey.N, DoomKey.N);
        }
    } 
}