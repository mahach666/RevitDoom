using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity.Event;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.World;
using DoomNetFrameworkEngine.UserInput;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace RevitDoom.UserInput
{
    public class WpfUserInput : IUserInput
    {
        private Config config;
        private Action<DoomEvent> postEventCallback;
        private readonly HashSet<Key> pressedKeys = new();

        public WpfUserInput(Config config, Action<DoomEvent> postEvent)
        {
            this.config = config;
            postEventCallback = postEvent;

            // Подписываемся на глобальные клавиши (например, в окне MainWindow)
            var window = System.Windows.Application.Current?.MainWindow;
            if (window != null)
            {
                window.KeyDown += OnKeyDown;
                window.KeyUp += OnKeyUp;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!pressedKeys.Add(e.Key))
                return;

            switch (e.Key)
            {
                case Key.Escape:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Escape));
                    break;
                case Key.Y:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Y));
                    break;
                case Key.N:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.N));
                    break;
                case Key.Enter:
                case Key.Space:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Enter));
                    break;
                case Key.Up:
                case Key.W:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Up));
                    break;
                case Key.Down:
                case Key.S:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Down));
                    break;
                case Key.Left:
                case Key.A:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Left));
                    break;
                case Key.Right:
                case Key.D:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Right));
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!pressedKeys.Remove(e.Key))
                return;

            // Отправка KeyUp
            switch (e.Key)
            {
                case Key.Escape:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Escape));
                    break;
                case Key.Y:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Y));
                    break;
                case Key.N:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.N));
                    break;
                case Key.Enter:
                case Key.Space:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Enter));
                    break;
                case Key.Up:
                case Key.W:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Up));
                    break;
                case Key.Down:
                case Key.S:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Down));
                    break;
                case Key.Left:
                case Key.A:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Left));
                    break;
                case Key.Right:
                case Key.D:
                    postEventCallback?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Right));
                    break;
            }
        }

        public void PostEvent(DoomEvent e) { }

        public void BuildTicCmd(TicCmd cmd)
        {
            cmd.Clear();

            int speed = 1;

            if (pressedKeys.Contains(Key.W)) cmd.ForwardMove += (sbyte)PlayerBehavior.ForwardMove[speed];
            if (pressedKeys.Contains(Key.S)) cmd.ForwardMove -= (sbyte)PlayerBehavior.ForwardMove[speed];
            if (pressedKeys.Contains(Key.A)) cmd.SideMove -= (sbyte)PlayerBehavior.SideMove[speed];
            if (pressedKeys.Contains(Key.D)) cmd.SideMove += (sbyte)PlayerBehavior.SideMove[speed];

            if (pressedKeys.Contains(Key.Q)) cmd.AngleTurn += (short)PlayerBehavior.AngleTurn[speed];
            if (pressedKeys.Contains(Key.E)) cmd.AngleTurn -= (short)PlayerBehavior.AngleTurn[speed];

            if (pressedKeys.Contains(Key.Space)) cmd.Buttons |= TicCmdButtons.Attack;
            if (pressedKeys.Contains(Key.F)) cmd.Buttons |= TicCmdButtons.Use;

            for (int i = 1; i <= 7; i++)
            {
                if (pressedKeys.Contains(Key.D1 + (i - 1)))
                {
                    cmd.Buttons |= TicCmdButtons.Change;
                    cmd.Buttons |= (byte)((i - 1) << TicCmdButtons.WeaponShift);
                    break;
                }
            }
        }

        public void PollMenuKeys() { } // не нужен, так как теперь всё через события

        public void Reset() => pressedKeys.Clear();
        public void GrabMouse() { }
        public void ReleaseMouse() { }
        public void Dispose() => Reset();

        public int MaxMouseSensitivity => 15;
        public int MouseSensitivity { get => 5; set { } }
    }
}
