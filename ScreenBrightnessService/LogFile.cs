using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenBrightnessService
{
    public class LogFile
    {
        private FileStream Logfilefs;
        private StreamWriter Logfilesw;
        string LogFilePath;

        public LogFile(string LogFileWithPath)
        {
            LogFilePath = LogFileWithPath;
        }

        public void CreateLog()
        {
            try
            {
                FileStream fs = new FileStream(LogFilePath, FileMode.Create,FileAccess.Write,FileShare.Read);
                fs.Flush(); fs.Close();
            }
            catch { }
        }

        public void OpenLog()
        {
            try
            {
                Logfilefs = new FileStream(LogFilePath,FileMode.Append,FileAccess.Write,FileShare.Read);
                Logfilesw = new StreamWriter(Logfilefs);
            }
            catch { }
        }

        public void WriteLog(string msg)
        {
            try
            {
                Logfilesw.WriteLine(msg);
                Logfilesw.Flush();
                Logfilefs.Flush();
            }
            catch { }
        }

        public void CloseLog()
        {
            try
            {
                Logfilesw.Close();
                Logfilefs.Close();
            }
            catch { }
        }

    }
}
