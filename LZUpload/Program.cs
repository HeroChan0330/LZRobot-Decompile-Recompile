using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LM_Common;
using LMST_Show;
using System.Threading;
using System.IO;


/*-------------------------------------------------
 *  Program:LZUpload
 *  Date:2018-2-24
 *  Author:HeroChan Sysu
 *  Function:Upload a python code to the raspbian in kenfish
 *  Usage:Add the code`s path to the argument
 *  Notice:Decompile from LeZhi`s KenFish,only for study
 ------------------------------------------------*/


namespace LZUpload
{
    class Program
    {
        static LMST_Show.LMST_AppLayer lmst_App;
        const int TIMEOUTSEC = 10;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input file,press any key to exit");
                //Console.Read();
                return;
            }

            lmst_App = new LMST_AppLayer();
            Console.WriteLine("Linking……");
            DateTime startConnectionTime = DateTime.Now;
            while (!lmst_App.IsConnection)//等待网络连接 连接超时为TIMEOUTSEC
            {
                Thread.Sleep(333);
                if ((DateTime.Now - startConnectionTime).TotalSeconds > TIMEOUTSEC)
                {
                    Console.WriteLine("Fail to link");
                    //Console.Read();
                    Environment.Exit(0);
                }
            }
            Console.WriteLine("Link succeed!");


            /*
            while (true)
            {
                lmst_App.OpenHeadLight();
                Thread.Sleep(2000);
                lmst_App.CloseHeadLight();
                Thread.Sleep(500);
            }*/
            int node = 5;
            //节点0~8对应的是
            //"头舱", "尾舱", "螺旋桨推进舱", "传感器舱", "浮力舱", "python", "红外传感器舱", "扩展舱", "头舱主控"
            lmst_App.StartUploadFireware(args[0], node);
            while (lmst_App.UploadFirmwareState == 0) Thread.Sleep(333);//等待上传
            switch (lmst_App.UploadFirmwareState)//打印结果
            {
                case 1:
                    Console.WriteLine("Upload succeed!,press any key to exit");
                    break;
                case -1:
                    Console.WriteLine("Fail to upload!,press any key to exit");

                    break;
            }
            //Console.Read();
            Environment.Exit(0);
        }
    }
}
