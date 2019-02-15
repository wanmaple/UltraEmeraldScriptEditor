using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EditorSupport.Utils
{
    /// <summary>
    /// 全局鼠标钩子
    /// </summary>
    /// <remarks>
    /// 本来是想用全局钩子获取窗体外的鼠标坐标，但是后来发现可以用Mouse.Capture和Mouse.GetPosition随时获取坐标就弃用了
    /// </remarks>
    public sealed class MouseHook
    {
        public event EventHandler<MouseEventArgs> MouseHooked;

        public Boolean IsEnabled => _enabled;

        public MouseHook(IntPtr hMod, IntPtr hThread)
        {
            _hMod = hMod;
            _hThread = hThread;
            _hHook = IntPtr.Zero;
            _hookProc = new HookProcedure(MouseHookProcedure);
        }

        public void Enable()
        {
            if (_enabled)
            {
                return;
            }

            _enabled = true;
            _hHook = Win32Api.SetWindowsHookEx(Win32Api.WH_MOUSE_LL, _hookProc, _hMod, _hThread);
        }

        public void Disable()
        {
            if (!_enabled)
            {
                return;
            }

            _enabled = false;
            Win32Api.UnhookWindowsHookEx(_hHook);
        }

        private Int32 MouseHookProcedure(Int32 code, Int32 wParam, Int32 lParam)
        {
            if (code < 0)
            {
                return Win32Api.CallNextHookEx(_hHook, code, wParam, lParam);
            }

            if (MouseHooked != null)
            {
                MouseHooked(this, new MouseEventArgs(Mouse.PrimaryDevice, Convert.ToInt32(DateTime.Now.ToFileTime())));
            }

            return Win32Api.CallNextHookEx(_hHook, code, wParam, lParam);
        }

        private IntPtr _hMod;
        private IntPtr _hThread;
        private IntPtr _hHook;
        private HookProcedure _hookProc;
        private Boolean _enabled;
    }
}
