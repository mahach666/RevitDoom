using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace RevitDoom.UserInput
{
    internal static class GlobalKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYUP = 0x0101;
        private const int HC_ACTION = 0;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private static IntPtr _hHook = IntPtr.Zero;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;

        private static readonly HashSet<int> _blocked = new()
        {
            VK(Key.W), VK(Key.A), VK(Key.S), VK(Key.D),
            VK(Key.Up), VK(Key.Down), VK(Key.Left), VK(Key.Right),
            VK(Key.Q), VK(Key.E),
            VK(Key.Space), VK(Key.F),
            VK(Key.LeftShift), VK(Key.RightShift),
            VK(Key.LeftCtrl), VK(Key.RightCtrl),
            VK(Key.D1), VK(Key.D2), VK(Key.D3), VK(Key.D4),
            VK(Key.D5), VK(Key.D6), VK(Key.D7)
        };

        static GlobalKeyboardHook() => Install();

        internal static void Install()
        {
            if (_hHook != IntPtr.Zero) return;

            IntPtr hModule = GetModuleHandle(null);
            _hHook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hModule, 0);
        }

        private static void ClearState()
        {
            Array.Clear(_state, 0, _state.Length);
        }

        internal static void Uninstall()
        {
            if (_hHook == IntPtr.Zero) return;
            UnhookWindowsHookEx(_hHook);
            _hHook = IntPtr.Zero;
            ClearState();
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
                                                      IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                                    IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        private static int VK(Key k) => KeyInterop.VirtualKeyFromKey(k);

        private static readonly bool[] _state = new bool[256];

        internal static bool IsKeyDown(Key key) => _state[VK(key)];

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HC_ACTION)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN ||
                    msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    int vk = (int)info.vkCode;

                    bool down = (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN);

                    _state[vk] = down;

                    if (down && _blocked.Contains(vk))
                        return (IntPtr)1;           
                }
            }
            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }
    }
}
