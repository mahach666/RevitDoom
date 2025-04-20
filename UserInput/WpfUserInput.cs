using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity.Event;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.MathUtils;
using DoomNetFrameworkEngine.DoomEntity.World;
using DoomNetFrameworkEngine.UserInput;

using InputKeyBinding = DoomNetFrameworkEngine.UserInput.KeyBinding;

namespace RevitDoom.UserInput
{
    public class WpfUserInput : IUserInput, IDisposable
    {
        private readonly Config _config;
        private Action<DoomEvent>? _postEvent;

        private Window? _window;

        private readonly HashSet<Key> _pressedKeys = new();
        private readonly HashSet<MouseButton> _pressedMouse = new();

        private readonly bool[] _weaponKeys = new bool[7];
        private int _turnHeld;

        private bool _mouseGrabbed;
        private Vector2 _mouseDelta;

        private readonly HashSet<Key> _menuPressedKeys = new();  


        public WpfUserInput(Config config)
        {
            _config = config;
            //_postEvent = postEvent;
        }

        public void RegisteredApp(Action<DoomEvent> postEvent)
        {
            _postEvent = postEvent;
        }

        public void AttachWindow(Window window)
        {
            _window = window;

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;

            window.MouseMove += OnMouseMove;
            window.MouseDown += OnMouseDown;
            window.MouseUp += OnMouseUp;

            window.LostKeyboardFocus += (_, _) => _pressedKeys.Clear();
        }

        public void BuildTicCmd(TicCmd cmd)
        {
            var kForward = IsPressed(_config.key_forward);
            var kBackward = IsPressed(_config.key_backward);
            var kStrafeLeft = IsPressed(_config.key_strafeleft);
            var kStrafeRight = IsPressed(_config.key_straferight);

            var kTurnLeft = IsPressed(_config.key_turnleft) || _pressedKeys.Contains(Key.Q);
            var kTurnRight = IsPressed(_config.key_turnright) || _pressedKeys.Contains(Key.E);
            var kFire = IsPressed(_config.key_fire) || _pressedKeys.Contains(Key.Space);
            var kUse = IsPressed(_config.key_use) || _pressedKeys.Contains(Key.F);

            var kRun = IsPressed(_config.key_run);
            var kStrafe = IsPressed(_config.key_strafe);

            _weaponKeys[0] = _pressedKeys.Contains(Key.D1);
            _weaponKeys[1] = _pressedKeys.Contains(Key.D2);
            _weaponKeys[2] = _pressedKeys.Contains(Key.D3);
            _weaponKeys[3] = _pressedKeys.Contains(Key.D4);
            _weaponKeys[4] = _pressedKeys.Contains(Key.D5);
            _weaponKeys[5] = _pressedKeys.Contains(Key.D6);
            _weaponKeys[6] = _pressedKeys.Contains(Key.D7);

            cmd.Clear();

            var speed = kRun ? 1 : 0;
            if (_config.game_alwaysrun) speed = 1 - speed;

            var forward = 0;
            var side = 0;
            var strafe = kStrafe;

            if (kTurnLeft || kTurnRight) _turnHeld++; else _turnHeld = 0;
            var turnSpeed = _turnHeld < PlayerBehavior.SlowTurnTics ? 2 : speed;

            if (strafe)
            {
                if (kTurnRight) side += PlayerBehavior.SideMove[speed];
                if (kTurnLeft) side -= PlayerBehavior.SideMove[speed];
            }
            else
            {
                if (kTurnRight) cmd.AngleTurn -= (short)PlayerBehavior.AngleTurn[turnSpeed];
                if (kTurnLeft) cmd.AngleTurn += (short)PlayerBehavior.AngleTurn[turnSpeed];
            }

            if (kForward) forward += PlayerBehavior.ForwardMove[speed];
            if (kBackward) forward -= PlayerBehavior.ForwardMove[speed];
            if (kStrafeLeft) side -= PlayerBehavior.SideMove[speed];
            if (kStrafeRight) side += PlayerBehavior.SideMove[speed];

            if (kFire) cmd.Buttons |= TicCmdButtons.Attack;
            if (kUse) cmd.Buttons |= TicCmdButtons.Use;

            for (int i = 0; i < _weaponKeys.Length; i++)
                if (_weaponKeys[i])
                {
                    cmd.Buttons |= TicCmdButtons.Change;
                    cmd.Buttons |= (byte)(i << TicCmdButtons.WeaponShift);
                    break;
                }

            //if (_mouseGrabbed) ApplyMouse(ref cmd, ref forward, ref side, strafe);

            forward = NetFunc.Clamp(forward, -PlayerBehavior.MaxMove, PlayerBehavior.MaxMove);
            side = NetFunc.Clamp(side, -PlayerBehavior.MaxMove, PlayerBehavior.MaxMove);

            cmd.ForwardMove += (sbyte)forward;
            cmd.SideMove += (sbyte)side;
        }

        public void PollMenuKeys()
        {
            SendMenuKey(Key.Up, DoomKey.Up);
            SendMenuKey(Key.W, DoomKey.Up);

            SendMenuKey(Key.Down, DoomKey.Down);
            SendMenuKey(Key.S, DoomKey.Down);

            SendMenuKey(Key.Left, DoomKey.Left);
            SendMenuKey(Key.A, DoomKey.Left);

            SendMenuKey(Key.Right, DoomKey.Right);
            SendMenuKey(Key.D, DoomKey.Right);

            SendMenuKey(Key.Enter, DoomKey.Enter);
            SendMenuKey(Key.Space, DoomKey.Enter);
            SendMenuKey(Key.Escape, DoomKey.Escape);

            SendMenuKey(Key.Y, DoomKey.Y);
            SendMenuKey(Key.N, DoomKey.N);
        }


        private void SendMenuKey(Key wpfKey, DoomKey mapped)
        {
            bool down = Keyboard.IsKeyDown(wpfKey);  
            if (down)
            {
                if (_menuPressedKeys.Add(wpfKey))     
                    _postEvent?.Invoke(new DoomEvent(EventType.KeyDown, mapped));
            }
            else if (_menuPressedKeys.Remove(wpfKey))
            {
                _postEvent?.Invoke(new DoomEvent(EventType.KeyUp, mapped));
            }
        }


        public void Reset()
        {
            _pressedKeys.Clear();
            _pressedMouse.Clear();
            _menuPressedKeys.Clear();
            _mouseDelta = Vector2.Zero;
        }


        public void GrabMouse() => _mouseGrabbed = true;
        public void ReleaseMouse() => _mouseGrabbed = false;

        public int MaxMouseSensitivity => 15;
        public int MouseSensitivity
        {
            get => _config.mouse_sensitivity;
            set => _config.mouse_sensitivity = value;
        }

        public void Dispose()
        {
            if (_window == null) return;
            _window.KeyDown -= OnKeyDown;
            _window.KeyUp -= OnKeyUp;
            _window.MouseMove -= OnMouseMove;
            _window.MouseDown -= OnMouseDown;
            _window.MouseUp -= OnMouseUp;
        }

        private void ApplyMouse(ref TicCmd cmd,
                                ref int forward,
                                ref int side,
                                bool strafe)
        {
            var ms = 0.5f * _config.mouse_sensitivity;
            var mx = (int)NetFunc.RoundF(ms * _mouseDelta.X);
            var my = (int)NetFunc.RoundF(ms * -_mouseDelta.Y);

            forward += my;

            if (strafe) side += mx * 2;
            else cmd.AngleTurn -= (short)(mx * 0x8);

            _mouseDelta = Vector2.Zero;
        }

        private bool IsPressed(InputKeyBinding binding)
        {
            // клавиши
            foreach (var doomKey in binding.Keys)
                if (_pressedKeys.Contains(DoomToWpf(doomKey))) return true;

            // кнопки мыши
            //foreach (var mb in binding.MouseButtons)
            //    if (IsMousePressed(mb)) return true;

            return false;
        }

        private bool IsMousePressed(int mb) => mb switch
        {
            //0 => _pressedMouse.Contains(MouseButton.Left),
            //1 => _pressedMouse.Contains(MouseButton.Right),
            //2 => _pressedMouse.Contains(MouseButton.Middle),
            //_ => false
        };

        private void OnKeyDown(object? s, KeyEventArgs e)
        {
            _pressedKeys.Add(e.Key);
            SendMenuEvent(e.Key, true);
        }

        private void OnKeyUp(object? s, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);
            SendMenuEvent(e.Key, false);
        }

        private void OnMouseMove(object? s, MouseEventArgs e)
        {
            //if (!_mouseGrabbed) return;
            //var p = e.GetPosition(_window);
            //_mouseDelta += new Vector2((float)p.X, (float)p.Y);
        }

        private void OnMouseDown(object? s, MouseButtonEventArgs e) { }
        //=> _pressedMouse.Add(e.ChangedButton);

        private void OnMouseUp(object? s, MouseButtonEventArgs e) { }
            //=> _pressedMouse.Remove(e.ChangedButton);

        private void SendMenuEvent(Key key, bool down)
        {
            DoomKey? mapped = key switch
            {
                Key.W or Key.Up => DoomKey.Up,
                Key.S or Key.Down => DoomKey.Down,
                Key.A or Key.Left => DoomKey.Left,
                Key.D or Key.Right => DoomKey.Right,

                Key.Space or Key.Enter => DoomKey.Enter,
                Key.Escape => DoomKey.Escape,
                Key.Y => DoomKey.Y,
                Key.N => DoomKey.N,
                _ => null
            };

            if (mapped.HasValue)
                _postEvent?.Invoke(new DoomEvent(down ? EventType.KeyDown : EventType.KeyUp, mapped.Value));
        }

        private static Key DoomToWpf(DoomKey k) => k switch
        {
            DoomKey.A => Key.A,
            DoomKey.B => Key.B,
            DoomKey.C => Key.C,
            DoomKey.D => Key.D,
            DoomKey.E => Key.E,
            DoomKey.F => Key.F,
            DoomKey.G => Key.G,
            DoomKey.H => Key.H,
            DoomKey.I => Key.I,
            DoomKey.J => Key.J,
            DoomKey.K => Key.K,
            DoomKey.L => Key.L,
            DoomKey.M => Key.M,
            DoomKey.N => Key.N,
            DoomKey.O => Key.O,
            DoomKey.P => Key.P,
            DoomKey.Q => Key.Q,
            DoomKey.R => Key.R,
            DoomKey.S => Key.S,
            DoomKey.T => Key.T,
            DoomKey.U => Key.U,
            DoomKey.V => Key.V,
            DoomKey.W => Key.W,
            DoomKey.X => Key.X,
            DoomKey.Y => Key.Y,
            DoomKey.Z => Key.Z,

            DoomKey.Num0 => Key.D0,
            DoomKey.Num1 => Key.D1,
            DoomKey.Num2 => Key.D2,
            DoomKey.Num3 => Key.D3,
            DoomKey.Num4 => Key.D4,
            DoomKey.Num5 => Key.D5,
            DoomKey.Num6 => Key.D6,
            DoomKey.Num7 => Key.D7,
            DoomKey.Num8 => Key.D8,
            DoomKey.Num9 => Key.D9,

            DoomKey.Escape => Key.Escape,
            DoomKey.LControl => Key.LeftCtrl,
            DoomKey.RControl => Key.RightCtrl,
            DoomKey.LShift => Key.LeftShift,
            DoomKey.RShift => Key.RightShift,
            DoomKey.LAlt => Key.LeftAlt,
            DoomKey.RAlt => Key.RightAlt,
            DoomKey.Space => Key.Space,
            DoomKey.Enter => Key.Enter,
            DoomKey.Backspace => Key.Back,
            DoomKey.Tab => Key.Tab,
            DoomKey.Left => Key.Left,
            DoomKey.Right => Key.Right,
            DoomKey.Up => Key.Up,
            DoomKey.Down => Key.Down,
            DoomKey.Pause => Key.Pause,
            DoomKey.F1 => Key.F1,
            DoomKey.F2 => Key.F2,
            DoomKey.F3 => Key.F3,
            DoomKey.F4 => Key.F4,
            DoomKey.F5 => Key.F5,
            DoomKey.F6 => Key.F6,
            DoomKey.F7 => Key.F7,
            DoomKey.F8 => Key.F8,
            DoomKey.F9 => Key.F9,
            DoomKey.F10 => Key.F10,
            DoomKey.F11 => Key.F11,
            DoomKey.F12 => Key.F12,
            _ => Key.None
        };
    }
}
