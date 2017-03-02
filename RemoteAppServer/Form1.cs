using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Hook;
using System.Diagnostics;
using System.Threading;
using RemoteApp;

namespace RemoteAppServer
{
    public partial class Form1 : Form
    {
        //勾子管理类 
        public KeyboardHookLib _keyboardHook = null;

        private IntPtr mHandle;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //启动U8客户端
            Process.Start(@"c:\windows\system32\notepad.exe");

            //初始化到客户端的连接，并启动对客户端消息的监听
            Thread thread = new Thread(ConnectAndListen);
            thread.Start();

            //开启一个后端线程，不断运行，存储非null的foreground窗口的handle
            Thread handleThread = new Thread(SetNotZeroForeGroundHandle);
            handleThread.Start();

            //禁用服务端
            btnInstallHook_Click(this,null);
        }

        private void SetNotZeroForeGroundHandle()
        {
            while (true)
            {
                IntPtr currentHandle = FindWindow.GetForegroundWindow();
                if (currentHandle != IntPtr.Zero&&currentHandle!=WinHelper.GetDesktopWindow())
                {
                    WinHelper.foreGroundHandle = currentHandle;
                }
            }
        }

        private void ConnectAndListen()
        { 
            //TSTunnels.Server.Server server = new TSTunnels.Server.Server();
            //server.Connect(_keyboardHook);
            Connect();
        }
        
        private void btnInstallHook_Click(object sender, EventArgs e)
        {
            //安装勾子 
            _keyboardHook = new KeyboardHookLib();
            _keyboardHook.InstallHook(this.OnKeyPress);
        }

        private void btnUninstallHook_Click(object sender, EventArgs e)
        {
            //取消勾子 
            if (_keyboardHook != null)
            {
                _keyboardHook.UninstallHook();
            }
        }

        /// <summary> 
        /// 客户端键盘捕捉事件. 
        /// </summary> 
        /// <param name="hookStruct">由Hook程序发送的按键信息</param> 
        /// <param name="handle">是否拦截</param> 
        public void OnKeyPress(KeyboardHookLib.HookStruct hookStruct, out bool handle)
        {
            handle = false; //预设不拦截任何键 

            if (hookStruct.vkCode == 91) // 截获左win(开始菜单键)
            {
                handle = true;
            }

            if (hookStruct.vkCode == 92)// 截获右win
            {
                handle = true;
            }

            //截获Ctrl+Esc 
            if (hookStruct.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Control)
            {
                handle = true;
            }

            //截获alt+f4 
            if (hookStruct.vkCode == (int)Keys.F4 && (int)Control.ModifierKeys == (int)Keys.Alt)
            {
                handle = true;
            }

            //截获alt+tab 
            if (hookStruct.vkCode == (int)Keys.Tab && (int)Control.ModifierKeys == (int)Keys.Alt)
            {
                handle = true;
            }

            //截获F1 
            if (hookStruct.vkCode == (int)Keys.F1)
            {
                handle = true;
            }

            //截获Ctrl+Alt+Delete 
            if ((int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Alt + (int)Keys.Delete)
            {
                handle = true;
            }

            //如果键A~Z 
            if (hookStruct.vkCode >= (int)Keys.A && hookStruct.vkCode <= (int)Keys.Z)
            {
                //挡掉A~Z键 
                hookStruct.vkCode = (int)Keys.None; //设键为0 

                handle = true;
            }

            //如果主键盘区0~9
            if (hookStruct.vkCode >= (int)Keys.D0 && hookStruct.vkCode <= (int)Keys.D9)
            { 
                //挡掉主键盘区0~9
                hookStruct.vkCode = (int)Keys.None;

                handle = true;
            }

            //如果小键盘去0~9
            if (hookStruct.vkCode >= (int)Keys.NumPad0 && hookStruct.vkCode <= (int)Keys.NumPad9)
            { 
               //挡掉小键盘区0~9
                hookStruct.vkCode = (int)Keys.None;

                handle = true;
            }

            //拦截PrintScreen
            if (hookStruct.vkCode == (int)Keys.PrintScreen)
            {
                handle = true;
            }

            Keys key = (Keys)hookStruct.vkCode;
            label1.Text = "你按下：" + (key == Keys.None ? "" : key.ToString());

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            //隐藏服务端窗体
            this.Hide();
        }

        public void Connect()
        {
            mHandle = WtsApi32.WTSVirtualChannelOpen(IntPtr.Zero, -1, ChannelMessage.ChannelName);

            if (mHandle == IntPtr.Zero)
            {
                //Log("RDP Virtual channel Open Failed: " + new Win32Exception().Message);
                return;
            }

            try
            {
                byte[] buf = new byte[1024];
                uint bytesRead;
                string text = "";
                while (true)
                {
                    if (WtsApi32.WTSVirtualChannelRead(mHandle, 0, buf, (uint)buf.Length, out bytesRead) != 0)
                    {
                        text = Encoding.Unicode.GetString(buf, 0, (int)bytesRead);
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        #region old
                        /*string result = "";
                        for (int i = 0; i < text.Length; i++)
                        {
                            if ((int)text[i] > 32 && (int)text[i] < 127)
                            {
                                result += text[i].ToString();
                            }
                            else
                            {
                                result += string.Format("\\u{0:x4}", (int)text[i]);
                            }
                        }

                        string tmpResult = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                             result, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));*/


                        /* //1、获取桌面窗口的句柄
                        IntPtr desktopPtr = WinHelper.GetDesktopWindow();
                        Write(desktopPtr.ToString() + ":" + GetHandleInfo(desktopPtr) + "\r\n");
                        //2、获得一个子窗口（这通常是一个顶层窗口，当前活动的窗口）
                        IntPtr winPtr = WinHelper.GetWindow(desktopPtr, GetWindowCmd.GW_CHILD);
                        Write(winPtr.ToString() + ":" + GetHandleInfo(winPtr) + "\r\n");
                        //3、循环取得桌面下的所有子窗口
                        while (winPtr != IntPtr.Zero)
                        {
                            //4、继续获取下一个子窗口
                            winPtr = WinHelper.GetWindow(winPtr, GetWindowCmd.GW_HWNDNEXT);
                            Write(winPtr.ToString() + ":" + GetHandleInfo(winPtr) + "\r\n");
                            if (GetHandleInfo(winPtr).Contains("记事本"))
                            {
                                break;
                            }
                        }*/
                        #endregion

                        if (text.Contains("使用服务端输入法"))
                        {
                            //解除键盘监控钩子 
                            btnUninstallHook_Click(this, null);
                        }
                        else if (text.Contains("使用客户端输入法"))
                        {
                            btnInstallHook_Click(this,null);
                        }
                        else
                        {
                            WinHelper.SetForegroundWindow(WinHelper.foreGroundHandle);
                            //WinHelper.SendMessage(WinHelper.foreGroundHandle, 11, IntPtr.Zero, IntPtr.Zero);
                            //WinHelper.SendMessage(WinHelper.foreGroundHandle, 11, (IntPtr)(-1), IntPtr.Zero);
                            WinHelper.SendText(text, WinHelper.foreGroundHandle);
                           
                        }
                    }
                }

            }
            catch (Win32Exception ex)
            {
                //Log("RDP Virtual channel Query Failed: " + ex.Message);
                return;
            }
        }
    }
}
