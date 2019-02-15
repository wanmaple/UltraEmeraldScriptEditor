using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace EditorSupport.Utils
{
    internal delegate Int32 HookProcedure(Int32 code, Int32 wParam, Int32 lParam);

    internal static class Win32Api
    {
        internal static readonly Int32 WH_MSGFILTER = -1;
        internal static readonly Int32 WH_JOURNALRECORD = 0;
        internal static readonly Int32 WH_JOURNALPLAYBACK = 1;
        internal static readonly Int32 WH_KEYBOARD = 2;
        internal static readonly Int32 WH_GETMESSAGE = 3;
        internal static readonly Int32 WH_CALLWNDPROC = 4;
        internal static readonly Int32 WH_CBT = 5;
        internal static readonly Int32 WH_SYSMSGFILTER = 6;
        internal static readonly Int32 WH_MOUSE = 7;
        internal static readonly Int32 WH_DEBUG = 9;
        internal static readonly Int32 WH_SHELL = 10;
        internal static readonly Int32 WH_FOREGROUNDIDLE = 11;
        internal static readonly Int32 WH_CALLWNDPROCRET = 12;
        internal static readonly Int32 WH_KEYBOARD_LL = 13;
        internal static readonly Int32 WH_MOUSE_LL = 14;

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
        internal extern static IntPtr SetWindowsHookEx(Int32 idHook, HookProcedure hookProc, IntPtr hMod, IntPtr hThread);

        [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx", SetLastError = true)]
        internal extern static Boolean UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll", EntryPoint = "CallNextHookEx", SetLastError = true)]
        internal extern static Int32 CallNextHookEx(IntPtr hHook, Int32 code, Int32 wParam, Int32 lParam);
    }
}
