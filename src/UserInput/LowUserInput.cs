using DoomNetFrameworkEngine;
using DoomNetFrameworkEngine.DoomEntity.Event;
using DoomNetFrameworkEngine.DoomEntity.Game;
using DoomNetFrameworkEngine.DoomEntity.World;
using DoomNetFrameworkEngine.UserInput;
using RevitDoom.Contracts;
using RevitDoom.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Input;
using InputKeyBinding = DoomNetFrameworkEngine.UserInput.KeyBinding;

namespace RevitDoom.UserInput
{
    public sealed class LowUserInput : CastomUserInput
    {
        private readonly Config _config;
        private Action<DoomEvent>? _postEvent;

        private readonly bool[] _weaponKeys = new bool[7];
        private int _turnHeld;

        private bool _mouseGrabbed;
        private Vector2 _mouseDelta;

        // для отслеживания автоповторов меню
        private readonly HashSet<Key> _menuPressedKeys = new();

        public LowUserInput(Config config)
        {
            _ = GlobalKeyboardHook.IsKeyDown(Key.None);
            _config = config;
        }
        public override void RegisterAppEvent(Action<DoomEvent> postEvent) => _postEvent = postEvent;

        public override void Reset()
        {
            _menuPressedKeys.Clear();
            _mouseDelta = Vector2.Zero;
        }

        public override void GrabMouse() => _mouseGrabbed = true;
        public override void ReleaseMouse() => _mouseGrabbed = false;

        public override int MaxMouseSensitivity => 15;
        public override int MouseSensitivity
        {
            get => _config.mouse_sensitivity;
            set => _config.mouse_sensitivity = value;
        }

        public override void BuildTicCmd(TicCmd cmd)
        {
            bool kForward = IsPressed(_config.key_forward);
            bool kBackward = IsPressed(_config.key_backward);
            bool kStrafeLeft = IsPressed(_config.key_strafeleft);
            bool kStrafeRight = IsPressed(_config.key_straferight);

            bool kTurnLeft = IsPressed(_config.key_turnleft) || IsDown(Key.Q);
            bool kTurnRight = IsPressed(_config.key_turnright) || IsDown(Key.E);
            bool kFire = IsPressed(_config.key_fire) || IsDown(Key.Space);
            bool kUse = IsPressed(_config.key_use) || IsDown(Key.F);

            bool kRun = IsPressed(_config.key_run);
            bool kStrafe = IsPressed(_config.key_strafe);

            _weaponKeys[0] = IsDown(Key.D1);
            _weaponKeys[1] = IsDown(Key.D2);
            _weaponKeys[2] = IsDown(Key.D3);
            _weaponKeys[3] = IsDown(Key.D4);
            _weaponKeys[4] = IsDown(Key.D5);
            _weaponKeys[5] = IsDown(Key.D6);
            _weaponKeys[6] = IsDown(Key.D7);

            cmd.Clear();

            int speed = kRun ? 1 : 0;
            if (_config.game_alwaysrun) speed = 1 - speed;

            int forward = 0, side = 0;
            if (kTurnLeft || kTurnRight) _turnHeld++; else _turnHeld = 0;
            int turnSpeed = _turnHeld < PlayerBehavior.SlowTurnTics ? 2 : speed;

            if (kStrafe)
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

            forward = NetFunc.Clamp(forward, -PlayerBehavior.MaxMove, PlayerBehavior.MaxMove);
            side = NetFunc.Clamp(side, -PlayerBehavior.MaxMove, PlayerBehavior.MaxMove);

            cmd.ForwardMove += (sbyte)forward;
            cmd.SideMove += (sbyte)side;

            SendEsc();   
        }

        private void SendEsc()
        {
            bool down = IsDown(Key.Escape);
            if (down)
            {
                if (_menuPressedKeys.Add(Key.Escape))
                    _postEvent?.Invoke(new DoomEvent(EventType.KeyDown, DoomKey.Escape));
            }
            else if (_menuPressedKeys.Remove(Key.Escape))
            {
                _postEvent?.Invoke(new DoomEvent(EventType.KeyUp, DoomKey.Escape));
            }
        }

        public override void PollMenuKeys()
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
            bool down = IsDown(wpfKey);
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



        private bool IsPressed(InputKeyBinding binding)
        {
            foreach (var doomKey in binding.Keys)
                if (IsDown(DoomToWpf(doomKey)))
                    return true;           
            return false;
        }

        private static bool IsDown(Key key) => GlobalKeyboardHook.IsKeyDown(key);

   
        //private void ApplyMouse(ref TicCmd cmd, ref int forward, ref int side, bool strafe)
        //{
        //    var ms = 0.5f * _config.mouse_sensitivity;
        //    var mx = (int)NetFunc.RoundF(ms * _mouseDelta.X);
        //    var my = (int)NetFunc.RoundF(ms * -_mouseDelta.Y);

        //    forward += my;
        //    if (strafe) side += mx * 2;
        //    else        cmd.AngleTurn -= (short)(mx * 0x8);

        //    _mouseDelta = Vector2.Zero;
        //}

        public override void Dispose() => GlobalKeyboardHook.Uninstall();

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
