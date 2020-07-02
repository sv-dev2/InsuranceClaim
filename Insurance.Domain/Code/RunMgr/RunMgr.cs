using SV.Domain.Code;
using System;
using System.Configuration;
using System.Net;
using System.Reflection;

namespace Insurance.Domain.Code
{
    public class RunMgr
    {
        #region The Singleton definition
        // the one and only Singleton instance. 
        private static readonly RunMgr instance = new RunMgr();
        // private constructor. 
        private RunMgr()
        {
            _initHasRun = false;
            LoggedInInfo = new LoggedIn();
        }

        // gets the instance of the singleton object
        public static RunMgr Instance
        {
            get { return instance; }
        }

        private bool _initHasRun { get; set; }
        #endregion

        public LoggedIn LoggedInInfo { get; set; }
        public void Init()
        {
            try
            {
                if (!_initHasRun)
                {
                    _initHasRun = true;                  
                   

                    // set system wide severity
                    Logger.Instance.Severity = LogSeverity.Info;

                    var eventLog = new ObserverLogToEventlog();
                    Logger.Instance.Attach(eventLog);

                    var logConsole = new ObserverLogToConsole();
                    Logger.Instance.Attach(logConsole);

                    // send log messages to a file
                 
                  

                    Logger.Instance.Info("Up and running ==> " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    Logger.Instance.Info("Config: " + InsuranceContext.ConnectionString);


                    //var result = CheckForInternetConnection();
                    //if(result == true)
                    LoggedInInfo.InitDb();

                    //else {
                    //    throw new Exception("Internet not connected to your system");
                    //}

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Init failed: " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }



        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

