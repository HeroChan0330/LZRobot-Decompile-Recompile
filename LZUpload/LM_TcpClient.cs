namespace LMST_Show
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public class LM_TcpClient
    {
        private Thread checkStateThread;
        private TcpClient client;
        private string hostip;
        private volatile bool isConnection = false;
        private int port;

        public LM_TcpClient(string hostip, int port)
        {
            this.hostip = hostip;
            this.port = port;
            this.client = new TcpClient();
            this.IsConnection = false;
            this.checkStateThread = new Thread(new ThreadStart(this.checkState));
            this.checkStateThread.IsBackground = true;
            this.checkStateThread.Start();
        }

        private void checkState()
        {
            while (true)
            {
                Thread.Sleep(0x3e8);
                if (!this.IsOnline(this.client))
                {
                    try
                    {
                        this.client.Close();
                        this.client = new TcpClient();
                        this.client.Connect(this.hostip, this.port);
                        this.isConnection = true;
                    }
                    catch
                    {
                        this.isConnection = false;
                    }
                }
                else
                {
                    this.isConnection = true;
                }
            }
        }

        public void Close()
        {
            this.client.Close();
        }

        public bool IsOnline(TcpClient c){
            return ((!c.Client.Poll(0x3e8, SelectMode.SelectRead) || (c.Client.Available != 0)) && c.Client.Connected);
        }
        public int RecvBytes(byte[] d, int len)
        {
            try
            {
                return this.client.GetStream().Read(d, 0, len);
            }
            catch
            {
                this.IsConnection = false;
                return 0;
            }
        }

        public int RecvOneByte(ref byte d)
        {
            byte[] buffer = new byte[1];
            try
            {
                int num = this.client.GetStream().Read(buffer, 0, 1);
                d = buffer[0];
                return num;
            }
            catch
            {
                this.IsConnection = false;
                return 0;
            }
        }

        public void SendBytes(byte[] d)
        {
            try
            {
                this.client.GetStream().Write(d, 0, d.Length);
            }
            catch
            {
                this.IsConnection = false;
            }
        }

        public void SendStr(string strMessage)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(strMessage + "\n");
                this.client.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch
            {
                this.IsConnection = false;
            }
        }

        public bool IsConnection
        {
            get
            {
                return this.isConnection;

            }
            set
            {
                this.isConnection = value;
            }
        }
    }
}

