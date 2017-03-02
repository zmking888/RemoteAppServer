using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AxMSTSCLib;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using TSTunnels.Client.WtsApi32;

namespace RemoteApp
{
    public partial class Form1 : Form
    {
        IntPtr mHandle = IntPtr.Zero;
        AxMsRdpClient7NotSafeForScripting rdc = new AxMsRdpClient7NotSafeForScripting();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //加载配置文件，并显示信息到界面
            GetConfigInfo();

            //连接remoteapp
            ConnectRemoteApp();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetConfigInfo();
        }

        #region old
        //private void OpenVirtualChannel()
        //{
        //    mHandle = WtsApi32.WTSVirtualChannelOpen(IntPtr.Zero, -1, "TSCS");
        //    try
        //    {
        //        string testString = "Hello World";
        //        //MemoryStream ms = new MemoryStream();
        //        //GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
        //        //FileStream fs = File.OpenRead();
        //        byte[] buffer = new byte[1024];
        //        //int bytesRead = 0;
        //        //while ((bytesRead = fs.Read(buffer, 0, 1024)) != 0)
        //        //{
        //        //    gs.Write(buffer, 0, bytesRead);
        //        //}
        //        //gs.Close();
        //        byte[] gziped = System.Text.Encoding.UTF8.GetBytes(testString);
        //        //ms.Position = 0;
        //        //ms.Read(gziped, 0, (int)ms.Length);
        //        int written = 0;
        //        bool ret = WtsApi32.WTSVirtualChannelWrite(mHandle, gziped, gziped.Length, ref written);
        //        if (ret || written == gziped.Length)
        //            MessageBox.Show("Sent!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        else
        //            MessageBox.Show("Bumm! Somethings gone wrong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Somethings gone wrong:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
        #endregion

        private void ConnectRemoteApp() {
            rdc.Dock = DockStyle.Fill;
            this.Controls.Add(rdc);
            //this.Hide(); //这里是Form1在加载的时候就陈隐藏掉，这样就只看到RemoteAPP了，估计微软也是这样处理的
            rdc.RemoteProgram2.RemoteProgramMode = true;
            rdc.OnConnected += (_1, _2) =>
            {
                rdc.RemoteProgram2.ServerStartProgram(txtPath.Text.Trim(), "", "%SYSTEMROOT%", true, "", false);
            };
            rdc.Server = txtServer.Text.Trim();
            rdc.UserName = string.IsNullOrEmpty(txtDomain.Text.Trim()) ? txtUser.Text.ToString() : txtDomain.Text.ToString() + "\\" + txtUser.Text.ToString(); //注意这里的用户名格式
            rdc.AdvancedSettings7.ClearTextPassword = txtPass.Text.Trim();  //用户名密码
            //rdc.AdvancedSettings7.EnableCredSspSupport = true;
            //rdc.AdvancedSettings7.PublicMode = false;
            rdc.DesktopWidth = SystemInformation.VirtualScreen.Width;
           
            rdc.DesktopHeight = SystemInformation.VirtualScreen.Height;
            rdc.AdvancedSettings7.SmartSizing = true;
            rdc.AdvancedSettings7.keepAliveInterval = 5000;
            //rdc.AdvancedSettings7.DisplayConnectionBar = true;


            //rdc.AdvancedSettings7.CanAutoReconnect = true;
            rdc.AdvancedSettings7.DisableCtrlAltDel = 1;
            rdc.AdvancedSettings7.DisplayConnectionBar = true;

            rdc.AdvancedSettings7.EnableAutoReconnect=cbAutoConnect.Checked?true:false;
            
            rdc.AdvancedSettings7.keepAliveInterval = 5000;
            rdc.CreateVirtualChannels("TSTnls");
            //rdc.SendOnVirtualChannel("TSTnls", "Hello");
            rdc.FullScreen = true;

            //禁用ctrl+alt+del，只允许remoteapp，不允许远程桌面登录
            rdc.AdvancedSettings7.DisableCtrlAltDel = 1;
            rdc.AdvancedSettings7.EnableWindowsKey = -1;
           
            //rdc.AdvancedSettings3.
          

            try
            {
                rdc.Connect();
                /*rdc.OnConnected += (object sender, EventArgs e) =>
                {
                    Thread thread = new Thread(sendMessage);
                    thread.Start();
                };*/
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           

        }

       /* private void sendMessage()
        {
            int i = 1;
            while (true)
            {
                Thread.Sleep(20000);
                rdc.SendOnVirtualChannel("TSTnls", "jj");
                //i++;
                if (i > 5)
                {
                    break;
                }
            }
        }*/

        private void GetConfigInfo()
        {
            #region 加载配置文件，从配置文件中读取相应信息
            Config config = new Config();
            Configs list = (Configs)XmlHelper.XmlDeserializeFromFile(AppDomain.CurrentDomain.BaseDirectory + "RemoteApp.xml", Encoding.UTF8);
            if (list.Datas != null && list.Datas.Count > 0)
            {
                config = list.Datas[0];
            }
            else
            {
                MessageBox.Show("没有找到配置文件！");
            }
            #endregion

            #region 将配置信息默认加载到界面上
            txtPath.Text = config.RemoteProgramePath;
            txtServer.Text = config.Server;
            txtDomain.Text = config.Domain;
            txtUser.Text = config.UserName;
            txtPass.Text = config.Password;
            cbAutoConnect.Checked = config.AutoConnect ? true : false;
            #endregion
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetConfigInfo();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendMessage("大阿朵司法所地方");
        }

        private void SendMessage(string messageContent)
        {
            string result = "";
            if (messageContent != null)
            {
                for (int i = 0; i < messageContent.Length; i++)
                {
                    if ((int)messageContent[i] > 32 && (int)messageContent[i] < 127)
                    {
                        result += messageContent[i].ToString();
                    }
                    else
                    {
                        result += string.Format("\\u{0:x4}", (int)messageContent[i]);
                    }
                }

                rdc.SendOnVirtualChannel("TSTnls", result);
            }
        }

        /*private void txtMonitor_TextChanged(object sender, EventArgs e)
        {
            //每次隐藏监控文字输入控件中文字的值发生变化时，就发送消息到服务端
            string monitorText = txtMonitor.Text.ToString().Trim();
            SendMessage(monitorText);
        }*/

        #region 监听键盘输入法结果

        private GetComposition getImmStr = new GetComposition();
        string immStr;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)                               //判断系统消息的ID号     
            {
                case (int)emWinMsg.eWM_KEYDOWN:
                    base.WndProc(ref m);
                    break;
                case (int)emWinMsg.eWM_IME_CHAR:
                    //截获输入法结果
                    immStr = getImmStr.CurrentCompStr(this.Handle);
                    SendMessage(immStr);
                    base.WndProc(ref m);
                    break;
                default:
                    //immStr = getImmStr.CurrentCompStr(this.Handle);
                    //SendMessage(immStr);
                    base.WndProc(ref m);
                    break;
            }
        }

        public class GetComposition
        {
            [DllImport("Imm32.dll")]
            public static extern IntPtr ImmGetContext(IntPtr hWnd);
            [DllImport("Imm32.dll")]
            public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
            [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
            private static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);

            public string CurrentCompStr(IntPtr handle)
            {
                IntPtr hIMC = ImmGetContext(handle);
                try
                {
                    int strLen = ImmGetCompositionStringW(hIMC, 0x0800, null, 0);

                    if (strLen > 0)
                    {
                        byte[] buffer = new byte[strLen];
                        ImmGetCompositionStringW(hIMC, 0x0800, buffer, strLen);
                        return Encoding.Unicode.GetString(buffer);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                finally
                {
                    ImmReleaseContext(handle, hIMC);
                }
            }
        }

        public enum emWinMsg
        {
            eWM_NULL = 0x0000,
            eWM_CREATE = 0x0001,
            eWM_DESTROY = 0x0002,
            eWM_MOVE = 0x0003,
            eWM_SIZE = 0x0005,
            eWM_ACTIVATE = 0x0006,
            eWM_SETFOCUS = 0x0007,
            eWM_KILLFOCUS = 0x0008,
            eWM_ENABLE = 0x000A,
            eWM_SETREDRAW = 0x000B,
            eWM_SETTEXT = 0x000C,
            eWM_GETTEXT = 0x000D,
            eWM_GETTEXTLENGTH = 0x000E,
            eWM_PAINT = 0x000F,
            eWM_CLOSE = 0x0010,
            eWM_QUERYENDSESSION = 0x0011,
            eWM_QUIT = 0x0012,
            eWM_QUERYOPEN = 0x0013,
            eWM_ERASEBKGND = 0x0014,
            eWM_SYSCOLORCHANGE = 0x0015,
            eWM_ENDSESSION = 0x0016,
            eWM_SHOWWINDOW = 0x0018,
            eWM_CTLCOLOR = 0x0019,
            eWM_WININICHANGE = 0x001A,
            eWM_SETTINGCHANGE = 0x001A,
            eWM_DEVMODECHANGE = 0x001B,
            eWM_ACTIVATEAPP = 0x001C,
            eWM_FONTCHANGE = 0x001D,
            eWM_TIMECHANGE = 0x001E,
            eWM_CANCELMODE = 0x001F,
            eWM_SETCURSOR = 0x0020,
            eWM_MOUSEACTIVATE = 0x0021,
            eWM_CHILDACTIVATE = 0x0022,
            eWM_QUEUESYNC = 0x0023,
            eWM_GETMINMAXINFO = 0x0024,
            eWM_PAINTICON = 0x0026,
            eWM_ICONERASEBKGND = 0x0027,
            eWM_NEXTDLGCTL = 0x0028,
            eWM_SPOOLERSTATUS = 0x002A,
            eWM_DRAWITEM = 0x002B,
            eWM_MEASUREITEM = 0x002C,
            eWM_DELETEITEM = 0x002D,
            eWM_VKEYTOITEM = 0x002E,
            eWM_CHARTOITEM = 0x002F,
            eWM_SETFONT = 0x0030,
            eWM_GETFONT = 0x0031,
            eWM_SETHOTKEY = 0x0032,
            eWM_GETHOTKEY = 0x0033,
            eWM_QUERYDRAGICON = 0x0037,
            eWM_COMPAREITEM = 0x0039,
            eWM_GETOBJECT = 0x003D,
            eWM_COMPACTING = 0x0041,
            eWM_COMMNOTIFY = 0x0044,
            eWM_WINDOWPOSCHANGING = 0x0046,
            eWM_WINDOWPOSCHANGED = 0x0047,
            eWM_POWER = 0x0048,
            eWM_COPYDATA = 0x004A,
            eWM_CANCELJOURNAL = 0x004B,
            eWM_NOTIFY = 0x004E,
            eWM_INPUTLANGCHANGEREQUEST = 0x0050,
            eWM_INPUTLANGCHANGE = 0x0051,
            eWM_TCARD = 0x0052,
            eWM_HELP = 0x0053,
            eWM_USERCHANGED = 0x0054,
            eWM_NOTIFYFORMAT = 0x0055,
            eWM_CONTEXTMENU = 0x007B,
            eWM_STYLECHANGING = 0x007C,
            eWM_STYLECHANGED = 0x007D,
            eWM_DISPLAYCHANGE = 0x007E,
            eWM_GETICON = 0x007F,
            eWM_SETICON = 0x0080,
            eWM_NCCREATE = 0x0081,
            eWM_NCDESTROY = 0x0082,
            eWM_NCCALCSIZE = 0x0083,
            eWM_NCHITTEST = 0x0084,
            eWM_NCPAINT = 0x0085,
            eWM_NCACTIVATE = 0x0086,
            eWM_GETDLGCODE = 0x0087,
            eWM_SYNCPAINT = 0x0088,
            eWM_NCMOUSEMOVE = 0x00A0,
            eWM_NCLBUTTONDOWN = 0x00A1,
            eWM_NCLBUTTONUP = 0x00A2,
            eWM_NCLBUTTONDBLCLK = 0x00A3,
            eWM_NCRBUTTONDOWN = 0x00A4,
            eWM_NCRBUTTONUP = 0x00A5,
            eWM_NCRBUTTONDBLCLK = 0x00A6,
            eWM_NCMBUTTONDOWN = 0x00A7,
            eWM_NCMBUTTONUP = 0x00A8,
            eWM_NCMBUTTONDBLCLK = 0x00A9,
            eWM_KEYDOWN = 0x0100,
            eWM_KEYUP = 0x0101,
            eWM_CHAR = 0x0102,
            eWM_DEADCHAR = 0x0103,
            eWM_SYSKEYDOWN = 0x0104,
            eWM_SYSKEYUP = 0x0105,
            eWM_SYSCHAR = 0x0106,
            eWM_SYSDEADCHAR = 0x0107,
            eWM_KEYLAST = 0x0108,
            eWM_IME_STARTCOMPOSITION = 0x010D,
            eWM_IME_ENDCOMPOSITION = 0x010E,
            eWM_IME_COMPOSITION = 0x010F,
            eWM_IME_KEYLAST = 0x010F,
            eWM_INITDIALOG = 0x0110,
            eWM_COMMAND = 0x0111,
            eWM_SYSCOMMAND = 0x0112,
            eWM_TIMER = 0x0113,
            eWM_HSCROLL = 0x0114,
            eWM_VSCROLL = 0x0115,
            eWM_INITMENU = 0x0116,
            eWM_INITMENUPOPUP = 0x0117,
            eWM_MENUSELECT = 0x011F,
            eWM_MENUCHAR = 0x0120,
            eWM_ENTERIDLE = 0x0121,
            eWM_MENURBUTTONUP = 0x0122,
            eWM_MENUDRAG = 0x0123,
            eWM_MENUGETOBJECT = 0x0124,
            eWM_UNINITMENUPOPUP = 0x0125,
            eWM_MENUCOMMAND = 0x0126,
            eWM_CTLCOLORWinMsgBOX = 0x0132,
            eWM_CTLCOLOREDIT = 0x0133,
            eWM_CTLCOLORLISTBOX = 0x0134,
            eWM_CTLCOLORBTN = 0x0135,
            eWM_CTLCOLORDLG = 0x0136,
            eWM_CTLCOLORSCROLLBAR = 0x0137,
            eWM_CTLCOLORSTATIC = 0x0138,
            eWM_MOUSEMOVE = 0x0200,
            eWM_LBUTTONDOWN = 0x0201,
            eWM_LBUTTONUP = 0x0202,
            eWM_LBUTTONDBLCLK = 0x0203,
            eWM_RBUTTONDOWN = 0x0204,
            eWM_RBUTTONUP = 0x0205,
            eWM_RBUTTONDBLCLK = 0x0206,
            eWM_MBUTTONDOWN = 0x0207,
            eWM_MBUTTONUP = 0x0208,
            eWM_MBUTTONDBLCLK = 0x0209,
            eWM_MOUSEWHEEL = 0x020A,
            eWM_PARENTNOTIFY = 0x0210,
            eWM_ENTERMENULOOP = 0x0211,
            eWM_EXITMENULOOP = 0x0212,
            eWM_NEXTMENU = 0x0213,
            eWM_SIZING = 0x0214,
            eWM_CAPTURECHANGED = 0x0215,
            eWM_MOVING = 0x0216,
            eWM_DEVICECHANGE = 0x0219,
            eWM_MDICREATE = 0x0220,
            eWM_MDIDESTROY = 0x0221,
            eWM_MDIACTIVATE = 0x0222,
            eWM_MDIRESTORE = 0x0223,
            eWM_MDINEXT = 0x0224,
            eWM_MDIMAXIMIZE = 0x0225,
            eWM_MDITILE = 0x0226,
            eWM_MDICASCADE = 0x0227,
            eWM_MDIICONARRANGE = 0x0228,
            eWM_MDIGETACTIVE = 0x0229,
            eWM_MDISETMENU = 0x0230,
            eWM_ENTERSIZEMOVE = 0x0231,
            eWM_EXITSIZEMOVE = 0x0232,
            eWM_DROPFILES = 0x0233,
            eWM_MDIREFRESHMENU = 0x0234,
            eWM_IME_SETCONTEXT = 0x0281,
            eWM_IME_NOTIFY = 0x0282,
            eWM_IME_CONTROL = 0x0283,
            eWM_IME_COMPOSITIONFULL = 0x0284,
            eWM_IME_SELECT = 0x0285,
            eWM_IME_CHAR = 0x0286,
            eWM_IME_REQUEST = 0x0288,
            eWM_IME_KEYDOWN = 0x0290,
            eWM_IME_KEYUP = 0x0291,
            eWM_MOUSEHOVER = 0x02A1,
            eWM_MOUSELEAVE = 0x02A3,
            eWM_CUT = 0x0300,
            eWM_COPY = 0x0301,
            eWM_PASTE = 0x0302,
            eWM_CLEAR = 0x0303,
            eWM_UNDO = 0x0304,
            eWM_RENDERFORMAT = 0x0305,
            eWM_RENDERALLFORMATS = 0x0306,
            eWM_DESTROYCLIPBOARD = 0x0307,
            eWM_DRAWCLIPBOARD = 0x0308,
            eWM_PAINTCLIPBOARD = 0x0309,
            eWM_VSCROLLCLIPBOARD = 0x030A,
            eWM_SIZECLIPBOARD = 0x030B,
            eWM_ASKCBFORMATNAME = 0x030C,
            eWM_CHANGECBCHAIN = 0x030D,
            eWM_HSCROLLCLIPBOARD = 0x030E,
            eWM_QUERYNEWPALETTE = 0x030F,
            eWM_PALETTEISCHANGING = 0x0310,
            eWM_PALETTECHANGED = 0x0311,
            eWM_HOTKEY = 0x0312,
            eWM_PRINT = 0x0317,
            eWM_PRINTCLIENT = 0x0318,
            eWM_HANDHELDFIRST = 0x0358,
            eWM_HANDHELDLAST = 0x035F,
            eWM_AFXFIRST = 0x0360,
            eWM_AFXLAST = 0x037F,
            eWM_PENWINFIRST = 0x0380,
            eWM_PENWINLAST = 0x038F,
            eWM_APP = 0x8000,
            eWM_USER = 0x0400,
            eWM_REFLECT = eWM_USER + 0x1c00
        }

        #endregion

    }
}
