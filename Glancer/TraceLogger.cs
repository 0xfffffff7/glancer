using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glancer
{
    public class TraceLogger
    {
         public void InitLogger(string logDirPath, bool bIndent, bool bDate, string logFileName)
        {
            _logDirPath = logDirPath;
            _isLogOutput = true;
            _bIndent = bIndent;
            _bDate = bDate;
            _logFileName = logFileName;
        }

        static internal object _sync = new object();
        bool _isLogOutput = false;
        string _logDirPath = string.Empty;
        List<string> _log = new List<string>();
        public bool _bIndent { get; set; }
        bool _bDate = false;
        public string _logFileName { get; set; }

        public void InputLog(string log)
        {
            if (_isLogOutput == false) { return; }
            _log.Add(log);
        }

        public void OutputLog(string log)
        {
            InputLog(log);
            OutputLog();
        }

        public void OutputLog(){

            if (_isLogOutput == false) { return; }

            string strLogFilePath = string.Empty;
            if (_logFileName == null)
            {
                DateTime dtToday = DateTime.Today;
                strLogFilePath = System.IO.Path.Combine(_logDirPath, dtToday.ToString("yyyy_MM_dd") + ".log");
            }
            else
            {
                strLogFilePath = System.IO.Path.Combine(_logDirPath, _logFileName);
            }

            StringBuilder output = new StringBuilder();

            if (_bDate)
            {
                DateTime dtNow = DateTime.Now;
                output.Append(dtNow.ToString("yyyy/MM/dd/ HH:mm:ss.fff"));
            }

            foreach (string str in _log)
            {
                if (_bIndent) {
                    output.Append("\t");
                }
                output.Append(str);
            }

            lock (_sync) {
                using (StreamWriter writer = new StreamWriter(strLogFilePath, true, Encoding.GetEncoding("utf-8")))
                {
                    writer.WriteLine(output.ToString());
                }
            }

            _log.Clear();
        }
    }
}
