using System;
using System.Collections.Generic;

namespace WorkerObj
{
    public class Worker
    {
        public string IPAddress { get; set; }
        public string Ports { get; set; }
        public string Status { get; set; }
        public List<string> ExeImages { get; set; } = new List<string>();

        public string GetFirstPort()
        {
            return Ports.Substring(0, Ports.IndexOf(":"));
        }

        public string GetSecondPort()
        {
            return Ports.Substring(Ports.IndexOf(":") + 1);
        }
    }
}
