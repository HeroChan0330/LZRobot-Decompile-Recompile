namespace LM_Common
{
    using System;

    public class YJ_Timer
    {
        private int h;

        public YJ_Timer()
        {
            this.Init();
        }

        public YJ_Timer(int ms)
        {
            this.Init();
            this.h += ms;
        }

        private void Init()
        {
            this.h = Environment.TickCount;
        }

        public bool IsTimeOut()
        {
            return (this.h < Environment.TickCount);
        }
        public void SetTime(int ms)
        {
            this.h = Environment.TickCount + ms;
        }
    }
}

