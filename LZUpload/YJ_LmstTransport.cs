namespace LMST_Show
{
    using LM_Common;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    internal class YJ_LmstTransport : LM_TcpClient
    {
        public const int LM_MAX_UUDP_PAYLOAD = 270;
        public int MaxLtvPayload;
        private object RecvLock;
        private Queue<RecvInfo> RecvQue;

        public YJ_LmstTransport(string hostip, int port) : base(hostip, port)
        {
            this.RecvQue = new Queue<RecvInfo>();
            this.RecvLock = new object();
            this.MaxLtvPayload = 270;
            new Thread(new ThreadStart(this.RecvProThrFunc)).Start();
        }

        private int OwspProtoCheck(byte[] Buf, int len)
        {
            if (len >= 4)
            {
                uint num = (uint) IPAddress.NetworkToHostOrder(BitConverter.ToInt32(Buf, 0));
                if (num > 270)
                {
                    return -1;
                }
                if (num == (len - 4))
                {
                    return 1;
                }
            }
            return 0;
        }

        public int RecvLtv(byte[] buf, int BufLen)
        {
            RecvInfo info;
            if (this.RecvQue.Count < 1)
            {
                return 0;
            }
            object recvLock = this.RecvLock;
            lock (recvLock)
            {
                info = this.RecvQue.Dequeue();
            }
            if (info.len > BufLen)
            {
                return 0;
            }
            Array.Copy(info.data, buf, info.len);
            return info.len;
        }

        private void RecvProThrFunc()
        {
            byte[] buf = new byte[0x400];
            byte d = 0;
            int len = 0;
            YJ_Timer timer = new YJ_Timer(0x3e8);
            RecvInfo item = new RecvInfo();
            while (true)
            {
                if (!base.IsConnection)
                {
                    Thread.Sleep(5);
                }
                else
                {
                    if (len == 0)
                    {
                        Thread.Sleep(5);
                    }
                    if (len >= 270)
                    {
                        len = 0;
                    }
                    if ((len > 0) && timer.IsTimeOut())
                    {
                        len = 0;
                    }
                    if (base.RecvOneByte(ref d) == 0)
                    {
                        Thread.Sleep(2);
                    }
                    else
                    {
                        buf[len++] = d;
                        timer.SetTime(0x3e8);
                        int num2 = this.OwspProtoCheck(buf, len);
                        if (num2 == 1)
                        {
                            item = new RecvInfo {
                                len = len - 8
                            };
                            Array.ConstrainedCopy(buf, 8, item.data, 0, item.len);
                            this.RecvQue.Enqueue(item);
                            len = 0;
                        }
                        else if (num2 != 0)
                        {
                            len = 0;
                        }
                    }
                }
            }
        }

        public void SendCmd(byte[] data, int len)
        {
            byte[] d = new byte[len + 4];
            byte num = 0;
            byte num2 = 1;
            d[0] = 0x21;
            d[1] = (byte) ((num2 << 6) | len);
            for (int i = 0; i < len; i++)
            {
                d[2 + i] = data[i];
                num = (byte) (num + data[i]);
            }
            d[2 + len] = (byte) (~num & 0xff);
            d[3 + len] = 0x52;
            base.SendBytes(d);
        }

        public void SendLtv(byte[] data, int len)
        {
            byte[] d = new byte[len + 8];
            int host = 0;
            int num3 = IPAddress.HostToNetworkOrder((int) (len + 4));
            d[0] = (byte) num3;
            d[1] = (byte) (num3 >> 8);
            d[2] = (byte) (num3 >> 0x10);
            d[3] = (byte) (num3 >> 0x18);
            host = IPAddress.HostToNetworkOrder(host);
            d[4] = (byte) host;
            d[5] = (byte) (host >> 8);
            d[6] = (byte) (host >> 0x10);
            d[7] = (byte) (host >> 0x18);
            for (int i = 0; i < len; i++)
            {
                d[8 + i] = data[i];
            }
            base.SendBytes(d);
        }

        private class RecvInfo
        {
            public byte[] data = new byte[270];
            public int len;
        }
    }
}

