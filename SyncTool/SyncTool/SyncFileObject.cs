using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;using System.Threading;


namespace SyncTool
{
    class SyncFileObject
    {
        Thread t;
        public int Interval = 500;
        public string f1 = "";
        public string f2 = "";
        private bool stopped = false;

        public SyncFileObject()
        {

        }

        private void sync()
        {
            if (!File.Exists(f1) && !File.Exists(f2)) this.Stop();
            while (!stopped)
            {
                if (!File.Exists(f1))
                    File.Copy(f2, f1);
                else if (!File.Exists(f2))
                    File.Copy(f1, f2);
                else
                {
                    Int64 f1ts = GetTimeStamp(File.GetLastWriteTime(f1));
                    Int64 f2ts = GetTimeStamp(File.GetLastWriteTime(f2));
                    if (f1ts > f2ts)
                        File.Copy(f1, f2, true);
                    else if (f1ts < f2ts)
                        File.Copy(f2, f1, true);
                }
                System.Threading.Thread.Sleep(this.Interval);
            }
        }

        public void Start()
        {
            t = new Thread(new ThreadStart(sync));
            stopped = false;
            t.Start();
        }

        public void Stop()
        {
            stopped = true;
        }

        public static string GetExactTimeStamp(DateTime time)
        {
            return time.ToString("yyyyMMddHHmmssfff");
        }

        public static Int64 GetTimeStamp(DateTime time)
        {
            // HH zeigt 24-Stunden-Format, hh zeigt 12-Stunden-Format.         
            return Convert.ToInt64(time.ToString("yyyyMMddHHmmsss"));
        }
    }
}