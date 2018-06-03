namespace LMST_Show
{
    using AForge.Controls;
    using AForge.Video;
    using LM_Common;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    internal class LMST_AppLayer
    {
        private int addr_pl = 0;
        private byte[] Buff_pl;
        private byte[] CtrlMeasure = new byte[] { 0, 7, 7, 7, 7, 7, 7, 7 };
        private int DataLen_pl = 0;
        public YJ_LmstMsg_sum DivingNodeParam = new YJ_LmstMsg_sum();
        private YJ_Timer DivingNodeTimeOut = new YJ_Timer(0x3e8);
        private FileStream file_pl;
        private int FileLen_pl = 0;
        private int flags_pl = 0;
        public YJ_LmstMsg_sum HeadNodeParam = new YJ_LmstMsg_sum();
        private YJ_Timer HeadNodeTimeOut = new YJ_Timer(0x3e8);
        public bool IsConnection = false;
        public bool IsDivingNodeOn = false;
        public bool IsHeadNodeOn = false;
        public bool IsRebootAllNode = false;
        public bool IsSensorNodeOn = false;
        public bool IsServoMotorNodeOn = false;
        public bool IsTailNodeOn = false;
        //private MJPEGStream JpegSource;
        private BinaryReader reader_pl;
        private int ReadOffset_pl;
        public YJ_LmstMsg_sum SensorNodeParam = new YJ_LmstMsg_sum();
        private YJ_Timer SensorNodeTimeOut = new YJ_Timer(0x3e8);
        public YJ_LmstMsg_sum ServoMotorNodeParam = new YJ_LmstMsg_sum();
        private YJ_Timer ServoMotorNodeTimeOut = new YJ_Timer(0x3e8);
        private int ses_pl = 0;
        private int state_pl = 0;
        public YJ_LmstMsg_sum TailNodeParam = new YJ_LmstMsg_sum();
        private YJ_Timer TailNodeTimeOut = new YJ_Timer(0x3e8);
        private YJ_Timer TimeOut_pl = new YJ_Timer(0x3e8);
        private YJ_LmstTransport Transport = new YJ_LmstTransport("192.168.42.1", 0x22c3);
        public int UploadFirmwareSchedule;
        public int UploadFirmwareState;//硬件的状态 1是上传成功 0是空闲 -1是上传失败

        public LMST_AppLayer()
        {
            this.Buff_pl = new byte[this.Transport.MaxLtvPayload];
            new Thread(new ThreadStart(this.StateMachineThrFunc)).Start();
            new Thread(new ThreadStart(this.HeaetBeatSendThrFunc)).Start();
        }

        private int AppLayerRecv(ref int addr, ref int type, ref string msg, ref byte[] msg_hex)
        {
            byte[] buf = new byte[this.Transport.MaxLtvPayload];
            int num = this.Transport.RecvLtv(buf, this.Transport.MaxLtvPayload);
            if (num < 8)
            {
                return 0;
            }
            type = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, 0));
            addr = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, 4));
            msg = Encoding.Default.GetString(buf, 8, num - 8);
            return (num - 8);
        }

        private void AppLayerSend(int addr, string msg)
        {
            this.AppLayerSend(addr, 3, msg);
        }

        private void AppLayerSend(int addr, int type, string msg)
        {
            byte[] destinationArray = new byte[this.Transport.MaxLtvPayload];
            type = IPAddress.HostToNetworkOrder(type);
            destinationArray[0] = (byte) type;
            destinationArray[1] = (byte) (type >> 8);
            destinationArray[2] = (byte) (type >> 0x10);
            destinationArray[3] = (byte) (type >> 0x18);
            addr = IPAddress.HostToNetworkOrder(addr);
            destinationArray[4] = (byte) addr;
            destinationArray[5] = (byte) (addr >> 8);
            destinationArray[6] = (byte) (addr >> 0x10);
            destinationArray[7] = (byte) (addr >> 0x18);
            byte[] bytes = Encoding.Default.GetBytes(msg);
            Array.Copy(bytes, 0, destinationArray, 8, bytes.Length);
            destinationArray[bytes.Length + 8] = 0;
            this.Transport.SendLtv(destinationArray, (bytes.Length + 8) + 1);
        }

        private void CheckeNodeTimeOut()
        {
            if (this.TailNodeTimeOut.IsTimeOut())
            {
                this.IsTailNodeOn = false;
            }
            else
            {
                this.IsTailNodeOn = true;
            }
            if (this.HeadNodeTimeOut.IsTimeOut())
            {
                this.IsHeadNodeOn = false;
            }
            else
            {
                this.IsHeadNodeOn = true;
            }
            if (this.ServoMotorNodeTimeOut.IsTimeOut())
            {
                this.IsServoMotorNodeOn = false;
            }
            else
            {
                this.IsServoMotorNodeOn = true;
            }
            if (this.SensorNodeTimeOut.IsTimeOut())
            {
                this.IsSensorNodeOn = false;
            }
            else
            {
                this.IsSensorNodeOn = true;
            }
            if (this.DivingNodeTimeOut.IsTimeOut())
            {
                this.IsDivingNodeOn = false;
            }
            else
            {
                this.IsDivingNodeOn = true;
            }
        }

        public void CloseHeadLight()
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "CloseLight"
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0xe0, str);
        }

        private void HeaetBeatSendThrFunc()
        {
            YJ_LmstMsg msg = new YJ_LmstMsg();
            while (true)
            {
                if (!this.IsConnection || (this.state_pl > 0))
                {
                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
        }

        public void OpenHeadLight()
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "OpenLight"
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0xe0, str);
        }

        private void ProNodeData(string msg, int addr)
        {
            this.refreshNode(addr);
            int num = addr;
            if (num <= 0x20)
            {
                switch (num)
                {
                    case 0x10:
                        try
                        {
                            this.ServoMotorNodeParam = (YJ_LmstMsg_sum) JsonConvert.DeserializeObject(msg, typeof(YJ_LmstMsg_sum));
                        }
                        catch
                        {
                            YJ_Log.DEBUG("ServoMotorNodeParam recv error", 0);
                        }
                        break;

                    case 0x20:
                        try
                        {
                            this.SensorNodeParam = (YJ_LmstMsg_sum) JsonConvert.DeserializeObject(msg, typeof(YJ_LmstMsg_sum));
                        }
                        catch
                        {
                            YJ_Log.DEBUG("SensorNodeParam recv error", 0);
                        }
                        break;
                }
            }
            else
            {
                switch (num)
                {
                    case 0x30:
                        try
                        {
                            this.DivingNodeParam = (YJ_LmstMsg_sum) JsonConvert.DeserializeObject(msg, typeof(YJ_LmstMsg_sum));
                        }
                        catch
                        {
                            YJ_Log.DEBUG("DivingNodeParam recv error", 0);
                        }
                        return;

                    case 0xe0:
                        try
                        {
                            this.HeadNodeParam = (YJ_LmstMsg_sum) JsonConvert.DeserializeObject(msg, typeof(YJ_LmstMsg_sum));
                        }
                        catch
                        {
                            YJ_Log.DEBUG("HeadNodeParam recv error", 0);
                        }
                        return;
                }
                if (num == 240)
                {
                    try
                    {
                        this.TailNodeParam = (YJ_LmstMsg_sum) JsonConvert.DeserializeObject(msg, typeof(YJ_LmstMsg_sum));
                    }
                    catch
                    {
                        YJ_Log.DEBUG("TailNodeParam recv error", 0);
                    }
                }
            }
        }

        private void ProUpload(string msg, int addr)
        {
            int num = 0;
            YJ_LmstMsg_sum _sum = new YJ_LmstMsg_sum();
            if ((this.state_pl != 0) && (this.addr_pl == addr))
            {
                try
                {
                    _sum = (YJ_LmstMsg_sum) JsonConvert.DeserializeObject(msg, typeof(YJ_LmstMsg_sum));
                }
                catch
                {
                    YJ_Log.DEBUG("ProUpload recv error", 0);
                }
                if (string.Compare(_sum.c, "DataReq") == 0)
                {
                    YJ_Log.DEBUG(msg, 0);
                    if (this.ses_pl == _sum.n)
                    {
                        this.SendUploadData(addr, this.ses_pl, this.Buff_pl, this.DataLen_pl);
                    }
                    else if ((this.ses_pl + 1) == _sum.n)
                    {
                        try
                        {
                            num = this.reader_pl.Read(this.Buff_pl, 0, 0x80);
                        }
                        catch
                        {
                            YJ_Log.DEBUG("reader_pl recv error", 0);
                        }
                        if (num > 0)
                        {
                            this.DataLen_pl = num;
                            this.ReadOffset_pl += num;
                            this.ses_pl++;
                            this.flags_pl++;
                            this.UploadFirmwareSchedule = (this.ReadOffset_pl * 100) / this.FileLen_pl;
                            this.SendUploadData(addr, this.ses_pl, this.Buff_pl, this.DataLen_pl);
                        }
                    }
                    else
                    {
                        YJ_Log.DEBUG("(ses_pl + 1) != m.n", 0);
                    }
                }
                else if (string.Compare(_sum.c, "UpdateFinish") == 0)
                {
                    this.file_pl.Close();
                    this.reader_pl.Close();
                    this.state_pl = 0;
                    this.UploadFirmwareState = 1;
                    YJ_Log.DEBUG("UpdateFinish", 0);
                }
            }
        }

        public void RebootAllNode()
        {
            this.IsRebootAllNode = true;
        }

        private void refreshNode(int addr)
        {
            switch (addr)
            {
                case 0x10:
                    this.ServoMotorNodeTimeOut.SetTime(0x1388);
                    break;

                case 0x20:
                    this.SensorNodeTimeOut.SetTime(0x1388);
                    break;

                case 0x30:
                    this.DivingNodeTimeOut.SetTime(0x1388);
                    break;

                case 0xe0:
                    this.HeadNodeTimeOut.SetTime(0x1388);
                    break;

                case 240:
                    this.TailNodeTimeOut.SetTime(0x1388);
                    break;
            }
        }

        public void SaveSmFinOffset()
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "SaveSmFinOffset"
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
        }

        public void SaveTraiFinOffset()
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "SaveTraiFinOffset"
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(240, str);
            Thread.Sleep(40);
        }

        private void SendUploadData(int addr, int ses, byte[] data, int len)
        {
            byte[] destinationArray = new byte[this.Transport.MaxLtvPayload];
            int host = 8;
            host = IPAddress.HostToNetworkOrder(host);
            destinationArray[0] = (byte) host;
            destinationArray[1] = (byte) (host >> 8);
            destinationArray[2] = (byte) (host >> 0x10);
            destinationArray[3] = (byte) (host >> 0x18);
            addr = IPAddress.HostToNetworkOrder(addr);
            destinationArray[4] = (byte) addr;
            destinationArray[5] = (byte) (addr >> 8);
            destinationArray[6] = (byte) (addr >> 0x10);
            destinationArray[7] = (byte) (addr >> 0x18);
            ses = IPAddress.HostToNetworkOrder(ses);
            destinationArray[8] = (byte) ses;
            destinationArray[9] = (byte) (ses >> 8);
            destinationArray[10] = (byte) (ses >> 0x10);
            destinationArray[11] = (byte) (ses >> 0x18);
            Array.Copy(data, 0, destinationArray, 12, len);
            this.Transport.SendLtv(destinationArray, len + 12);
        }

        public void SetLSmFinOffset(short Offset)
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "SetSmFinOffset_l",
                value = Offset
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
        }

        public void SetRSmFinOffset(short Offset)
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "SetSmFinOffset_r",
                value = Offset
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
        }

        public void SetTraiFinOffset(short Offset)
        {
            YJ_LmstMsg msg = new YJ_LmstMsg {
                c = "SetTraiFinOffset",
                value = Offset
            };
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(240, str);
            Thread.Sleep(40);
        }

        public void StartToSendMsg(byte[] cm, int DivingNodeState)
        {
            YJ_LmstMsg msg = new YJ_LmstMsg();
            this.CtrlMeasure = cm;
            msg.c = "SetSpeed";
            msg.value = this.CtrlMeasure[0];
            string str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(240, str);
            Thread.Sleep(40);
            msg.c = "SetDirection";
            msg.value = this.CtrlMeasure[1];
            str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(240, str);
            Thread.Sleep(40);
            msg.c = "SetSteerDirection_l";
            msg.value = this.CtrlMeasure[2];
            str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
            msg.c = "SetMotorSpeed_l";
            msg.value = this.CtrlMeasure[3];
            str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
            msg.c = "SetSteerDirection_r";
            msg.value = this.CtrlMeasure[4];
            str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
            msg.c = "SetMotorSpeed_r";
            msg.value = this.CtrlMeasure[5];
            str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x10, str);
            Thread.Sleep(40);
            msg.c = "SetDivingState";
            msg.value = DivingNodeState;
            str = JsonConvert.SerializeObject(msg);
            this.AppLayerSend(0x30, str);
        }

        public int StartUploadFireware(string path, int NodeIdex)
        {
            int num = 0;
            if (!System.IO.File.Exists(path))
            {
                return -1;
            }
            switch (NodeIdex)
            {
                case 0:
                    num = 0xe0;
                    break;

                case 1:
                    num = 240;
                    break;

                case 2:
                    num = 0x10;
                    break;

                case 3:
                    num = 0x20;
                    break;

                case 4:
                    num = 0x30;
                    break;

                case 5:
                    num = 0x40;
                    break;
            }
            this.file_pl = new FileStream(path, FileMode.Open, FileAccess.Read);
            this.reader_pl = new BinaryReader(this.file_pl);
            this.FileLen_pl = (int) this.file_pl.Length;
            this.addr_pl = num;
            this.state_pl = 1;
            this.ses_pl = 0;
            this.ReadOffset_pl = 0;
            this.UploadFirmwareState = 0;
            return 1;
        }

        public int StartUploadFireware_str(string s, int type)
        {
            int num = 0;
            num = 0x40;
            byte[] bytes = Encoding.Default.GetBytes(s);
            if (bytes.Length == 0)
            {
                return 0;
            }
            this.file_pl = new FileStream("json.txt", FileMode.Create, FileAccess.Write);
            this.file_pl.Write(bytes, 0, bytes.Length);
            this.file_pl.Close();
            this.file_pl = new FileStream("json.txt", FileMode.Open, FileAccess.Read);
            this.reader_pl = new BinaryReader(this.file_pl);
            this.FileLen_pl = (int) this.file_pl.Length;
            this.addr_pl = num;
            this.state_pl = 1;
            this.ses_pl = 0;
            this.ReadOffset_pl = 0;
            this.UploadFirmwareState = 0;
            return 1;
        }

        private void StateMachineThrFunc()
        {
            int num = 0;
            string str = "msg";
            byte[] buffer = new byte[this.Transport.MaxLtvPayload];
            int addr = 0;
            int type = 0;
            while (true)
            {
                Thread.Sleep(1);
                if (this.IsRebootAllNode)//函数RebootAllNode()的作用
                {
                    YJ_LmstMsg msg = new YJ_LmstMsg();
                    this.IsRebootAllNode = false;
                    msg.c = "Reboot";
                    string str2 = JsonConvert.SerializeObject(msg);
                    this.AppLayerSend(240, str2);
                    this.AppLayerSend(0x10, str2);
                    this.AppLayerSend(0x20, str2);
                    this.AppLayerSend(0x30, str2);
                }
                this.CheckeNodeTimeOut();//检测超时？
                this.UploadStateMachine();//上传状态机
                switch (num)
                {
                    case 0:
                        if (this.Transport.IsConnection)
                        {
                            num = 1;
                            this.IsConnection = true;
                        }
                        break;

                    case 1:
                        if (!this.Transport.IsConnection)
                        {
                            num = 0;
                            this.IsConnection = false;
                        }
                        else if (this.AppLayerRecv(ref addr, ref type, ref str, ref buffer) > 0)
                        {
                            if (type == 5)
                            {
                                YJ_Log.ERR(str, addr);
                            }
                            else if (type == 4)
                            {
                                YJ_Log.DEBUG(str, addr);
                            }
                            else if ((type == 7) || (type == 9))
                            {
                                this.ProUpload(str, addr);
                            }
                            else
                            {
                                this.ProNodeData(str, addr);
                                YJ_Log.DATA(str, addr);
                            }
                        }
                        break;
                }
            }
        }

        private void UploadStateMachine()//上传状态机？
        {
            string str;
            YJ_LmstMsg msg = new YJ_LmstMsg();
            switch (this.state_pl)
            {
                case 1:
                    msg.c = "Reboot";
                    str = JsonConvert.SerializeObject(msg);
                    this.AppLayerSend(this.addr_pl, str);
                    this.TimeOut_pl.SetTime(500);
                    this.state_pl = 2;
                    this.UploadFirmwareState = 0;
                    break;

                case 2:
                    if (this.TimeOut_pl.IsTimeOut())
                    {
                        msg.c = "UplaodFirmware";
                        msg.value = this.FileLen_pl;
                        str = JsonConvert.SerializeObject(msg);
                        this.AppLayerSend(this.addr_pl, 6, str);
                        this.TimeOut_pl.SetTime(0xfa0);
                        this.state_pl = 3;
                    }
                    break;

                case 3:
                    if (!this.TimeOut_pl.IsTimeOut())
                    {
                        if (this.flags_pl > 0)
                        {
                            this.flags_pl = 0;
                            this.TimeOut_pl.SetTime(0xfa0);
                        }
                        break;
                    }
                    YJ_Log.DEBUG("Update fail!", 0);
                    this.file_pl.Close();
                    this.reader_pl.Close();
                    this.UploadFirmwareState = -1;
                    this.state_pl = 0;
                    break;
            }
        }
    }
}

