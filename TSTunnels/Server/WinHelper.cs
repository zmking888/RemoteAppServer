using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace TSTunnels.Server
{
    enum GetWindowCmd : uint
    {
        /// <summary>
        /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
        /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
        /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
        /// </summary>
        GW_HWNDFIRST = 0,
        /// <summary>
        /// 返回的句柄标识了在z序最低端的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
        /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
        /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
        /// </summary>
        GW_HWNDLAST = 1,
        /// <summary>
        /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
        /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
        /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
        /// </summary>
        GW_HWNDNEXT = 2,
        /// <summary>
        /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
        /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
        /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
        /// </summary>
        GW_HWNDPREV = 3,
        /// <summary>
        /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
        /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
        /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
        /// </summary>
        GW_OWNER = 4,
        /// <summary>
        /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
        /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
        /// </summary>
        GW_CHILD = 5,
        /// <summary>
        /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
        /// 如果无使能窗口，则获得的句柄与指定窗口相同。
        /// </summary>
        GW_ENABLEDPOPUP = 6
    }

    /*public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }*/

    public class WinHelper
    {
        public static IntPtr foreGroundHandle = IntPtr.Zero;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /*[DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);*/

        [DllImport("user32.dll")]
        public static extern IntPtr GetLastActivePopup(IntPtr hWnd); 

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        [DllImport("User32.dll", EntryPoint = "GetWindowText")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /*[DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);*/

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);


        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public Rect rectCaret;
        }

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static GUITHREADINFO? GetGuiThreadInfo(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                //Mbox.Info(GetTitle(hwnd), "O");
                uint threadId = GetWindowThreadProcessId(hwnd, IntPtr.Zero);

                GUITHREADINFO guiThreadInfo = new GUITHREADINFO();
                guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);

                if (GetGUIThreadInfo(threadId, ref guiThreadInfo) == false)
                    return null;
                return guiThreadInfo;
            }
            return null;
        }

        #region 对临时messagebox的操作
        /*private static void StartKiller()
        {
            Thread thread = new Thread(KillMessageBox);
            thread.Start();
        }

        private static void KillMessageBox()
        {
            while (true)
            {
                const uint WM_CLOSE = 0x0010;
                IntPtr tmpHandle = FindWindow(null, "临时窗口");
                SendMessage(tmpHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);  // 调用了 发送消息 发送关闭窗口的消息
            }
        }*/
        #endregion

        public static void SendText(string text,IntPtr winPtr)
        {
            //IntPtr hwnd = GetForegroundWindow();
            IntPtr hwnd = winPtr;

            //SetForegroundWindow(winPtr);

            if (String.IsNullOrEmpty(text))
                return ;
            WinHelper.GUITHREADINFO? guiInfo = WinHelper.GetGuiThreadInfo(hwnd);

            if (guiInfo != null)
            {
                IntPtr ptr = (IntPtr)guiInfo.Value.hwndCaret;

                //MessageBox.Show(winPtr.ToString()+";"+ptr.ToString());
                /*StringBuilder s = new StringBuilder(512);
                int p = GetWindowText(hwnd, s, s.Capacity);
                MessageBox.Show("获取到的句柄：" + ptr + ";窗口标题：" + s.ToString());*/
                if (ptr != IntPtr.Zero)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        SendMessage(ptr, 0x0102, (IntPtr)(int)text[i], IntPtr.Zero);
                    }
                }
            }
        }
    }
}
