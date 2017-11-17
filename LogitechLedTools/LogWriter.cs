using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogitechLedTools
{
    public class LogWriter
    {
        public enum Level
        {
            DEBUG, INFO, WARNING, ERROR, FATAL
        }

        private static LogWriter instance = null;

        public static LogWriter getInstance(string filename)
        {
            if (instance == null)
            {
                instance = new LogWriter(filename);
            }
            return instance;
        }

        public static string getLevelText(Level level)
        {
            switch (level)
            {
                case Level.INFO:
                    return "info";
                case Level.DEBUG:
                    return "debug";
                case Level.WARNING:
                    return "warning";
                case Level.ERROR:
                    return "error";
                case Level.FATAL:
                    return "fatal";
            }
            return "unknown";
        }

        private StreamWriter writerLog;

        public LogWriter(string filename)
        {
            writerLog = File.AppendText(filename);
            writerLog.AutoFlush = true;
        }

        public void Dispose()
        {
            if (writerLog != null)
            {
                writerLog.Flush();
                writerLog.Dispose();
                writerLog = null;
            }
            instance = null;
        }

        public void Log(string message)
        {
            Log(message, Level.ERROR);
        }

        public void Log(string message, Level level)
        {
            if (writerLog == null)
            {
                return;
            }
            writerLog.Write( string.Format("[{0}] {1,8}: {2}\r\n", DateTime.Now.ToString(), getLevelText(level), message) );
        }

    }
}
