using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GymSimulation
{
    #region MsgType
    /// <summary> 通信メッセージ種別 </summary>
    public enum MsgType
    {
        /// <summary> Tx:送信 </summary>
        Tx,
        /// <summary> Rx:受信 </summary>
        Rx,
        /// <summary> TxMsg:送信メッセージ </summary>
        TxMsg,
        /// <summary> RxMsg:受信メッセージ </summary>
        RxMsg,
    }
    #endregion
    public class TcpIF
    {
        #region Event Handler
        /// <summary> データ受信時のイベントハンドラ </summary>
        public event EventHandler TcpIFDataReceived;

        /// <summary> エラー発生時のイベントハンドラ </summary>
        public event EventHandler TcpIFErrorReceived;

        /// <summary> 受信タイムアウト発生時のイベントハンドラ </summary>
        public event EventHandler TcpIFTimeoutReceived;

        /// <summary> 通信ログ保存コールバック </summary>
        public delegate void WriteLogEventHandler(object sender, MsgType type, string log);
        /// <summary> 通信ログ保存イベントハンドラ </summary>
        public event WriteLogEventHandler TcpIFWriteLog;

        #endregion


        #region Property

        // socket TCP server listen message from clients
        public Socket serverSocket;

        /// <summary> 受信タイムアウト(msec) </summary>
        public int ReceiveTimeout { get; set; }

        /// <summary> 通信状態フラグ </summary>
        public bool IsOpen { get; private set; }

        /// <summary> 送信先IPアドレス </summary>
        public string SendIpAddr { get; set; }

        /// <summary> 送信先ポート番号 </summary>
        public int SendPortNo { get; set; }

        /// <summary> 受信IPアドレス(表示にのみ使用) </summary>
        public string RecvIpAddr { get; set; }
        /// <summary> 受信ポート番号 </summary>

        public int RecvPortNo { get; set; }

        /// <summary> TCP受信時の全受信データ </summary>
        public byte[] RxRawData { get; private set; }

        /// <summary> 受信タイムアウトタイマー </summary>
        private System.Timers.Timer recvTimeoutTimer;

        /// <summary> 通信ログファイル書込みクラス </summary>
        //private FileLog fileLog;

        /// <summary> 通信ログファイル書込みONOFFフラグ </summary>
        private bool enableWriteLog;


        /// <summary> 通信ログファイル出力ONOFFフラグ </summary>
        public bool EnableWriteLog
        {
            get { return this.enableWriteLog; }

            set
            {
                this.enableWriteLog = value;
                //if (this.fileLog != null) this.fileLog.Enabled = value;
            }
        }


        /// <summary> 通信ログのファイルパス </summary>
        public string FileLogPath { get; set; }

        /// <summary> シミュレータの状態を文字列で返す </summary>
        public string PortStatus { get { return $"[{((this.IsOpen) ? "動作中" : "停止中")}] Main(受信)[{this.RecvIpAddr}]→UI(送信先)[{this.SendIpAddr}]"; } }

        /// <summary> エラーメッセージ </summary>
        public string ErrorMessage { get; private set; }

        #endregion


        #region Constructor
        /// <summary>コンストラクタ</summary>
        public TcpIF()
        {
            this.ReceiveTimeout = 2000;
            this.IsOpen = false;

            //this.fileLog = new FileLog();
            this.EnableWriteLog = false;

            this.SendIpAddr = "192.168.254.25";
            this.SendPortNo = 52001;

            this.RecvIpAddr = "192.168.254.25";
            this.RecvPortNo = 52003;

            this.RxRawData = null;
        }
        #endregion


        #region Method

        #region async_tcp

        /// <summary>
        /// Start TCP listener to received reuest from clients
        /// async handling to avoid GUI response blocking 
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <param name="port"></param>
        public async void RcvManager(string ipAddr, int port)
        {

            Console.WriteLine("RcvManager fucntion is started ..." + ipAddr + ":" + port);

            await StartServer(ipAddr, port);

        }

        /// <summary>
        /// start TCP Server for listening clients's request -> start exchange data
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<int> StartServer(string ip, int port)
        {

            int ret = 0;

            string ipAddr = ip;

            int PORT_READER = port;

            IPAddress ipAddress = IPAddress.Parse(ipAddr);

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT_READER);

            try
            {
                // Create a Socket that will use Tcp protocol
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Read socket created");

                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);

                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(10);


                // assign socket for close button handling purpose
                this.serverSocket = listener;

                // listening request from clients
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = await listener.AcceptAsync();

                    Console.WriteLine("Socket is established");


                    // Incoming data from the client.
                    string data = null;
                    byte[] bytes = null; ;

                    Console.WriteLine("Reading from socket {0}", handler.RemoteEndPoint.ToString());
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    Console.WriteLine("Read {0} bytes from {1}", bytesRec, handler.RemoteEndPoint.ToString());

                    byte[] buf = new byte[bytesRec];

                    Array.Copy(bytes, buf, bytesRec);

                    this.RxRawData = buf;

                    data += convertByteString(buf);

                    setReceive();

                    Console.WriteLine("client address --------> :  " + handler.RemoteEndPoint.ToString());
                    Console.WriteLine("Text received            :  " + data);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }


        /// <summary>
        /// sending data to clients handling
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool Send(byte[] data, int length)
        {

            Console.WriteLine("-------------------->>> size : " + length.ToString());
            Console.WriteLine("-------------------->>> data.len : " + data.Length.ToString());

            _ = SendAsync(data, length);

            return true;
        }


        /// <summary>
        /// データを非同期送信対応
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(byte[] data, int length)
        {
            if (!this.IsOpen) return false;

            // ツール画面にTX内容として反映する
            writeLog(MsgType.Tx, data, length);

            // start timer 
            StartReceiveTimeout();

            try
            {
                IPAddress ipAddress = IPAddress.Parse(this.SendIpAddr);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, this.SendPortNo);

                Console.WriteLine("CLIENT Address IPaddr : " + this.SendIpAddr + " port : " + this.SendPortNo);
                Console.WriteLine("CLIENT Address : " + remoteEP);

                // Create a TCP/IP  socket.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to Remote EndPoint 
                    await socket.ConnectAsync(remoteEP);

                    Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());

                    // Send the data over TCP async.
                    ArraySegment<byte> myArrSegAll = new ArraySegment<byte>(data);
                    int bytesSent = await socket.SendAsync(myArrSegAll, SocketFlags.None);
                    Console.WriteLine("Reading from socket {0}", socket.RemoteEndPoint.ToString());
                    byte[] rxBuffer = new byte[1024];
                    int rxSize = socket.Receive(rxBuffer);

                    Console.WriteLine("Read {0} bytes from {1}", rxSize, socket.RemoteEndPoint.ToString());

                    byte[] buf = new byte[rxSize];

                    Array.Copy(rxBuffer, buf, rxSize);

                    this.RxRawData = buf;

                    setReceive();

                    // Release the socket.
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return true;
        }
        #endregion


        #region CommController

        /// <summary>TCP受信処理を開始</summary>
        public void Open()
        {
            this.IsOpen = true;

            // start listening clients asynchronously
            //RcvManager(this.RecvIpAddr, this.RecvPortNo);

            //受信タイムアウトタイマー初期化
            this.recvTimeoutTimer = new System.Timers.Timer(this.ReceiveTimeout);
            this.recvTimeoutTimer.Stop();
            this.recvTimeoutTimer.Elapsed += new System.Timers.ElapsedEventHandler(elapsedTimeout);

        }


        /// <summary>TCP通信を終了する。送信側、受信側両方ともCloseする</summary>
        public void Close()
        {
            this.IsOpen = false;

            if (this.recvTimeoutTimer != null)
            {
                this.recvTimeoutTimer.Dispose();
                this.recvTimeoutTimer = null;
            }

            //this.fileLog.Close();

            if (this.serverSocket != null)
            {
                this.serverSocket.Close();
            }

        }

        #endregion

        #region timerHandler


        /// <summary>受信タイムアウト処理を開始する</summary>
        public void StartReceiveTimeout()
        {
            if (this.recvTimeoutTimer == null) return;

            this.recvTimeoutTimer.Stop();
            this.recvTimeoutTimer.Start();

            Console.WriteLine("start timer ===========>>>");
        }


        /// <summary>受信タイムアウト処理を停止する</summary>
        public void StopReceiveTimeout()
        {
            if (this.recvTimeoutTimer == null) return;

            this.recvTimeoutTimer.Stop();
        }


        /// <summary>
        /// 受信タイムアウト発生イベント処理(タイマー起動)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void elapsedTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            setTimeout();

            StopReceiveTimeout();
        }

        /// <summary>
        /// log timeout event once timeout occurred 
        /// </summary>
        private void setTimeout()
        {
            Console.WriteLine("受信タイムアウト ===========>>>");

            this.ErrorMessage = "受信タイムアウト";

            writeLog(MsgType.RxMsg, this.ErrorMessage);

            this.TcpIFTimeoutReceived?.Invoke(this, new EventArgs());
        }

        #endregion


        #region receivedDataHandler

        /// <summary>エラーイベントを起こす</summary>
        protected void SetError(MsgType type, string errorMessage)
        {
            StopReceiveTimeout();

            this.ErrorMessage = errorMessage;

            writeLog(type, errorMessage);

            this.TcpIFErrorReceived?.Invoke(this, new EventArgs());
        }


        /// <summary>受信イベントを起こす</summary>
        private void setReceive()
        {
            writeLog(MsgType.Rx, this.RxRawData);

            Console.WriteLine("setReceive : received data ==================================>>. " + convertByteString(this.RxRawData));

            this.TcpIFDataReceived?.Invoke(this, new EventArgs());
        }


        #region WriteLog
        /// <summary>
        /// 通信ログにbyte配列データを16進表記で保存する
        /// </summary>
        /// <param name="type">MsgType</param>
        /// <param name="data">byte配列</param>
        /// <param name="length">通信ログ長(byte)指定無しはbyte配列長</param>
        private void writeLog(MsgType type, byte[] data, int length = -1)
        {
            if (length < 0) length = data.Length;
            string log = Util.ToString(data, 0, length);
            writeLog(type, log);
        }

        /// <summary>
        /// 通信ログに文字列を保存する
        /// </summary>
        /// <param name="type"></param>
        /// <param name="log"></param>
        private void writeLog(MsgType type, string log)
        {
            string writelog = $"{DateTime.Now:HH:mm:ss.fff} {type} {log}";

            //if (this.enableWriteLog) this.fileLog.Write(this.FileLogPath, writelog); //ファイルに通信ログを書き込む

            this.TcpIFWriteLog?.Invoke(this, type, writelog);
        }

        #endregion
        #endregion



        private string convertByteString(byte[] data)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte e in data)
            {
                s.Append(e);
            }
            return s.ToString();
        }

        #endregion
    }
}
