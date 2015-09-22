using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JB007
{
    class ListManager
    {
        private const string JB_ListFile = "List.txt";
        private string RepositoryRootFolder;
        private string listFile;
        private List<string> list;
        public ListManager(string rootFolder)
        {
            this.RepositoryRootFolder = rootFolder;
            this.listFile = this.RepositoryRootFolder + @"\" + JB_ListFile;
            //this.ReadList();
        }
        public List<string> TickList
        {
            get
            {
                if (this.list == null) return ReadList();
                return this.list;
            }
        }
        public string ListFile { get { return this.listFile; } }
        public List<string> ReadList(string listFile=null)
        {
            if (listFile == null) listFile = this.ListFile;
            list = new List<string>();
            using (System.IO.FileStream fs = File.Open(listFile, FileMode.Open))
            {
                StreamReader objReader = new StreamReader(fs);
                string sLine = "";
                int i = 0;
                while (sLine != null)
                {
                    i++;
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                    {
                        string[] kv = sLine.Split(',');
                        list.Add(kv[0]);
                    }
                }
                this.listFile = listFile;
            }
            return list;
        }
    }
}
