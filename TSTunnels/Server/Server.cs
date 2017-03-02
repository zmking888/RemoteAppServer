using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TSTunnels.Common;
using TSTunnels.Common.Messages;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Hook;

namespace TSTunnels.Server
{
    public class Server : IStreamServer
    {
        private IntPtr mHandle;
        private BinaryReader reader;
        //记事本句柄
        public IntPtr notepadHandle = IntPtr.Zero;

        public bool IsConnected { get; private set; }
        private bool SeenHello;

        public delegate void CreatePort(string listenAddress, int listenPort, string connectAddress, int connectPort);

        class ClientListener
        {
            public Action<ListenResponse> ListenResponse;
            public Action<StreamError> StreamError;
            public Action<AcceptRequest> AcceptRequest;
        }
        private readonly Dictionary<int, ClientListener> clientListeners = new Dictionary<int, ClientListener>();

        #region Events

        public EventHandler<ConnectedEventArgs> Connected;

        protected void OnConnected(string machineName)
        {
            if (Connected != null)
                Connected(this, new ConnectedEventArgs(machineName));
        }

        public EventHandler<ForwardedPortEventArgs> ForwardedPortAdded;

        protected void OnForwardedPortAdded(ForwardedPort forwardedPort)
        {
            if (ForwardedPortAdded != null)
                ForwardedPortAdded(this, new ForwardedPortEventArgs(forwardedPort));
        }

        public EventHandler<ForwardedPortEventArgs> ForwardedPortRemoved;

        protected void OnForwardedPortRemoved(ForwardedPort forwardedPort)
        {
            if (ForwardedPortRemoved != null)
                ForwardedPortRemoved(this, new ForwardedPortEventArgs(forwardedPort));
        }

        public EventHandler<MessageLoggedEventArgs> MessageLogged;

        protected void OnMessageLogged(string message)
        {
            if (MessageLogged != null)
                MessageLogged(this, new MessageLoggedEventArgs(message));
        }

        #endregion

        public Server()
        {
            Streams = new Dictionary<int, Stream>();
            Listeners = new Dictionary<int, TcpListener>();
        }

        private delegate void Action();

        public  void Write(string text)
        {
            FileStream fs = new FileStream(@"E:\1.txt", FileMode.Append, FileAccess.Write);
            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(text);
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }

        public  string GetHandleInfo(IntPtr ptr)
        {
            StringBuilder s = new StringBuilder(512);
            int p = WinHelper.GetWindowText(ptr, s, s.Capacity);
            return s.ToString();
        }

        public void Connect(KeyboardHookLib _keyboardHook)
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
                string text="";
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
                            if (_keyboardHook != null)
                            {
                                _keyboardHook.UninstallHook();
                            }
                        }
                        else if (text.Contains("使用客户端输入法"))
                        { 
                            
                        }
                        else
                        {
                            WinHelper.SendText(text, WinHelper.foreGroundHandle);
                        }

                        WinHelper.SetForegroundWindow(WinHelper.foreGroundHandle);
                    } 
                }

            }
            catch (Win32Exception ex)
            {
                Log("RDP Virtual channel Query Failed: " + ex.Message);
                return;
            }


            //IsConnected = true;

            //Action process = Process;
            //process.BeginInvoke(process.EndInvoke, null);
            ////process();

            //Action hello = () =>
            //{
            //    while (!SeenHello && IsConnected)
            //    {
            //        WriteMessage(new HelloRequest());
            //        Thread.Sleep(200);
            //    }
            //};
            //hello.BeginInvoke(hello.EndInvoke, null);
            ////hello();
        }

        public void Disconnect()
        {
            IsConnected = false;
            if (reader != null) reader.Close();
            var ret = WtsApi32.WTSVirtualChannelClose(mHandle);
        }

        private void Process()
        {
            while (IsConnected)
            {
                try
                {
                    var len = reader.ReadInt32();
                    var buff = reader.ReadBytes(len);
                    MessageReceived(ChannelMessage.FromByteArray(buff));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Log(ex);
                }
            }
        }

        public void CreateClientPort(string listenAddress, int listenPort, string connectAddress, int connectPort)
        {
            var listenIndex = ++ConnectionCount;
            ForwardedPort forwardedPort = null;
            var listener = new ClientListener
            {
                ListenResponse = response =>
                {
                    forwardedPort = new ForwardedPort
                    {
                        Direction = ForwardDirection.Local,
                        ListenEndPoint = listenAddress + ":" + listenPort,
                        ConnectEndPoint = connectAddress + ":" + connectPort,
                        Remove = () =>
                        {
                            Log("Cancelling local port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort + " requested");
                            WriteMessage(new StreamError(listenIndex, new EndOfStreamException()));
                        },
                    };
                    Log("Local port forwarding from " + listenAddress + ":" + listenPort + " enabled");
                    OnForwardedPortAdded(forwardedPort);
                },
                StreamError = error =>
                {
                    if (forwardedPort == null)
                    {
                        Log("Local port forwarding from " + listenAddress + ":" + listenPort + " failed: " + error.Message);
                        lock (clientListeners)
                        {
                            if (clientListeners.ContainsKey(listenIndex)) clientListeners.Remove(listenIndex);
                        }
                    }
                    else
                    {
                        Log("Cancelled local port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort);
                        OnForwardedPortRemoved(forwardedPort);
                    }
                },
                AcceptRequest = request =>
                {
                    Log("Opening forwarded connection " + request.RemoteEndPoint + " to " + connectAddress + ":" + connectPort);
                    new ConnectRequest(request.StreamIndex, request.RemoteEndPoint, connectAddress, connectPort).Process(this);
                },
            };
            lock (clientListeners)
            {
                clientListeners[listenIndex] = listener;
            }
            Log("Requesting local port " + listenAddress + ":" + listenPort + " forward to " + connectAddress + ":" + connectPort);
            WriteMessage(new ListenRequest(listenIndex, listenAddress, listenPort));
        }

        public void CreateServerPort(string listenAddress, int listenPort, string connectAddress, int connectPort)
        {
            try
            {
                var listenIndex = ++ConnectionCount;
                ForwardedPort forwardedPort = null;
                Listeners[listenIndex] = TcpListenerHelper.Start(listenAddress, listenPort, client =>
                {
                    var streamIndex = ++ConnectionCount;
                    Log("Attempting to forward remote port " + client.Client.RemoteEndPoint + " to " + connectAddress + ":" + connectPort);
                    Streams[streamIndex] = client.GetStream();
                    WriteMessage(new ConnectRequest(streamIndex, client.Client.RemoteEndPoint.ToString(), connectAddress, connectPort));
                }, exception =>
                {
                    if (forwardedPort == null)
                    {
                        Log("Remote port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort + " exception: " + exception);
                    }
                    else
                    {
                        Log("Cancelled remote port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort);
                        OnForwardedPortRemoved(forwardedPort);
                    }
                });
                Log("Remote port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort);
                forwardedPort = new ForwardedPort
                {
                    Direction = ForwardDirection.Remote,
                    ListenEndPoint = listenAddress + ":" + listenPort,
                    ConnectEndPoint = connectAddress + ":" + connectPort,
                    Remove = () =>
                    {
                        Log("Cancelling remote port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort);
                        new StreamError(listenIndex, new EndOfStreamException()).Process(this);
                    },
                };
                OnForwardedPortAdded(forwardedPort);
            }
            catch (Exception ex)
            {
                Log("Remote port " + listenAddress + ":" + listenPort + " forwarding to " + connectAddress + ":" + connectPort + " failed: " + ex);
            }
        }

        #region Implementation of IStreamServer

        public int ConnectionCount { get; set; }
        public IDictionary<int, TcpListener> Listeners { get; private set; }
        public IDictionary<int, Stream> Streams { get; private set; }

        public void Log(object message)
        {
            OnMessageLogged(message.ToString());
        }

        public void MessageReceived(ChannelMessage msg)
        {
            switch (msg.Type)
            {
                default:
                    Log("Unknown message: " + msg);
                    break;
                case MessageType.HelloResponse:
                    {
                        var response = (HelloResponse)msg;
                        if (!SeenHello)
                        {
                            SeenHello = true;
                            Log(response.MachineName + " connected to " + Environment.MachineName);
                            OnConnected(response.MachineName);
                        }
                    }
                    break;
                case MessageType.ListenResponse:
                    {
                        var response = (ListenResponse)msg;
                        ClientListener listener = null;
                        lock (clientListeners)
                        {
                            if (clientListeners.ContainsKey(response.ListenIndex)) listener = clientListeners[response.ListenIndex];
                        }
                        if (listener != null) listener.ListenResponse(response);
                    }
                    break;
                case MessageType.AcceptRequest:
                    {
                        var request = (AcceptRequest)msg;
                        ClientListener listener = null;
                        lock (clientListeners)
                        {
                            if (clientListeners.ContainsKey(request.ListenIndex)) listener = clientListeners[request.ListenIndex];
                        }
                        if (listener != null) listener.AcceptRequest(request);
                    }
                    break;
                case MessageType.ConnectResponse:
                    ((ConnectResponse)msg).Process(this);
                    break;
                case MessageType.StreamData:
                    ((StreamData)msg).Process(this);
                    break;
                case MessageType.StreamError:
                    {
                        var error = (StreamError)msg;
                        ClientListener listener = null;
                        lock (clientListeners)
                        {
                            if (clientListeners.ContainsKey(error.StreamIndex)) listener = clientListeners[error.StreamIndex];
                        }
                        if (listener != null)
                        {
                            listener.StreamError(error);
                        }
                        else
                        {
                            ((StreamError)msg).Process(this);
                        }
                    }
                    break;
            }
        }

        public bool WriteMessage(ChannelMessage msg)
        {
            var data = msg.ToByteArray();
            int written;
            var ret = WtsApi32.WTSVirtualChannelWrite(mHandle, data, data.Length, out written);
            if (ret) return true;
            var ex = new Win32Exception();
            if (!SeenHello && ex.NativeErrorCode == 1 /* Incorrect Function */) return false;
            Log("RDP Virtual channel Write Failed: " + ex.Message);
            return false;
        }

        #endregion
    }
}
