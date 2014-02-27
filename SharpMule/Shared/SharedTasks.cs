using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Windows.Controls;
using TestManager;
using TestManager.TreeViewCtrl;
using TestManager.DataGridCtrl;
using SharpMule.Automation.Framework.Essentials;



namespace TestManager.Shared
{
    public class SharedTasks
    {
        public static TestInterfaceEngine TaskLibInstance { get; set; }
        public static List<TestInterfaceEngine> TestSuitInstances = new List<TestInterfaceEngine>();
        public static List<TestCaseViewer> TestCaseViewerInstances = new List<TestCaseViewer>();
       

        public static string CurrentTestPath { get; set; }
        public static string CurrentTestTitle { get; set; }


        public static void SetNewInstance()
        {
            CurrentTestPath = TreeViewCtrl.TreeViewTasks.TestPath;
            TaskLibInstance = new TestInterfaceEngine();
            TaskLibInstance.ScriptSetup(CurrentTestPath, @"\main.tc");

        }
        public static void SetNewInstance(string testCasePath)
        {
            TaskLibInstance = new TestInterfaceEngine();
            TaskLibInstance.ScriptSetup(testCasePath, @"\main.tc");

        }
        public static void SetNewInstance(TestCaseViewer tcvInstance, string testCasePath, int instanceID)
        {
            TaskLibInstance = new TestInterfaceEngine();
            TaskLibInstance.InstanceID = instanceID;
            TaskLibInstance.ScriptSetup(testCasePath, @"\main.tc");

            TestSuitInstances.Add(TaskLibInstance);
            TestCaseViewerInstances.Add(tcvInstance);
            tcvInstance.lblTestName.Content = TaskLibInstance.TestCaseName; 
           
        }

       

        public static int GetFailedTestCaseCount()
        {
            int count = 0;
            foreach (TestInterfaceEngine ts in TestSuitInstances)
            {
                if (!GetTestCaseResult(ts))
                    count++;
            }

            return count;

        }
        public static List<TestInterfaceEngine> GetFailedTestCaseInstances()
        {
            List<TestInterfaceEngine> failedList = new List<TestInterfaceEngine>();
            foreach (TestInterfaceEngine ts in TestSuitInstances)
            {
                if (!GetTestCaseResult(ts))
                {
                    TestInterfaceEngine newts = new TestInterfaceEngine();
                    newts.ScriptSetup(ts.TestCasePath, ts.TestCaseFileName);
                    newts.InstanceID = ts.InstanceID; 
                    failedList.Add(newts);
                }

            }

            return failedList;


           
        }
        public static string GetFailedTestStepLog(TestInterfaceEngine test)
        {
            foreach (Command cmd in test.CommandLib.Commands)
            {
                DebugLogger log = new DebugLogger();
                test.CommandLib.Log.LogStorage.TryGetValue(cmd, out log);
                
                if (log == null)
                {
                    return "This step has failed due to an uknown issue. Nothing was logged. UI has forced this log message";

                }
                else
                    if (!log.IsTestPass)
                        return cmd.Desc+"{"+log.Debug+"}";                  

            }

            return String.Empty; 
        }
        public static bool GetTestCaseResult(TestInterfaceEngine testCaseInstance)
        {
            int steps = 0;

            foreach (Command cmd in testCaseInstance.CommandLib.Commands)
            {
                DebugLogger log = new DebugLogger();
                testCaseInstance.CommandLib.Log.LogStorage.TryGetValue(cmd, out log);
                steps ++; 
                if (log == null)
                {
                    log = new DebugLogger();
                    log.Debug = "This step has failed due to an uknown issue. Nothing was logged. UI has forced this log message";
                    testCaseInstance.CommandLib.Log.LogStorage[cmd] = log;
                    return false; 
                }
                else
                    if (!log.IsTestPass)
                        return false;


            }

            if (steps != testCaseInstance.CommandLib.Commands.Count)
                return false; 

            return true;
        }

        public static void SetTestCaseTitle()
        {
            if (CurrentTestPath != null)
            {
                string[] folders = CurrentTestPath.Split('\\');
                string testname = folders[folders.Length - 2];
                CurrentTestTitle = testname;
            }
        }


        public static void ResetLog(TestInterfaceEngine tcLib)
        {
            tcLib.CommandLib.Log.LogStorage.Clear();
        }
        public static void ResetLogs()
        {
            foreach (TestInterfaceEngine tcLib in TestSuitInstances)
            {
                tcLib.CommandLib.Log.LogStorage.Clear();
            }
        }
        public static void ResetAllInstances()
        {
            TestSuitInstances.Clear();
            TestCaseViewerInstances.Clear();
        }

        public static bool LoadAllProjects(XmlDocument xml)
        {
            foreach (XmlNode project in xml.SelectNodes("/configuration/projects/project"))
            {
                try
                {
                    Projects.Project newproject = new Projects.Project(project["path"].InnerXml);
                    Projects.ProjectList.Add(newproject);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public static void GetEnvironmentByName(string name, out Environments.Environment env)
        {
            env = null;
            foreach (Environments.Environment e in Environments.EnvironmentList)
            {
                if (e.EnvName.ToLower().Equals(name.ToLower()))
                    env = e;
            }

        }

        public static bool LoadDbLoggingConfig(XmlDocument xml)
        {
            try
            {
                DataBaseLogging.IsEnabled = Convert.ToBoolean(xml.SelectSingleNode("/configuration/logging/isenabled").InnerXml);
                DataBaseLogging.DbServer = xml.SelectSingleNode("/configuration/logging/dbserver").InnerXml;
                DataBaseLogging.DbName = xml.SelectSingleNode("/configuration/logging/dbname").InnerXml;
               

            }
            catch
            {
                MainWindow.MessageBx("Unable to read db configuration");
                return false;
            }

            return true; 

        }
        public static bool LoadEmailConfig(XmlDocument xml)
        {
            try
            {
                Email.SmtpHost = xml.SelectSingleNode("/configuration/email/smtphost").InnerXml;
                Email.EmailAddress = xml.SelectSingleNode("/configuration/email/recipient").InnerXml;
                Email.IsEnabled = Convert.ToBoolean(xml.SelectSingleNode("/configuration/email/isenabled").InnerXml);
                
            }
            catch
            {
                MainWindow.MessageBx("Unable to read email configuration");
                return false; 
            }

            return true; 
        }
        public static bool LoadAllEnvironments(XmlDocument xml)
        {
            string _ename = String.Empty;
            int _epindex = 0;

            try
            {
                // Set default environment and settigns file
                Environments.Settings = xml.SelectSingleNode("/configuration/environments/settings");
                Environments.DefaultEnvironment = xml.SelectSingleNode("/configuration/environments/defaultenvironment").InnerXml;


                // Get all of the environment endpoints
                foreach (XmlNode env in xml.SelectNodes("/configuration/environments/environment"))
                {
                    List<string> _elist = new List<string>();
                    _ename = env["name"].InnerXml;
                    _epindex = Convert.ToInt32(env["defaultepindex"].InnerXml);


                    foreach (XmlNode ep in env["endpointlist"].ChildNodes)
                    {
                        _elist.Add(ep.InnerXml);
                    }

                    Environments.Environment newenv = new Environments.Environment(_ename, _elist, _epindex);
                    newenv.EnvironmentNode = env;
                    Environments.EnvironmentList.Add(newenv);


                }


            }
            catch (Exception ex)
            {
                MainWindow.MessageBx("Unable to load Environments - " + ex.Message);
                return false;
            }

            return true;
        }

        private static List<Environments.TPID> LoadEnvironmentTpids(XmlDocument xml, Environments.Environment env)
        {
            List<Environments.TPID> tpids = new List<Environments.TPID>();


            foreach (XmlNode node in xml.SelectNodes("/tpids/" + env.EnvName.ToLower() + "/tpid"))
            {
                string name = node["name"].InnerXml;
                string id = node["id"].InnerXml;
                List<string> users = node["users"].InnerXml.Split(',').ToList();

                Environments.TPID tpid = new Environments.TPID(name, id, users);
                tpids.Add(tpid);
            }



            return tpids;
        }


        public static bool LoadAllConnections(XmlDocument xml)
        {

            try
            {
                Connections.ActiveConnection = String.Empty; 
                foreach (XmlNode node in xml.SelectNodes("/configuration/connections/connection"))
                {
                    Connections.Connection connection = new Connections.Connection(); 
                    connection.Name = node["name"].InnerXml;
                    connection.Host = node["host"].InnerXml; 
                    connection.Port = node["port"].InnerXml;
                    connection.Env = node["envmap"].InnerXml; 

                    Connections.ConnectionList.Add(connection); 

                }
            }
            catch(Exception ex)
            {
                MainWindow.MessageBx("Unable to load Connections - " + ex.Message);
                return false;
            }

            return true; 
            
        }

        public static bool LoadAllTools(XmlDocument xml)
        {
            try
            {
                foreach(XmlNode node in xml.SelectNodes("/configuration/tools/tool"))
                {
                    string toolName = node["name"].InnerXml;
                    string toolPath = node["path"].InnerXml; 
                    Tools.Tool tool = new Tools.Tool(toolName,toolPath);
                    Tools.ToolsList.Add(tool); 

                }
            }
            catch (Exception ex)
            {
                MainWindow.MessageBx("Unable to load tools - " + ex.Message);
                return false;
            }


            return true; 

        }

        public class Script
        {
            public static string Root { get; set; }
        }

        public class Tools
        {
            public static List<Tool> ToolsList = new List<Tool>(); 

            public class Tool
            {
                public string ToolName { get; set; }
                public string ToolPath { get; set; }

                public Tool(string toolName, string toolPath)
                {
                    ToolName = toolName;
                    ToolPath = toolPath; 
                }

            }

        }

        public class Connections
        {
            public static string ActiveConnection { get; set; }
            public static int ActivePort { get; set; }
            public static List<Connection> ConnectionList = new List<Connection>();



            public static Connection GetConnectionByName(string name)
            {
                foreach (Connection connection in ConnectionList)
                {
                    if (connection.Name.Equals(name))
                        return connection; 
                }

                return null; 

            }


            public static Connection GetConnectionByEnvMap(string envmap)
            {
                foreach (Connection connection in ConnectionList)
                {
                    if (connection.Env.Equals(envmap))
                        return connection;
                }

                return null; 

            }

            public class Connection
            {
                public string Name { get; set; }
                public string Host { get; set; }
                public string Port { get; set; }
                public string Env { get; set; }

            }

        }

        public class Editor
        {
            public class External
            {
                public static string Name { get; set; }
                public static string Path { get; set; }
            }
            public class Internal
            {
                public static string Text { get; set; }
                public static string FilePath { get; set; }
                public static bool TextChanged { get; set; }
            }
        }


        public class DataBaseLogging
        {
            public static bool IsEnabled { get; set; }
            public static string DbServer { get; set; }
            public static string DbName { get; set; }
            
            public static string ConnectionString
            {
                get
                {
                    return "Data Source= " + DbServer + ";Initial Catalog= " + DbName + ";Integrated Security=true";
                }

            }


            public static bool LogToDatabase(TestInterfaceEngine instance)
            {

                if (!DataBaseTasks.Static.IsConnected)
                    DataBaseTasks.Static.Connect(DataBaseLogging.DbServer, DataBaseLogging.DbName); 


                Guid guid = Guid.NewGuid();
                int index = 0; 
                bool isDbCommitSuccessful;
                string testcasequery = @"   declare @UniqueKey as uniqueidentifier = '" + guid + @"'
                                declare @SuiteName as varchar(255) = '" + instance.TestSuiteName + @"'
                                declare @TestName as varchar(255) = '" + instance.TestCaseName + @"'
                                declare @TestStatus as bit = " + (instance.CommandLib.Status == true ? 0 : 1) + @"
                                declare @CreateDate as DateTime =  Current_Timestamp 
                                declare @Tester as varchar(32) = '" + Environment.UserName + @"'
                                declare @TestEnv as varchar(255) = '"+Environments.DefaultEnvironment+@"'
                                declare @TestEP as varchar(255) = '"+Environments.GetDefaultEnvironment().DefaultEndpoint+@"'

                                INSERT INTO [TripTest].[dbo].[TestCaseLogging]
                                           ([UniqueKey]
                                           ,[SuiteName]
                                           ,[TestName]
                                           ,[TestStatus]
                                           ,[CreateDate]
                                           ,[Tester]
                                           ,[TestEnvironment]
                                           ,[TestEndPoint])
                                     VALUES
                                           (@UniqueKey
                                           ,@SuiteName
                                           ,@TestName
                                           ,@TestStatus
                                           ,@CreateDate
                                           ,@Tester
                                           ,@TestEnv
                                           ,@TestEP)
                                ";

                string keptrack = String.Empty;
                string response = String.Empty;

                isDbCommitSuccessful = DataBaseTasks.Static.SendQuery(testcasequery,out response); 


                Logger Log = instance.CommandLib.Log;
                foreach (Command cmd in Log.LogStorage.Keys)
                {

                    string info = String.Empty;
                    string error = String.Empty;
                    string warn = String.Empty;
                    string debug = String.Empty;
                    string bit = String.Empty;

                    try
                    {
                        
                        info = Log.LogStorage[cmd].Info == null ? "" : "INFO: " + Log.LogStorage[cmd].Info;
                        error = Log.LogStorage[cmd].Error == null ? "" : "ERROR: " + Log.LogStorage[cmd].Error;
                        warn = Log.LogStorage[cmd].Warning == null ? "" : "WARN: " + Log.LogStorage[cmd].Warning;
                        debug = Log.LogStorage[cmd].Debug == null ? "" : "DEBUG: " + Log.LogStorage[cmd].Debug;
                        bit = Log.LogStorage[cmd].IsTestPass == true ? "0" : "1";

                        

                        string steplog = (info + error + warn + debug).Replace('\'', ' ');
                        string steplogquery = @"
                                    INSERT INTO [TripTest].[dbo].[TestCaseStepLogging]
                                   ([TestCaseIdentifier]
                                   ,[TestCaseStep]
                                   ,[TestCaseStepLog]
                                   ,[TestCaseStepResult]
                                   ,[TestCaseStepIndex]
                                   ,[CreateDate])
                                    VALUES
                                   ('" + guid + @"'
                                   ,'"+cmd.Desc+@"'
                                   ,'" + steplog + @"'
                                   ," + bit + @"
                                   ,"+(index++)+@"
                                   ,CURRENT_TIMESTAMP)";



                        if (isDbCommitSuccessful)
                            isDbCommitSuccessful = DataBaseTasks.Static.SendQuery(steplogquery,out response); 


                    }
                    catch(Exception ex)
                    {
                        System.IO.File.AppendAllText(@"C:\SharpMuleLoggingDbException.log", ex.Message + Environment.NewLine);
                    }


                }


                return isDbCommitSuccessful;
            }

        }

      


        public class Email
        {
            public static string SmtpHost { get; set; }
            public static string EmailAddress { get; set; }
            public static bool IsEnabled { get; set; }
        }
        public class Projects
        {
            static List<Project> projects = new List<Project>();
            public static List<Project> ProjectList
            {
                get
                {
                    return projects;
                }
                set
                {
                    projects = value;
                }
            }
            public class Project
            {
                public Project(string path)
                {
                    Path = path;
                }
                public string Path { get; set; }
            }

        }

        public class Environments
        {
            static List<TPID> tpids = new List<TPID>();
            static List<Environment> environments = new List<Environment>();
            public static XmlNode Settings { get; set; }
            public static XmlDocument TpidsSettings { get; set; }
            public static string DefaultEnvironment { get; set; }
            public static string HotelVersion { get; set; }

            public static List<Environment> EnvironmentList
            {
                get
                {
                    return environments;
                }
                set
                {
                    environments = value;
                }
            }

            public static bool IsRemoteEnvironment()
            {
                if (DefaultEnvironment.Equals("PPE") || DefaultEnvironment.Equals("PROD"))
                    return true;
                else
                    return false; 

            }

            public static Environment GetEnvironmentByName(string name)
            {
                foreach (Environment env in EnvironmentList)
                {
                    if (env.EnvName.Equals(name.ToUpper()))
                        return env;
                }

                return null;
            }

            public static Environment GetDefaultEnvironment()
            {
                foreach (Environment env in EnvironmentList)
                {
                    if (env.EnvName.Equals(DefaultEnvironment.ToUpper()))
                        return env;
                }

                return null;
            }


            public class Environment
            {

                public Environment(string envname, List<string> endpoints, int defaultendpointindex)
                {
                    EnvName = envname;
                    EndPoints = endpoints;
                    DefaultEndpointIndex = defaultendpointindex;
                }

                public List<string> EndPoints { get; set; }
                public string EnvName { get; set; }
                public int DefaultEndpointIndex { get; set; }
                public string DefaultUser { get; set; }
                public XmlNode EnvironmentNode { get; set; }
                public string DefaultEndpoint
                {
                    get
                    {
                        return EndPoints[DefaultEndpointIndex]; 
                    }
                }
            }

            public class TPID
            {
                string name;

                public string Name
                {
                    get { return name; }
                }
                string id;

                public string Id
                {
                    get { return id; }
                }
                List<string> users;

                public List<string> Users
                {
                    get { return users; }
                }

                public TPID(string name, string id, List<string> users)
                {
                    this.name = name;
                    this.id = id;
                    this.users = users;
                }

            }
        }




    }
}
