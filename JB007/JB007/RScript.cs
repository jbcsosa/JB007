using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JB007
{
    class RScript
    {
        private static string NEWLINE = Environment.NewLine;
        private static Dictionary<string, string> R_metaVars = new Dictionary<string, string>
        {
            ["close"] = "close = c(qIntraday{0}$C[nrow(qIntraday{1})], close)",
            ["high"] = "high = c(max(qIntraday{0}$C), high)",
            ["low"] = "low = c(min(qIntraday{0}$C), low)",
            ["open"] = "open = c(qIntraday{0}$O[1], open)",
            ["vol"] = "vol = c(sum(qIntraday{0}$Volume), vol)",
            ["CGOVol"] = "CGOVol=c(sum(qIntraday{0}[qIntraday{1}$C>qIntraday{2}$O[1], \"Volume\"]), CGOVol)",
            ["CLOVol"] = "CLOVol =c(sum(qIntraday{0}[qIntraday{1}$C<qIntraday{2}$O[1],\"Volume\"]), CLOVol)",
            ["transCount"] = "transCount = c(nrow(subset(qIntraday{0}, Volume>0)), transCount)",
            ["maxV"] = "maxV = c(max(qIntraday{0}$Volume), maxV)"       // maximum transaction volume in the day
        };
        public static string RScriptIntraday(string ticker, CacheRepositoryManager cacheMgr, int days = 15)
        {
            string script = string.Format("##### {0}: Auto R Script Generated #####", ticker) + NEWLINE;
            script += string.Format("ticker = \"{0}\"", ticker) + NEWLINE;

            // Initial R variables
            script += "date = c()" + NEWLINE;
            foreach (string key in R_metaVars.Keys)
            {
                script += key + " = c()" + NEWLINE;
            }

            int i = 0;
            foreach (string path in cacheMgr.GetCacheIntradayPathList(ticker, days))
            {
                string date = cacheMgr.GetCacheFileDate(path, ticker);

                if (long.Parse(date) < 20150917)    //converted time column was add in the csv later
                {
                    script += string.Format(
                        "qIntraday{0} = read.table(\"{1}\", header=F, sep=\",\", skip=17, col.names=c(\"TS\",\"C\",\"H\",\"L\",\"O\",\"Volume\"))",
                        i, path.Replace("\\", "/")) + NEWLINE;
                }
                else
                {
                    script += string.Format(
                        "qIntraday{0} = read.table(\"{1}\", header=F, sep=\",\", skip=17, col.names=c(\"TS\",\"C\",\"H\",\"L\",\"O\",\"Volume\",\"T\"))",
                        i, path.Replace("\\", "/")) + NEWLINE;
                }

                //meta info
                script += string.Format("date = c(\"{0}\", date)", date) + NEWLINE;
                foreach (string key in R_metaVars.Keys)
                {
                    script += string.Format(R_metaVars[key], i, i, i, i, i, i) + NEWLINE;
                }
                i++;
            }
            script += "jb_meta = data.frame(date=date";
            foreach (string key in R_metaVars.Keys)
            {
                script += string.Format(",{0} = {1}", key, key);
            }
            script += ")" + NEWLINE;

            //script += "options(\"width\"=300)" + NEWLINE;   // set output width in Rprofile.site
            //script += "sortByC = qIntraday0[order(qIntraday0$C),c(\"C\", \"Volume\")]";
            //script += "sortByC = qIntraday0[order(qIntraday0$C),]" + NEWLINE;
            //script += "groupByC = aggregate(Volume ~ C, qIntraday0, sum)" + NEWLINE;
            //script += "min_date = jb_meta[jb_meta$low == min(jb_meta$low), ]" + Environment.NewLine;
            //script += string.Format("message(\"***** {0} *****\")", Ticker) + NEWLINE;
            //script += "jb_meta[jb_meta$low == min(jb_meta$low), ]" + NEWLINE;


            //Console.Write(script);
            return script;
        }
        public static string RGraph_Intraday(string ticker, CacheRepositoryManager cacheMgr, int startDay=0)
        {
            string[] colors = { "black", "purple", "blue", "green", "red" };
            List<string> cacheList = cacheMgr.GetCacheIntradayPathList(ticker, colors.Length);

            string script = string.Format("##### {0} #####", ticker) + NEWLINE;
            script += "library(\"ggplot2\")" + NEWLINE;
            script += string.Format("graph <- ggplot()") + NEWLINE;

            int i = 0;
            foreach (string path in cacheList)
            {
                if (i++ < startDay) continue;

                string date = cacheMgr.GetCacheFileDate(path, ticker);

                if (long.Parse(date) < 20150917)    //converted time column was added in the csv after this date
                {
                    script += string.Format(
                        "qIntraday{0} = read.table(\"{1}\", header=F, sep=\",\", skip=17, col.names=c(\"TS\",\"C\",\"H\",\"L\",\"O\",\"Volume\"))",
                        i - startDay - 1, path.Replace("\\", "/")) + NEWLINE;
                }
                else
                {
                    script += string.Format(
                        "qIntraday{0} = read.table(\"{1}\", header=F, sep=\",\", skip=17, col.names=c(\"TS\",\"C\",\"H\",\"L\",\"O\",\"Volume\",\"T\"))",
                        i - startDay - 1, path.Replace("\\", "/")) + NEWLINE;
                }

                if ((i-startDay) == colors.Length) break;
            }

            // ggplot2 script
            for(i=0; i<colors.Length; i++)
            {
                script += string.Format(
                    "graph <- graph + geom_point(data=qIntraday{0}, aes(seq(qIntraday{1}$C),qIntraday{2}$C,size=qIntraday{3}$Volume,alpha=.7), colour=\"{4}\")",
                    i, i, i, i, colors[i]) + NEWLINE;
            }
            script += string.Format("graph <- graph + labs(title=\"{0} Intraday\") + ylab(\"Close\")", ticker) + NEWLINE;
            script += string.Format("graph") + NEWLINE;

            return script;
        }
    }
}
