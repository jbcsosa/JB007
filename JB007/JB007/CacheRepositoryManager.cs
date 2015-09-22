using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JB007
{
    class CacheRepositoryManager
    {
        private string RepositoryRootFolder;
        public const string CN_PATTERN_INTRADAY_1DAY = "*_1d_*.csv";
        public CacheRepositoryManager(string rootFolder)
        {
            this.RepositoryRootFolder = rootFolder;
        }
        public List<string> GetCacheIntradayPathList(string ticker, int days, string namePattern = CN_PATTERN_INTRADAY_1DAY)
        {
            int i = 0;
            List<string> list = new List<string>();
            DirectoryInfo di = new DirectoryInfo(this.getTickFolderName(ticker));
            var dir = (from file in di.EnumerateFiles(namePattern)
                           //orderby file.CreationTime ascending
                       orderby file.Name descending
                       select file.FullName);//.Distinct();

            string prev = "";
            foreach (string path in dir)
            {
                string[] s = path.Split('_');
                if (prev == s[1]) continue;     // check same date and skip
                if (i++ >= days) break;
                list.Add(path);
                prev = s[1];
            }
            return list;
        }
        public string GetCacheFileDate(string path, string ticker)
        {
            string date = path.Substring(path.IndexOf(ticker + "_") + ticker.Length + 1, 8);
            return date;
        }
        /// <summary>
        /// Cache folder for a tick
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public string getTickFolderName(string tick)
        {
            string folder = System.IO.Path.Combine(this.RepositoryRootFolder, string.Format("{0}", tick));
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return folder;
        }
        /// <summary>
        /// Yahoo Intraday cache convention name 
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public string getTickYahooIntradayCacheFileName(string tick, DateTime rangeStart, int days = 1)
        {
            string downloadDate = DateTime.Now.ToString("yyyyMMdd");
            string path = System.IO.Path.Combine(this.getTickFolderName(tick),
                string.Format("{0}_{1}_{2}d_{3}.csv",
                tick,
                rangeStart.ToString("yyyyMMdd"),    // date in intraday
                days,
                downloadDate));
            return path;
        }
        //note: method is created because timestamp converted problem where year is 0046 and corrected
        public List<string> Rename(string ticker)
        {
            List<string> list = new List<string>();
            DirectoryInfo di = new DirectoryInfo(this.getTickFolderName(ticker));
            var dir = (from file in di.EnumerateFiles()
                       select file.FullName);//.Distinct();

            foreach (string path in dir)
            {
                if (path.Contains("_0046"))
                {
                    string to = path.Replace("_0046", "_2015");
                    System.IO.File.Move(path, to);
                }
            }
            return list;
        }
    }
}
