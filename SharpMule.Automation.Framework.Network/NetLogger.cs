using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpMule.Automation.Framework.Network
{
    [Serializable]
    public class NetLogger
    {
        public enum Levels { NONE, INFO, ALL }
        DateTime TimeStamp;
        DateTime LogStamp; 
        Levels level = Levels.NONE;
        //StreamWriter sw;
        const string FORMAT = "MMM-ddd-d.HH-mm-yyyy";

        public Levels Level
        {
            get { return level; }
            set { level = value; }
        }
        

        public NetLogger()
        {
            TimeStamp = DateTime.Now;
            LogStamp = DateTime.Now; 
            level = Levels.ALL;
            try
            {
                //Directory.CreateDirectory("Log");
                //sw = File.AppendText("Log/Log" + TimeStamp.ToString(FORMAT) + ".txt");
                //sw.AutoFlush = true;
            }
            catch
            {

            }
        }
        public void LogError(string info)
        {        

            switch (level)
            {
                case Levels.ALL:
                case Levels.INFO:
                    string logmsg = "[" + TimeStamp + "] - " + info;
                    //sw.WriteLine(logmsg);
                    Console.Write(logmsg +Environment.NewLine);
                    break;
                default:
                    break;
            }

        }

        public void LogInfo(string info)
        {
            switch (level)
            {
                case Levels.ALL:
                case Levels.INFO:
                    string logmsg = "[" + TimeStamp + "] - " + info;
                    //sw.WriteLine(logmsg);
                    Console.Write(logmsg +Environment.NewLine);
                    break;
                default:
                    break;
            }
        }

    }
}
