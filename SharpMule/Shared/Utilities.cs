using System;
using System.Collections.Generic;
using System.Diagnostics; 
using System.Linq;
using System.Text;
using TestManager.TreeViewCtrl; 
using SharpMule.Automation.Framework.Essentials; 

namespace TestManager.Shared
{
    public class Utilities
    {
        public static void StartNewProcess(string executable, string arguments)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo();
            procStartInfo.FileName = executable;
            procStartInfo.Arguments = arguments;
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }


        public static void SendEmail()
        {
            TestUtilities.EmailClient EmailClient = new TestUtilities.EmailClient(SharedTasks.Email.EmailAddress, SharedTasks.TestSuitInstances);
            EmailClient.Environment = SharedTasks.Environments.DefaultEnvironment;
            EmailClient.EndPoint = SharedTasks.Environments.GetEnvironmentByName(SharedTasks.Environments.DefaultEnvironment).DefaultEndpoint;
            EmailClient.SuiteName = SharedTasks.CurrentTestTitle;
            EmailClient.CurrentUser = Environment.UserName.ToUpper();
            EmailClient.Description = SharedTasks.GetFailedTestCaseCount()==0?"All Test Cases have Passed":"Number of tests cases failed:"+SharedTasks.GetFailedTestCaseCount();
            EmailClient.SmtpHost = SharedTasks.Email.SmtpHost; 

            if (SharedTasks.Email.IsEnabled)
                EmailClient.SendEmail(); 
        }
        
        public class Timer
        {
            private static DateTime startTime;
            private static DateTime stopTime;
            private static bool running = false;


            public static void Start()
            {
                startTime = DateTime.Now;
                running = true;
            }


            public static void Stop()
            {
                stopTime = DateTime.Now;
                running = false;
            }

         
            // elaspsed time in milliseconds
            public static double GetElapsedTime()
            {
                TimeSpan interval;

                if (running)
                    interval = DateTime.Now - startTime;
                else
                    interval = stopTime - startTime;

                return interval.TotalMilliseconds;
            }


            // elaspsed time in seconds
            public static double GetElapsedTimeSecs()
            {
                TimeSpan interval;

                if (running)
                    interval = DateTime.Now - startTime;
                else
                    interval = stopTime - startTime;

                return interval.TotalSeconds;
            }
           
        }

        public class EventLogs
        {
            string machine;

            public EventLogs(string targetMachine)
            {
                machine = targetMachine; 
            }

            public string GetEventLogs(int numOfRecord)
            {
                //logType can be Application, Security, System or any other Custom Log.
                string logType = "Application";

                StringBuilder strb = new StringBuilder();

                try
                {
                    EventLog ev = new EventLog(logType, machine);
                    int LastLogToShow = ev.Entries.Count;
                    if (LastLogToShow <= 0)
                        strb.Append("No Event Logs in the Log :" + logType);

                    // Read the last 2 records in the specified log. 
                    int i;
                    for (i = ev.Entries.Count - 1; i >= LastLogToShow - numOfRecord; i--)
                    {
                        
                        EventLogEntry CurrentEntry = ev.Entries[i];

                        if (CurrentEntry.EntryType == EventLogEntryType.Error || 
                            CurrentEntry.EntryType == EventLogEntryType.Warning ||
                            CurrentEntry.EntryType == EventLogEntryType.FailureAudit)
                        {
                            strb.Append("Event ID : " + CurrentEntry.InstanceId.ToString()+Environment.NewLine);
                            strb.Append("Entry Type : " + CurrentEntry.EntryType.ToString()+Environment.NewLine);
                            strb.Append("Message :  " + CurrentEntry.Message + Environment.NewLine);
                        }

                    }
                    ev.Close();
                }
                catch(Exception ex)
                {
                    strb.Append("Unable to find logs - " + ex.Message); 
                }


                return strb.ToString(); 

            }

        }
    }
}
