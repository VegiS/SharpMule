using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMule.Automation.Framework.Essentials
{
    [Serializable]
    public class DebugLogger
    {
        public bool IsTestPass { get; set; }
        public string Warning { get; set; }
        public string Error { get; set; }
        public string Debug { get; set; }
        public string Info { get; set; }
        public string TimeElappsed { get; set; }
      
    }

    // A delegate type for hooking up added log notifications.
    public delegate void AddedLogEventHandler(object sender, EventArgs e);

    [Serializable]
    public class Logger
    {
        public enum Levels { NONE,INFO,WARNING,ERROR, DEBUG }
        public Levels level = Levels.NONE;
        public Dictionary<Command, DebugLogger> LogStorage;
        DebugLogger logKeeper;
        DateTime TimeStamp;
        public event AddedLogEventHandler NewLogEntry;



        public Logger()
        {
            TimeStamp = DateTime.Now; 
            logKeeper = new DebugLogger();
            this.LogStorage = new Dictionary<Command, DebugLogger>();
            
        }



        // Invoke the Changed event; called whenever list changes
        protected virtual void OnLogAdded(EventArgs e)
        {
            if (NewLogEntry != null)
                NewLogEntry(this, e);
        }

        public  void FillLogBuffer(Command cmd)
        {
            // Add Time Stamp to beginning of each log category
            
            logKeeper.Debug = logKeeper.Debug;
            logKeeper.Error = logKeeper.Error;
            logKeeper.Warning = logKeeper.Warning;
            logKeeper.Info = TimeStamp.ToString(); 

            if (LogStorage.ContainsKey(cmd))
                LogStorage[cmd] = logKeeper; 
            else
                LogStorage.Add(cmd, logKeeper);

            OnLogAdded(EventArgs.Empty); 
            logKeeper = new DebugLogger(); 

        }

        public Levels Level
        {
            get { return level; }
            set { level = value; }
        }

        //public bool LogToDatabase(string dbserver, string dbname,string newquery)
        //{
        //    string sqlresp = String.Empty;


        //    return LogToDatabase(dbserver,dbname,newquery,out sqlresp);
        //}

        //public bool LogToDatabase(string dbserver, string dbname, string newquery, out string sqlresp)
        //{
        //    sqlresp = String.Empty;


        //    try
        //    {
                
        //        DataBaseTasks dbtasks = new DataBaseTasks();
        //        dbtasks.Connect(dbserver, dbname);
        //        dbtasks.SendQuery(newquery, out sqlresp);

                

        //    }
        //    catch (Exception ex)
        //    {
        //        System.IO.File.WriteAllText(@"C:\SharpMuleLoggingDbException.log", ex.Message); 
        //        return false;
        //    }


        //    return true;
        //}


        public void LogClear()
        {
            logKeeper.Debug = String.Empty;
            logKeeper.Error = String.Empty;
            logKeeper.Warning = String.Empty; 
        }

        public void LogPass(string info)
        {
            logKeeper.IsTestPass = true; 
            switch (level)
            {
                case Levels.DEBUG:
                case Levels.ERROR:
                case Levels.WARNING:
                case Levels.INFO:
                case Levels.NONE:
                    SetPrefixColor("[Pass]", info, ConsoleColor.Green);
                    break;
                default:
                    break;
            }

        }

        public void LogFail(string info)
        {
            logKeeper.IsTestPass = false; 
            switch (level)
            {
                case Levels.DEBUG:
                case Levels.ERROR:
                case Levels.WARNING:
                case Levels.INFO:
                case Levels.NONE:
                    SetPrefixColor("[Fail]", info, ConsoleColor.Red);
                    break;
                default:
                    break;
            }
        }
       
        public void LogInfo(string info)
        {
            switch (level)
            {
                case Levels.INFO:
                    SetPrefixColor("[INFO]", info, ConsoleColor.White);
                    break;
                default:
                    break;
            }
        }

        public void LogWarning(string info)
        {
            TimeStamp = DateTime.Now;
            logKeeper.IsTestPass = true;
            logKeeper.Warning += info.Trim() + Environment.NewLine;

            switch (level)
            {
                case Levels.DEBUG:
                case Levels.WARNING:
                    SetPrefixColor("[Warning]", info, ConsoleColor.Yellow);
                    break;
                default:
                    break; 
            }

                
        }

        public void LogError(string info)
        {
            TimeStamp = DateTime.Now;
            logKeeper.IsTestPass = false;
            logKeeper.Error += info.Trim() + Environment.NewLine;

            switch (level)
            {
                case Levels.DEBUG:
                case Levels.WARNING:
                case Levels.ERROR:
                    SetPrefixColor("[Error]", info, ConsoleColor.DarkRed);
                    break;
                default:
                    break;
            }
                
        }
        public void LogDebug(string info)
        {
            TimeStamp = DateTime.Now;
            logKeeper.Debug += info.Trim()+Environment.NewLine;


            switch (level)
            {
                case Levels.DEBUG:
                    SetPrefixColor("[Debug]",info,ConsoleColor.Cyan);
                    break;
                default:
                    break;
            }           
        }

        public void LogElapsedTime(string etime)
        {
            logKeeper.TimeElappsed = etime; 
        }

        private void SetPrefixColor(string prefix,string info, ConsoleColor color)
        {
            //Console.Write("[" + TimeStamp + "]");
            Console.ForegroundColor = color; 
            Console.Write(prefix);
            Console.ResetColor();
            Console.WriteLine(info); 
        }
    }
}
