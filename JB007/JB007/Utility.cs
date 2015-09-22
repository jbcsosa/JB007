using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JB007
{
    class Utility
    {
        const string REXE = @"\Program Files\R\R-3.2.2\bin\x64\R.exe";
        const string REXE_ARGS = @"-q --slave --no-save --no-restore -f {0} > {1}";
        const string RCMD = @"\Program Files\R\R-3.2.2\bin\x64\Rcmd.exe";
        const string RCMD_ARGS = @"BATCH ";
        public static void RunR(string RPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.FileName = RCMD;
            //startInfo.Arguments = RCMD_ARGS + RPath;
            startInfo.FileName = REXE;
            startInfo.Arguments = string.Format(REXE_ARGS, RPath, RPath + "out");
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
        public static string GetFileContent(string path)
        {
            using (System.IO.FileStream fs = File.Open(path, FileMode.Open))
            {
                StreamReader io = new StreamReader(fs);
                return io.ReadToEnd();
            }
        }
        public static void PutFileContent(string path, string content)
        {
            using (System.IO.FileStream fs = File.Open(path, FileMode.Create))
            {
                StreamWriter io = new StreamWriter(fs);
                io.Write(content);
                io.Flush();
            }
        }
    }
}
