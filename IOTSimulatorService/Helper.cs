using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace IOTSimulatorService
{
    public class Helper
    {
        string startDateTime = string.Empty;
        string endDateTime = string.Empty;

        //private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Logger objLogger = new Logger();
        public SimulatorConfig GetSimulatorConfig()
        {
            try
            {
                string path = ConfigurationManager.AppSettings["JsonFilePath"].ToString().Trim();
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    //SimulatorConfig simulatorConfig = new SimulatorConfig();
                    SimulatorConfig simulatorConfig = JsonConvert.DeserializeObject<SimulatorConfig>(json);
                    //if (!isComplete)
                    //{
                    //    /* Get Table needs export today. In this case LastModifiedDate will be yesterdays date and status should be S
                    //     * If any table failed to export then it needs retry after some time */
                    //    List<ExportTable> exportTableList = tableCollection.ExportTableList.FindAll(
                    //            s => (s.Status == "F")
                    //                    || (Convert.ToInt32(s.LastModifiedDate) < Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")) & s.Status == "S"));
                    //    tableCollection.ExportTableList = exportTableList;
                    //}
                    return simulatorConfig;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        //private void UpdateTableSyncStatus(string tableName, string status)
        //{
        //    try
        //    {
        //        string path = ConfigurationManager.AppSettings["JsonFilePath"].ToString().Trim();
        //        string json = File.ReadAllText(path);
        //        var jObject = JObject.Parse(json);
        //        JArray exportTableArrary = (JArray)jObject["ExportTableList"];
        //        foreach (var exportTable in exportTableArrary.Where(obj => obj["SourceTableName"].Value<string>() == tableName))
        //        {
        //            exportTable["Status"] = !string.IsNullOrEmpty(status) ? status : "";
        //            exportTable["LastModifiedDate"] = DateTime.Now.ToString("yyyyMMdd");
        //        }

        //        jObject["ExportTableList"] = exportTableArrary;
        //        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jObject, Newtonsoft.Json.Formatting.Indented);
        //        File.WriteAllText(path, output);

        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        //public async Task ParquetFileHDFUploadAsync(TriggerType triggerType)
        //{
        //    try
        //    {
        //        string previousDay = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
        //        objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "====================================" + previousDay + "==========================================");
        //        DeleteOldFiles();
        //        // return;
        //        /*Read JSON file to get export info for source & destination DB connection and export table list */
        //        ExportInfo exportInfo = GetListOfTableCopiers(false);

        //        if (exportInfo.ExportTableList.Count > 0)
        //        {
        //            /* DB & HDF connection created */
        //            Database db = DatabaseFactory.CreateDatabase(exportInfo.SourceDbConnectionString);
        //            exportInfo.HDFConnectionString.CreateHDFConnection();
        //            // objLogger.INFO("DB & HDF connections are created");
        //            objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "DB & HDF connections are created");
        //            /* Loop through export table */
        //            startDateTime = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
        //            //objLogger.INFO("===========Started:" + startDateTime);
        //            objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "===========Started:" + startDateTime);
        //            foreach (ExportTable expportTable in exportInfo.ExportTableList)
        //            {
        //                try
        //                {
        //                    /* Below code is added if required to push any specfic date data then take
        //                     * value from app.config else condier previous day date */
        //                    string manualDate = ConfigurationManager.AppSettings["ManualDate"].ToString().Trim();
        //                    previousDay = manualDate.Length > 0 ? manualDate : previousDay;
        //                    /* Execute query in export list and get pervious date data from DB */
        //                    DataTable sourceData = expportTable.GetTableDataWithFitler(db, previousDay);
        //                    if (sourceData.Rows.Count == 0)
        //                    {
        //                        //objLogger.INFO(expportTable.SourceTableName + " : No record found"); 
        //                        objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, expportTable.SourceTableName + " : No record found");
        //                        UpdateTableSyncStatus(expportTable.SourceTableName, "S");
        //                        continue;
        //                    }

        //                    /* Create Parquet file and upload to temporary path */
        //                    #region ConvertAndSaveInTempPath
        //                    string tempPath = ConfigurationManager.AppSettings["TempFilepath"].ToString().TrimEnd('/') + "/" + previousDay;
        //                    Guid guid = Guid.NewGuid();
        //                    if (!Directory.Exists(tempPath))
        //                    {
        //                        DirectoryInfo di = Directory.CreateDirectory(tempPath);
        //                        // objLogger.INFO("Directory created:" + tempPath);
        //                        objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "Directory created:" + tempPath);
        //                    }
        //                    /* Temporary File Path and format "yyyyMMdd/tablename_guid_triggertype.parquet"*/
        //                    string tempFilePath = tempPath + "/" + expportTable.SourceTableName + "_" + guid.ToString() + "_" + triggerType.ToString() + ".parquet";
        //                    TableParquetFileConverter.ConvertTableToParquetFile(sourceData, tempFilePath);
        //                    //objLogger.INFO("Parquet file created:" + tempFilePath);
        //                    objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "Parquet file created:" + tempFilePath);
        //                    #endregion

        //                    #region FileUploadToHDF
        //                    /* Upload file to HDF 
        //                       For Hong Kong:landing/oracle_hkdc/<DB-Username>/<Table-Name>/YYYY/MM/DD/HH/
        //                       /landing/oracle_hkdc/RFCMES/LDB1_ADDITIONPARAMETER/2019/09/16/09/LDB1_ADDITIONPARAMETER_c481d478-ae1c-4547-a3f9-e375e833c1ab_Manual.parquet
        //                       For EU:landing/oracle_eu/<DB-Username>/<Table-Name>/YYYY/MM/DD/HH/
        //                       /landing/oracle_eu/USER_VEGA/LDB1_ADDITIONPARAMETER/2020/08/12/13/LDB1_ADDITIONPARAMETER_014d8ff3-b32b-489a-a13b-56b5eab94db8_ScheduleTrigger.parquet */

        //                    if (ConfigurationManager.AppSettings["HDFPush"].ToString().Trim().ToUpper() == "TRUE")
        //                    {
        //                        string fileName = expportTable.SourceTableName + "_" + guid.ToString() + "_" + triggerType.ToString() + ".parquet";
        //                        string hdfDirectoryPath = exportInfo.HDFConnectionString.GetHDFDirectoryPath(expportTable.SourceTableName);
        //                        bool status = await exportInfo.HDFConnectionString.UploadFile(hdfDirectoryPath, fileName, tempFilePath);
        //                        if (status)
        //                        {
        //                            //objLogger.INFO("Parquet file uploaded to HDF=> Path: " + hdfDirectoryPath + " File Name: " + fileName);
        //                            objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "Parquet file uploaded to HDF=> Path: " + hdfDirectoryPath + " File Name: " + fileName);
        //                            UpdateTableSyncStatus(expportTable.SourceTableName, "S");
        //                        }
        //                        else
        //                        {
        //                            //objLogger.Error("Failed to upload Parquet file in HDF=> Path: " + hdfDirectoryPath + " File Name: " + fileName);
        //                            objLogger.LogMsg(LogModes.UPLOAD, LogLevel.ERROR, "Failed to upload Parquet file in HDF=> Path: " + hdfDirectoryPath + " File Name: " + fileName);
        //                            UpdateTableSyncStatus(expportTable.SourceTableName, "F");
        //                        }
        //                    }
        //                    #endregion
        //                }
        //                catch (Exception ex)
        //                {
        //                    objLogger.LogMsg(LogModes.UPLOAD, LogLevel.ERROR, ex.Message.ToString());
        //                    //objLogger.Error(ex.Message.ToString());
        //                }
        //            }
        //            endDateTime = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
        //            //objLogger.Error("==============Completed:" + endDateTime);
        //            objLogger.LogMsg(LogModes.UPLOAD, LogLevel.INFO, "==============Completed:" + endDateTime);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.LogMsg(LogModes.UPLOAD, LogLevel.ERROR, ex.Message.ToString());
        //    }
        //}
        private void DeleteOldFiles()
        {
            try
            {
                string tempPath = ConfigurationManager.AppSettings["TempFilepath"].ToString().TrimEnd('/');
                int deleteFilesOlder = Convert.ToInt32(ConfigurationManager.AppSettings["DeleteFilesOlder"].ToString().TrimEnd('/'));

                if (Directory.Exists(tempPath))
                {
                    //objLogger.INFO(" Directory Deleted: " + tempPath);
                    objLogger.LogMsg(LogModes.OnRun, LogLevel.INFO, " Directory Deleted: " + tempPath);
                    string[] directories = Directory.GetDirectories(tempPath);
                    foreach (string directory in directories)
                    {
                        DateTime dt = Directory.GetCreationTime(directory);
                        if (DateTime.Now.Subtract(dt).TotalDays > deleteFilesOlder)
                        {
                            Directory.Delete(directory, true);
                        }

                    }
                }

                /* Delete log files older than configuration */
                var date = DateTime.Now.AddDays(-deleteFilesOlder);
                //var task = new LogFileCleanupTask();
                //task.CleanUp(date);
            }
            catch (Exception ex)
            {
                objLogger.LogMsg(LogModes.OnRun, LogLevel.ERROR, ex.Message.ToString());

            }
        }
    }

    public class SimulatorConfig
    {
        public BrokerConfig MQTTBrokerConfig { get; set; }
        public TimeInterval[] TimeIntervals { get; set; }
        public string[] DownTimeReasons { get; set; }
        public string[] ProductCodes { get; set; }
    }
    public class BrokerConfig
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientID { get; set; }
    }

    public class TimeInterval
    {
        public Int64 TimeDuration { get; set; }
        public bool DependedTopic { get; set; }
        public TopicInfo[] Topics { get; set; }
    }

    public class TopicInfo
    {
        private int _step = 0;
        private int _temp = 0;
        public int Temp
        {
            get { return _temp; }
            set { _temp = value; }
        }
        public int Min { get; set; }
        public int Max { get; set; }
        public string[] Tags { get; set; }
        public string[] Values { get; set; }
        public bool RunAlone { get; set; }
        public bool IsRunning { get; set; }
        public int Step
        {
            get { return _step; }
            set { _step = value; }
        }
    }
}
