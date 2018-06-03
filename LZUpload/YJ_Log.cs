namespace LM_Common
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public class YJ_Log
    {
        private static Hashtable ht;
        private static bool IsOpened = false;

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        public static void close()
        {
            if (IsOpened)
            {
                FreeConsole();
            }
            IsOpened = false;
        }

        public static void DATA(string s, int from)
        {
            if (IsOpened)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                DateTime now = new DateTime();
                now = DateTime.Now;
                object[] arg = new object[] { now.Year % 100, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond };
                Console.Write("[{0}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}]", arg);
                Console.Write("[{0}]", ht[from]);
                Console.WriteLine(s);
            }
        }

        public static void DEBUG(string s)
        {
            if (IsOpened)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                DateTime now = new DateTime();
                now = DateTime.Now;
                object[] arg = new object[] { now.Year % 100, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond };
                Console.Write("[{0}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}] ", arg);
                Console.WriteLine(s);
            }
        }

        public static void DEBUG(string s, int from)
        {
            if (IsOpened)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                DateTime now = new DateTime();
                now = DateTime.Now;
                object[] arg = new object[] { now.Year % 100, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond };
                Console.Write("[{0}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}]", arg);
                Console.Write("[{0}]", ht[from]);
                Console.WriteLine(s);
            }
        }

        public static void ERR(string s)
        {
            if (IsOpened)
            {
                DateTime now = new DateTime();
                now = DateTime.Now;
                object[] arg = new object[] { now.Year % 100, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond };
                Console.Write("[{0}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}]", arg);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(s);
            }
        }

        public static void ERR(string s, int from)
        {
            if (IsOpened)
            {
                DateTime now = new DateTime();
                now = DateTime.Now;
                Console.ForegroundColor = ConsoleColor.Red;
                object[] arg = new object[] { now.Year % 100, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond };
                Console.Write("[{0}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}]", arg);
                Console.Write("[{0}]", ht[from]);
                Console.WriteLine(s);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        public static void open()
        {
            if (!IsOpened)
            {
                AllocConsole();
                ht = new Hashtable();
                ht.Add(0xe0, "Head");
                ht.Add(0xe1, "Head");
                ht.Add(240, "Tail");
                ht.Add(0x10, "ServoMotor");
                ht.Add(0x20, "Sensor");
                ht.Add(0x30, "Diving");
                ht.Add(0, "PC");
                IsOpened = true;
            }
        }
    }
}

