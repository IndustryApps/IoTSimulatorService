using System;
using log4net;

namespace IOTSimulatorService
{

    enum LogLevel : int
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4
    }

    enum LogModes : int
    {
        OnRun = 0
    }

    class Logger
    {
        public void LogMsg(LogModes mode, LogLevel level, object msg)
        {
            string strLogFileName = String.Empty;

            switch (mode)
            {
                case LogModes.OnRun:
                    strLogFileName = "log-";
                    break;

                //case LogModes.EMAIL:
                //    strLogFileName = "emaillog-";
                //    break;
            }

            log4net.GlobalContext.Properties["LogFileName"] = strLogFileName;
            log4net.Config.XmlConfigurator.Configure();

            ILog log = LogManager.GetLogger(typeof(Logger));

            switch (level)
            {
                case LogLevel.DEBUG:
                    log.Debug(msg);
                    break;

                case LogLevel.INFO:
                    log.Info(msg);
                    break;

                case LogLevel.WARN:
                    log.Warn(msg);
                    break;

                case LogLevel.ERROR:
                    log.Error(msg);
                    break;

                case LogLevel.FATAL:
                    log.Fatal(msg);
                    break;
            }
        }
    }
}
