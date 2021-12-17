using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using uPLibrary.Networking.M2Mqtt;

namespace IOTSimulatorService
{
    partial class SimulatorService : ServiceBase
    {
        private static Logger objLogger = new Logger();
        SimulatorConfig simulatorConfig;
        MqttClient mqttClient;
        List<Timer> timerList = new List<Timer>();
        public SimulatorService()
        {
            InitializeComponent();
            objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, "==================Service Started================");
        }

        protected override void OnStart(string[] args)
        {
            Helper helper = new Helper();
            simulatorConfig = helper.GetSimulatorConfig();
            mqttClient = new MqttClient(simulatorConfig.MQTTBrokerConfig.HostName, simulatorConfig.MQTTBrokerConfig.Port, true, null, null, MqttSslProtocols.TLSv1_2, RemoteCertificateValidationCallback);
            mqttClient.Connect(simulatorConfig.MQTTBrokerConfig.ClientID, simulatorConfig.MQTTBrokerConfig.UserName, simulatorConfig.MQTTBrokerConfig.Password);

            objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, "--------Start------------------");

            if(simulatorConfig != null)
            {
                if(simulatorConfig.TimeIntervals.Length > 0)
                {
                    foreach(TimeInterval timerInterval in simulatorConfig.TimeIntervals)
                    {
                        System.Timers.Timer timer1 = new System.Timers.Timer();
                        timer1.Enabled = true;
                        timer1.Interval = timerInterval.TimeDuration;
                        timer1.Elapsed += new ElapsedEventHandler((sender, e) => TimerTick(sender, e, timerInterval));
                        timer1.Start();
                        
                    }

                }
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        public void TimerTick(object source, ElapsedEventArgs e, TimeInterval timerInterval)
        {
            int step = 0;
            objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, "==================Running: "  + timerInterval.TimeDuration + " Start ================");
            if (timerInterval.DependedTopic == true)
            {
                List<TopicInfo> topics = timerInterval.Topics.Where(i => i.RunAlone == true).ToList();// && i.IsRunning == false && i.Temp >= i.Max).ToList();
                if (topics.Count > 1)
                    objLogger.LogMsg(LogModes.OnRun, LogLevel.ERROR, "More than one topic should not confiure with RunAlone True");
                else if (topics.Count == 1)
                {
                    if(topics[0].IsRunning == false && topics[0].Temp >= topics[0].Max)
                    {
                        topics[0].IsRunning = true;
                        topics[0].Temp = 0;
                        if (topics[0].Step >= topics[0].Values.Length)
                            topics[0].Step = 0;
                        else
                            topics[0].Step++;
                    }
                    else if (topics[0].IsRunning == true && topics[0].Temp >= topics[0].Min)
                    {
                        topics[0].IsRunning = false;
                        topics[0].Temp = 0;
                    }
                    objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, "Numn: : " + topics[0].IsRunning.ToString());
                    objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, "Numn: : " + topics[0].Temp.ToString());
                    if (!topics[0].IsRunning)
                    {
                        List<TopicInfo> runningtopics = timerInterval.Topics.Where(i => i.RunAlone == false).ToList();
                        IncreamentCount(runningtopics);
                    }
                    else
                    {
                        step = topics[0].Step;
                        foreach (string tag in topics[0].Tags)
                        {
                            Task.Run(() =>
                            {
                                if (ConfigurationManager.AppSettings["PushToBroker"].ToString().ToUpper() == "TRUE")
                                {
                                    if (mqttClient.IsConnected)
                                    {
                                        mqttClient.Publish(tag, Encoding.UTF8.GetBytes(topics[0].Values[step].ToString()));
                                    }
                                }
                                else
                                {
                                    objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, topics[0].Values[step]);
                                }
                            });
                        }
                    }

                    topics[0].Temp++;
                }
            }
            else
            {
                IncreamentCount(timerInterval.Topics.ToList());
                
            }
            objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, "==================Running: " + timerInterval.TimeDuration + " End ================");
        }

        private void IncreamentCount(List<TopicInfo> topics)
        {
            foreach (TopicInfo topic in topics)
            {
                string value = string.Empty;
                if (topic.Values != null && topic.Values.Length > 0)
                {
                    value = topic.Values[topic.Temp];
                    topic.Temp = topic.Temp + 1;
                    if (topic.Temp >= topic.Values.Length)
                        topic.Temp = topic.Min;
                }
                else
                {
                    topic.Temp = topic.Temp + 1;
                    if (topic.Temp > topic.Max)
                        topic.Temp = topic.Min;

                    value = topic.Temp.ToString();
                }

                foreach (string tag in topic.Tags)
                {
                    Task.Run(() =>
                    {
                        if (ConfigurationManager.AppSettings["PushToBroker"].ToString().ToUpper() == "TRUE")
                        {
                            if (mqttClient.IsConnected)
                            {
                                mqttClient.Publish(tag, Encoding.UTF8.GetBytes(value.ToString()));
                            }
                        }
                        else
                        {
                            objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, tag + ": " + value);
                        }

                    });
                }
            }
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            //throw new NotImplementedException();
        }

        public class LogFileCleanupTask
        {
            //  private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            private static Logger objLogger = new Logger();
            #region - Constructor -
            public LogFileCleanupTask()
            {
            }
            #endregion

            #region - Methods -
            /// <summary>
            /// Cleans up. Auto configures the cleanup based on the log4net configuration
            /// </summary>
            /// <param name="date">Anything prior will not be kept.</param>
            public void CleanUp(DateTime date)
            {
                try
                {
                    string directory = string.Empty;
                    string filePrefix = string.Empty;

                    var repo = LogManager.GetAllRepositories().FirstOrDefault(); ;
                    if (repo == null)
                        throw new NotSupportedException("Log4Net has not been configured yet.");

                    var app = repo.GetAppenders().Where(x => x.GetType() == typeof(RollingFileAppender)).FirstOrDefault();
                    if (app != null)
                    {
                        var appender = app as RollingFileAppender;

                        directory = Path.GetDirectoryName(appender.File);
                        filePrefix = Path.GetFileName(appender.File);

                        CleanUp(directory, filePrefix, date);
                    }
                }
                catch (Exception ex)
                {
                    objLogger.LogMsg(LogModes.OnRun, LogLevel.ERROR, ex);
                    // objLogger.Error(ex);
                }
            }
            /// <summary>
            /// Cleans up.
            /// </summary>
            /// <param name="logDirectory">The log directory.</param>
            /// <param name="logPrefix">The log prefix. Example: logfile dont include the file extension.</param>
            /// <param name="date">Anything prior will not be kept.</param>
            public void CleanUp(string logDirectory, string logPrefix, DateTime date)
            {
                try
                {
                    if (string.IsNullOrEmpty(logDirectory))
                        throw new ArgumentException("logDirectory is missing");

                    if (string.IsNullOrEmpty(logPrefix))
                        throw new ArgumentException("logPrefix is missing");

                    var dirInfo = new DirectoryInfo(logDirectory);
                    if (!dirInfo.Exists)
                        return;

                    FileInfo fi = new FileInfo(logPrefix);
                    var fileInfos = dirInfo.GetFiles("*" + fi.Extension.ToString());
                    if (fileInfos.Length == 0)
                        return;

                    foreach (FileInfo info in fileInfos)
                    {
                        if (info.CreationTime < date)
                        {
                            info.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    objLogger.LogMsg(LogModes.OnRun, LogLevel.ERROR, ex);
                }
            }
            #endregion
        }

       //// For Debugging uncomment below method
       // public void Run()
       // {
       //     OnStart(null);
       // }

    }


}
