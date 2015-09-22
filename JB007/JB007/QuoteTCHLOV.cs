using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JB007
{
    class QuoteTCHLOV
    {
        private DateTime timeStamp;
        private float close;
        private float high;
        private float low;
        private float open;
        private long volume;
        private float previousClose;
        public DateTime TimeStamp { get { return timeStamp; } set { timeStamp = value; } }
        public float Close { get { return close; } set { close = value; } }
        public float High { get { return high; } set { high = value; } }
        public float Low { get { return low; } set { low = value; } }
        public float Open { get { return open; } set { open = value; } }
        public long Volume { get { return volume; } set { volume = value; } }
        public float PreviousClose { get { return previousClose; } set { previousClose = value; } }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5}",
                this.TimeStamp.ToString("yyyyMMdd") + "-" + this.timeStamp.ToString("HH:mm:ss"),
                this.Close,
                this.High,
                this.Low,
                this.Open,
                this.Volume);
        }
    }
}
