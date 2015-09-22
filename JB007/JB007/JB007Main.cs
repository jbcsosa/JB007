using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace JB007
{
    class JB007Main
    {
        const string CONF_WORKDIR = "DownloadFolder";
        const string CONF_LISTDIR = "DownloadListFolder";
        const string CONF_RSCRIPTDIR = "RScriptFolder";
        const string RScriptFilePattern = "RScriptIntraday_{0}.R";
        const string AllRScriptFilePattern = "all_RScriptIntraday";
        static Dictionary<string, string> Cmds = new Dictionary<string, string>()
        {
            { "Mining", "CMD_Mining" },
            { "RScriptIntraday", "CMD_RScriptIntraday" },
            { "RunRScriptIntraday", "CMD_RunRScriptIntraday" },
            { "MailRScriptIntraday", "CMD_MailRScriptIntraday" },
            { "RGraphScript", "CMD_RGraphScript" },
            { "Download", "CMD_Download" }
        };
        static void Usage()
        {
            Console.WriteLine("Commands ==>");
            foreach(string cmd in Cmds.Keys)
            {
                Console.WriteLine("\t-" + cmd);
            }
        }
        static void Main(string[] args)
        {
            //MethodInfo m = new JB007Main().GetType().GetMethod("CMD_Mining");
            //m.Invoke()
            if (args.Length > 0 && args[0].StartsWith("-"))
            {
                ListManager listManager = new ListManager(AppConf_ReadSetting(CONF_LISTDIR));
                CacheRepositoryManager cacheManager = new CacheRepositoryManager(AppConf_ReadSetting(CONF_WORKDIR));
                string rDir = AppConf_ReadSetting(CONF_RSCRIPTDIR);

                Console.WriteLine("***** DownloadDir: {0}", AppConf_ReadSetting(CONF_WORKDIR));
                Console.WriteLine("***** ListFile: {0}", listManager.ListFile);
                Console.WriteLine("***** R Dir: {0}", rDir);

                for(int i=0; i<args.Length; i++)
                {
                    string tickerList = null;
                    string cmd = args[i].Substring(1);
                    Console.WriteLine("************************************************");
                    Console.WriteLine("* {0} {1}ing ...", DateTime.Now, cmd);
                    switch (cmd)
                    {
                        case "Mining":
                            CMD_Mining(cacheManager, listManager);
                            break;
                        case "Download":
                            if (args.Length > (i+1) && !args[i+1].StartsWith("-"))
                            {
                                tickerList = args[++i];
                            }
                            CMD_Download(listManager, cacheManager, tickerList);
                            break;
                        case "RScriptIntraday":
                            if (args.Length > (i + 1) && !args[i + 1].StartsWith("-"))
                            {
                                tickerList = args[++i];
                            }
                            CMD_RScriptIntraday(listManager, cacheManager, rDir, tickerList);
                            break;
                        case "RunRScriptIntraday":
                            CMD_RunRScriptIntraday(listManager, rDir);
                            break;
                        case "MailRScriptIntraday":
                            CMD_MailRScriptIntraday(rDir);
                            break;
                        case "RGraphScript":
                            CMD_RGraphScript(listManager, cacheManager, rDir);
                            break;
                        default:
                            Console.WriteLine("- Bad command. Try again.");
                            Usage();
                            break;
                    }
                    Console.WriteLine("* {0} {1} Done", DateTime.Now, cmd);
                }
            }
            else
            {
                Usage();
            }
        }
        static void CMD_Mining(CacheRepositoryManager cacheMgr, ListManager listMgr, int days=30)
        {
            foreach (string ticker in listMgr.TickList)
            {
                foreach(string path in cacheMgr.GetCacheIntradayPathList(ticker, days))
                {
                    Console.WriteLine(path);
                    //YahooIntraday.CSV csvIntraday = YahooIntraday.CSV.openCache(path);
                }
            }
        }
        static void CMD_RunRScriptIntraday(ListManager listMgr, string rDir)
        {
            // Read post script to Append
            string postRfile = Path.Combine(rDir, AllRScriptFilePattern + "_Post.R");
            string postScript = Utility.GetFileContent(postRfile);

            string script = "";
            string Rfile = Path.Combine(rDir, AllRScriptFilePattern + ".R");
            foreach (string ticker in listMgr.TickList)
            {
                string path = Path.Combine(rDir, string.Format(RScriptFilePattern, ticker));
                script += string.Format("source(\"{0}\")", path.Replace(@"\", "/")) + Environment.NewLine;
                script += postScript;
            }
            Utility.PutFileContent(Rfile, script);
            // Run all R scripts
            Utility.RunR(Rfile);
        }
        static void CMD_MailRScriptIntraday(string rDir)
        {
            SmtpClient sc = new SmtpClient("smtp.cox.net", 25);
            string from = "jbshiao@cox.net";
            string to = "jbshiao@yahoo.com";
            MailMessage msg = new MailMessage(from, to, "RunRScriptIntraday Rout", "See attached.");
            msg.Attachments.Add(new Attachment(Path.Combine(rDir, AllRScriptFilePattern + ".Rout")));
            //msg.IsBodyHtml = true;
            sc.Send(msg);
        }
        // days=15: default number of days of intraday csv data files
        static void CMD_RScriptIntraday(ListManager listMgr, CacheRepositoryManager cacheMgr, string rScriptFolder, string tickers, int days = 15)
        {
            string Rfile_Append = Path.Combine(rScriptFolder, "RScriptIntraday_Append.R");
            List<string> tickerList = (tickers == null) ? listMgr.TickList : tickers.Split(',').ToList();
            foreach (string ticker in tickerList)
            {
                string path = Path.Combine(rScriptFolder, string.Format(RScriptFilePattern, ticker));
                string script = RScript.RScriptIntraday(ticker, cacheMgr, days);
                script += Utility.GetFileContent(Rfile_Append);
                Utility.PutFileContent(path, script);
                //Console.WriteLine(path);
            }
        }
        static void CMD_RGraphScript(ListManager listMgr, CacheRepositoryManager cacheMgr, string rScriptFolder, int days =5)
        {
            const string fileNamePattern = "RGraph_{0}.R";
            foreach (string ticker in listMgr.TickList)
            {
                string path = Path.Combine(rScriptFolder, string.Format(fileNamePattern, ticker));
                Utility.PutFileContent(path, RScript.RGraph_Intraday(ticker, cacheMgr, 0));
                //Console.WriteLine(path);
            }
        }
        static void CMD_Download(ListManager listMgr, CacheRepositoryManager cacheManager, string tickers, int[] days = null)
        {
            if (days == null) days = new int[] { 1 };
            List<string> tickerList = (tickers == null) ? listMgr.TickList : tickers.Split(',').ToList();
            foreach (string ticker in tickerList)
            {
                foreach (int n in days)
                {
                    YahooIntraday.CSV csvIntraday = YahooIntraday.CSV.download(ticker, n);
                    string cacheFileName = cacheManager.getTickYahooIntradayCacheFileName(ticker, csvIntraday.RangeStart, n);
                    csvIntraday.cacheSave(cacheFileName);
                    //Console.WriteLine(cacheFileName);
                }
            }
        }
        static string AppConf_ReadSetting(string key)
        {
            string result = "";
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
            return result;
        }
    }
}
