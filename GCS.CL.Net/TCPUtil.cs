using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GCS.CL.Net
{
    static public class RobotProtocol
    {
        public const string REQ_HEARTBIT = "0010 0000 0000 8001 0000 0000 0000 0000";
        public const string RES_HEARTBIT = "0010 0000 0000 0001 0000 0000 0000 0000";
        public const string REQ_EXECUTE = "000C 0001 0000 8002 0000 0002";
        public const string RES_EXECUTE = "000C 0001 0000 0002 0000 0002";
        public const string REQ_MOVE_B = "000C 0002 0000 8003 0000 0041";
        public const string RES_MOVE_B = "000C 0002 0000 0003 0000 0041";
        public const string RES_MOVE_BEND = "000C 0002 0000 2003 0000 0041";

        public const string REQ_UNLOAD_B = "000C 0003 0000 8005 0000 0042";
        public const string RES_UNLOAD_B = "000C 0003 0000 0005 0000 0042";
        public const string RES_UNLOAD_BEND = "000C 0003 0000 2005 0000 0042";

        public const string REQ_MOVE_C = "000C 0004 0000 8003 0000 0043";
        public const string RES_MOVE_C = "000C 0004 0000 0003 0000 0043";
        public const string RES_MOVE_CEND = "000C 0004 0000 2003 0000 0043";

        public const string REQ_LOAD_C = "000C 0005 0000 8004 0000 0042";
        public const string RES_LOAD_C = "000C 0005 0000 0004 0000 0043";
        public const string RES_LOAD_CEND = "000C 0005 0000 2004 0000 0043";

        public const string REQ_MOVE_D = "000C 0006 0000 8003 0000 0044";
        public const string RES_MOVE_D = "000C 0006 0000 0003 0000 0044";
        public const string RES_MOVE_DEND = "000C 0006 0000 2003 0000 0044";

        public const string REQ_UNLOAD_D = "000C 0007 0000 8005 0000 0044";
        public const string RES_UNLOAD_D = "000C 0007 0000 0005 0000 0044";
        public const string RES_UNLOAD_DEND = "000C 0007 0000 2005 0000 0044";

        public const string REQ_MOVE_B2 = "000C 0008 0000 8003 0000 0042";
        public const string RES_MOVE_B2 = "000C 0008 0000 0003 0000 0042";
        public const string RES_MOVE_B2END = "000C 0008 0000 2003 0000 0042";

        public const string REQ_LOAD_B2 = "000C 0009 0000 8004 0000 0042";
        public const string RES_LOAD_B2 = "000C 0009 0000 0004 0000 0042";
        public const string RES_LOAD_B2END = "000C 0009 0000 2004 0000 0042";

        public const string RES_SCENARIO_END = "000C 0001 0000 2002 0000 0042";

        public const string REQ_MOVE_E = "000C 000A 0000 8003 0000 0045";
        public const string RES_MOVE_E = "000C 000A 0000 2003 0000 0045";
        public const string RES_MOVE_EEND = "000C 000A 0000 2003 0000 0045";

        public static byte[] ConvertToHexaStringToByte(string textdata)
        {
            textdata = textdata.Replace(" ", "");
            byte[] data = new byte[textdata.Length / 2];

            for (int i = 0; i < textdata.Length / 2; ++i)
                data[i] = Convert.ToByte(textdata.Substring(i * 2, 2), 16);

            return data;
        }
        public static string ConvertByteToHexaString(byte[] datas)
        {
            StringBuilder textHex = new StringBuilder(datas.Length * 2);

            for (int i = 0; i < datas.Length; ++i)
            {
                textHex.AppendFormat("{0:X2}", datas[i]);
                if (i % 2 == 1) textHex.Append(" ");
            }
            return textHex.ToString().Trim();
        }
    }

    public class Form1
    {
        public void AddDebugText(string text)
        {
            Console.WriteLine(text);
        }

    }
    public class SimpleTcp
    {
        //===================================================================== Client
        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 256;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        Socket clientSocket;
        Form1 calledForm;
        int port;
        string ip;
        List<string> listReceives;

        public delegate void OnReceiveMsgMessageClinetDelegate(string textReceive, byte[] bytesReceive);
        public event OnReceiveMsgMessageClinetDelegate OnReceiveMsgMessageClient;


        public bool IsConnect
        {
            get { return (clientSocket != null && clientSocket.Connected); }
        }
        public void DebugText(string text)
        {
            if (calledForm != null)
                calledForm.AddDebugText(text);
            else
                Console.WriteLine(text);
        }
        public Form1 MessageForm
        {
            get { return calledForm; }
            set { calledForm = value; }
        }
        public void Connect(string aip, int aport)
        {
            port = aport;
            ip = aip;

            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                DebugText("tryconnect" + ip + ":" + aport.ToString());
                //Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);

                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.BeginConnect(ipe, new AsyncCallback(ConnectCallback), clientSocket);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                DebugText("ConnectError " + e.ToString());
            }
        }
        public void Close()
        {
            if (clientSocket.Connected == true)
                clientSocket.Shutdown(SocketShutdown.Both);

            clientSocket.Close();
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                Receive(client);
                DebugText("Connect Succesful");
            }
            catch (Exception e)
            {
                DebugText("Connect Error:" + e.ToString());
                clientSocket.Close();
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(
                    state.buffer,
                    0,
                    StateObject.BufferSize,
                    0,
                    new AsyncCallback(ReceiveCallback),
                    state);
            }
            catch (Exception e)
            {
                DebugText("Receive Error:" + e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    var reciveText = string.Format(Encoding.UTF8.GetString(state.buffer));
                    OnReceiveMsgMessageClient(reciveText, state.buffer);

                    if (listReceives == null)
                        listReceives = new List<string>();

                    listReceives.Add(reciveText);

                    Receive(client);
                    //DebugText("Response received:" + reciveText);
                }
                else
                {
                    DebugText("Response received Byte0, Close Socket");
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                DebugText("Receive Error:" + e.ToString());
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        public void Send(String data)
        {
            if (IsConnect == false)
            {
                DebugText("Not Ready~~");
                return;
            }
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            clientSocket.BeginSend(
                byteData,
                0,
                byteData.Length,
                0,
                new AsyncCallback(SendCallback),
                clientSocket);
        }
        public void Send(byte[] byteData, int length)
        {
            clientSocket.BeginSend(
                byteData,
                0,
                length,
                0,
                new AsyncCallback(SendCallback),
                clientSocket);
        }
        public void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            client.BeginSend(
                byteData,
                0,
                byteData.Length,
                0,
                new AsyncCallback(SendCallback),
                client);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                DebugText(string.Format("Send {0} bytes to Server.", bytesSent));
            }
            catch (Exception e)
            {
                DebugText("Send Error:" + e.ToString());
            }
        }


        // Client 샘플 
        //
        //simpleTcp tcpmodule = new simpleTcp();

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    tcpmodule.MessageForm = this;
        //    tcpmodule.Connect("192.168.0.10", 12100);
        //}
        //private void sendbtn_Click(object sender, EventArgs e)
        //{
        //    if (tcpmodule.IsConnect == true)
        //    {
        //        tcpmodule.Send(richTextBox2.Text);
        //        richTextBox2.Clear();
        //    }
        //}

        //===================================================================== Server  
        public enum SocketMessageType
        {
            Connect = 1000,
            DisConnect = 1001,
            StringMessage = 0,
        };
        public class SocketetMsg
        {
            public SocketMessageType code;
            public Socket socket;
            public string msg;
            public String ip;
            public String port;
        }
        Dictionary<Socket, SocketetMsg> mapSocket = new Dictionary<Socket, SocketetMsg>();



        public delegate void OnReceiveMsgMessageDelegate(SocketetMsg message);
        public event OnReceiveMsgMessageDelegate OnReceiveMsgMessage;

        public void AsyncListen(int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            var localip = string.Empty;
            foreach (var ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    localip = ip.ToString();
            }
            DebugText("IpAdress" + ipAddress.ToString());

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(localip), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipe);
            socket.Listen(100);
            AsyncAccept(socket);
        }
        private void AsyncAccept(Socket socket)
        {
            socket.BeginAccept(asyncResult =>
            {
                Socket client = socket.EndAccept(asyncResult);
                ServerReveive(client);
                AsyncAccept(socket);
                OnLocalMsg(SocketMessageType.Connect, "", client);
            }, null);
        }
        private void ServerReveive(Socket socket)
        {
            if (socket == null) return;
            byte[] data = new byte[1024];

            try
            {
                socket.BeginReceive(data, 0, data.Length, SocketFlags.None,
                    asyncResult =>
                    {
                        int length = socket.EndReceive(asyncResult);
                        if (length > 0)
                        {
                            string msg = Encoding.UTF8.GetString(data).Trim();
                            msg = msg.Trim("\0"[0]);
                            ServerReveive(socket);
                            OnLocalMsg(SocketMessageType.StringMessage, msg, socket);
                        }
                        else
                        {
                            OnLocalMsg(SocketMessageType.DisConnect, "", socket, false);
                        }

                    }, null);
            }
            catch (Exception ex)
            {
                DebugText("Server Receive Error:" + ex.ToString());
            }
        }
        public void ServerAsyncSend(Socket client, string p)
        {
            if (client == null) return;
            _ServerAsyncSend(client, p);
        }

        void _ServerAsyncSend(Socket client, string p, bool bl = true)
        {
            if (bl && client == null) return;
            byte[] data = new byte[1024];
            data = Encoding.UTF8.GetBytes(p);
            try
            {
                client.BeginSend(
                    data,
                    0,
                    data.Length,
                    SocketFlags.None,
                    asyncResult =>
                    {
                        client.EndSend(asyncResult);
                    },
                    null
                    );
            }
            catch (Exception)
            {
                if (client != null && mapSocket.ContainsKey(client))
                {
                    OnLocalMsg(SocketMessageType.DisConnect, "", client, false);
                }
            }
        }
        void OnLocalMsg(SocketMessageType code, string msg, Socket socket, bool bl = true)
        {
            if (OnReceiveMsgMessage != null)
            {
                SocketetMsg _SocketetMsg = new SocketetMsg();
                _SocketetMsg.socket = socket;
                _SocketetMsg.code = code;
                _SocketetMsg.msg = msg;
                if (bl)
                {
                    IPEndPoint IPpoint = (IPEndPoint)socket.RemoteEndPoint;
                    _SocketetMsg.ip = IPpoint.Address.ToString();
                    _SocketetMsg.port = IPpoint.Port.ToString();
                    OnReceiveMsgMessage(_SocketetMsg);
                    if (mapSocket.ContainsKey(socket))
                        mapSocket.Remove(socket);
                    mapSocket.Add(socket, _SocketetMsg);
                }
                else
                {
                    if (mapSocket.ContainsKey(socket))
                    {
                        _SocketetMsg.ip = mapSocket[socket].ip;
                        _SocketetMsg.port = mapSocket[socket].port;
                        OnReceiveMsgMessage(_SocketetMsg);
                        mapSocket.Remove(socket);
                    }
                }
            }
        }

        // Server 샘플 ...
        //simpleTcp tcpmodule= new simpleTcp();
        //
        //private void btnServerStart_Click(object sender, EventArgs e)
        //{
        //    tcpmodule.MessageForm = this;
        //    tcpmodule.OnReceiveMsgMessage += OnReceiveMsg;
        //    tcpmodule.AsyncListen(12000);
        //}
        //public void OnReceiveMsg(SocketetMsg rMessage)
        //{
        //    if (rMessage == null)
        //        return;

        //    switch (rMessage.code)
        //    {

        //        case SocketMessageType.StringMessage:
        //            AddDebugText(string.Format("Receive:{0}", rMessage.msg));
        //            tcpmodule.ServerAsyncSend(rMessage.socket, "return" + rMessage.msg);
        //            break;

        //        case SocketMessageType.Connect:

        //            AddDebugText
        //            (string.Format("The new client {0} has connected successfully ", rMessage.socket.RemoteEndPoint));
        //            break;

        //        case SocketMessageType.DisConnect:

        //            AddDebugText
        //            (string.Format("Client {0}  port {1} Connection Disconnect", rMessage.ip, rMessage.port));
        //            break;
        //    }
        //}

    }

    public class TCPUtil
    {
        /// <summary>
        /// 소켓 연결하려는 대상서버의 IP, Port 로 접속 가능 여부 확인
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CanConnect(string ip, int port)
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);

                    IAsyncResult ar = socket.BeginConnect(ip, port, null, null);
                    return ar.AsyncWaitHandle.WaitOne(100, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Connection attempt failed.", ex);
            }
        }



        /// <summary>
        /// 특정 아이피의 핑체크를 수행 한다.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool PingCheck(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingOptions po = new PingOptions()
                    {
                        DontFragment = true
                    };

                    byte[] buf = Encoding.ASCII.GetBytes("abcd");

                    PingReply reply = ping.Send(IPAddress.Parse(ip), 10, buf, po);

                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
