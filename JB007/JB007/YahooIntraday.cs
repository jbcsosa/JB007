using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JB007
{
    class YahooIntraday
    {
        public static void download(IYahooDownloadStream downloadStream)
        {
            //sURL = "http://www.microsoft.com";
            //sURL = "https://www.google.com";
            string sURL = downloadStream.getURL();
            WebRequest wrGETURL = WebRequest.Create(sURL);

            //WebProxy myProxy = new WebProxy("myproxy", 80);
            //myProxy.BypassProxyOnLocal = true;
            //wrGETURL.Proxy = WebProxy.GetDefaultProxy();

            string sLine = "";
            Stream objStream = wrGETURL.GetResponse().GetResponseStream();
            StreamReader objReader = new StreamReader(objStream);
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                {
                    downloadStream.downloadStreamParser(sLine);
                }
            }
        }
        public static void openCache(IYahooDownloadStream downloadStream, string path)
        {
            using (System.IO.FileStream fs = File.Open(path, FileMode.Open))
            {
                string sLine = "";
                StreamReader objReader = new StreamReader(fs);

                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                    {
                        downloadStream.downloadStreamParser(sLine);
                    }
                }
            }
        }
        public interface IYahooDownloadStream
        {
            string getURL();
            void downloadStreamParser(string line);
        }
        public class CSV : IYahooDownloadStream
        {
            private string Tick;
            private long gmtOffset = 0;
            private int Days = 1;

            private List<string> Quote;
            private Dictionary<string, string> MetaInfo;
            private const string URLFORMAT_Intraday = "http://chartapi.finance.yahoo.com/instrument/1.0/{0}/chartdata;type=quote;range={1}d/csv";
            public CSV()
            {
                this.Quote = new List<string>();
                this.MetaInfo = new Dictionary<string, string>();
            }
            public CSV(string tick, int days)
            {
                this.Tick = tick;
                this.Days = days;
                this.Quote = new List<string>();
                this.MetaInfo = new Dictionary<string, string>();
            }

            public void analysis()
            {
                string[] tmp = this.MetaInfo["Timestamp"].Split(',');
                long lStart = long.Parse(tmp[0]);
                long lEnd = long.Parse(tmp[1]);
                DateTime dtStart = timestampParser(lStart, gmtOffset);
                DateTime dtEnd = timestampParser(lEnd, gmtOffset);
                // parameters
                Console.WriteLine("Tick: {0}", this.Tick);
                Console.WriteLine("Transaction Period: {0}-{1}", dtStart, dtEnd);
                Console.WriteLine("Transaction Times: {0}", this.Quote.Count);
                foreach (string line in this.Quote)
                {
                    tmp = line.Split(',');
                    DateTime ts = timestampParser(long.Parse(tmp[0]), gmtOffset);
                    Console.WriteLine("{0},{1},{2},{3},{4},{5}", ts.ToLongTimeString(), tmp[1], tmp[2], tmp[3], tmp[4], tmp[5]);
                }
            }
            public string Ticker
            {
                get
                {
                    return this.MetaInfo["ticker"].ToUpper();
                }
            }
            public DateTime RangeStart
            {
                get
                {
                    string[] seconds = this.MetaInfo["Timestamp"].Split(',');
                    return timestampParser(long.Parse(seconds[0]), this.gmtOffset);
                }
            }
            public DateTime RangeEnd
            {
                get
                {
                    string[] seconds = this.MetaInfo["Timestamp"].Split(',');
                    return timestampParser(long.Parse(seconds[1]), this.gmtOffset);
                }
            }
            public float PreviousClose
            {
                get
                {
                    return float.Parse(this.MetaInfo["previous_close"]);
                }
            }
            public List<QuoteTCHLOV> getTransactions()
            {
                List<QuoteTCHLOV> quotes = new List<QuoteTCHLOV>();
                foreach (string line in this.Quote)
                {
                    string[] tmp = line.Split(',');
                    QuoteTCHLOV q = new QuoteTCHLOV();
                    q.TimeStamp = timestampParser(long.Parse(tmp[0]), gmtOffset);
                    q.Close = float.Parse(tmp[1]);
                    q.High = float.Parse(tmp[2]);
                    q.Low = float.Parse(tmp[3]);
                    q.Open = float.Parse(tmp[4]);
                    q.Volume = long.Parse(tmp[5]);
                    q.PreviousClose = this.PreviousClose;
                    //Console.WriteLine(q.ToString());
                    quotes.Add(q);
                }
                return quotes;
            }

            public string getURL()
            {
                return string.Format(URLFORMAT_Intraday, this.Tick, this.Days);
            }
            public static CSV download(string tick, int days = 1)
            {
                CSV intraday = new CSV(tick, days);
                YahooIntraday.download(intraday);
                return intraday;
            }
            public static CSV openCache(string path)
            {
                CSV intraday = new CSV();
                YahooIntraday.openCache(intraday, path);
                return intraday;
            }
            public void downloadStreamParser(string line)
            {
                if (line.Contains(':'))
                {
                    string[] keyValue = line.Split(':');

                    if (keyValue[0] == "range") return;     // if more than 1 day data, there are multiple range value

                    if (!this.MetaInfo.ContainsKey(keyValue[0]))
                    {
                        this.MetaInfo.Add(keyValue[0], keyValue[1]);
                        if (this.Tick == null && keyValue[0] == "ticker")
                        {
                            this.Tick = this.MetaInfo[keyValue[0]];
                        }
                        if (this.gmtOffset == 0 && keyValue[0] == "gmtoffset")
                        {
                            this.gmtOffset = long.Parse(this.MetaInfo["gmtoffset"]);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Duplicate MetaInfo Key: {0}", keyValue[0]);
                    }
                }
                else
                {
                    string ts = line.Split(',')[0];
                    string t = new DateTime((long.Parse(ts) + gmtOffset) * TimeSpan.TicksPerSecond)
                                            .ToString("t", CultureInfo.CreateSpecificCulture("es-ES"));
                    //append time
                    this.Quote.Add(line + "," + ((t.StartsWith("9")) ? "0" : "") + t.Replace(":", ""));
                }
            }
            public void cacheSave(string pathString)
            {
                string data = "";
                foreach (KeyValuePair<string, string> kvp in this.MetaInfo)
                {
                    data += string.Format("{0}:{1}", kvp.Key, kvp.Value) + Environment.NewLine;
                }
                foreach (string line in Quote)
                {
                    data += line + Environment.NewLine;
                }
                Utility.PutFileContent(pathString, data);
            }
            public override string ToString()
            {
                foreach (KeyValuePair<string, string> kvp in this.MetaInfo)
                {
                    Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
                }
                foreach (string line in Quote)
                {
                    Console.WriteLine(line);
                }
                return this.Quote.ToString();
            }
            public static DateTime timestampParser(long seconds, long gmtOffset)
            {
                //return new DateTime((seconds + gmtOffset) * TimeSpan.TicksPerSecond);
                return new DateTime((seconds + gmtOffset) * TimeSpan.TicksPerSecond).AddYears(1969);
            }
        }
    }
}
