using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections; 


namespace SharpMule.Automation.Framework.Essentials
{
    [Serializable]
    public class TestInterfaceEngine
    {
        ArrayList RESERVED_KEYWORDS = new ArrayList{
                                              "$GETGUID","$GETDATE"
                                          };
        public const string DEFAULT_SCRIPT_MAIN = "main.tc";
        public static Logger Log;

        TestRunner cmdlib;
        Logger.Levels loglevel;
        string script_customization_path= @"..\Properties\script.settings";
        //string script_customization_path = @"script.settings";

        public delegate void TestCaseCompletedHandler(object sender, EventArgs e);
        public event TestCaseCompletedHandler TestCaseCompleted;
        public bool IsTestCompleted { get; set; }
        public int InstanceID { get; set; }
        public string TestCasePath { get; set; }
        public string TestCaseFileName { get; set; }
        public string TestCaseName { get; set; }
        public string TestCaseParent { get; set; }
        public string TestSuiteName { get; set; }
        public bool TestCaseOutcome { get; set; }

        public TestRunner CommandLib
        {
            get { return cmdlib; }
            set { cmdlib = value; }
        }

        public TestInterfaceEngine(Logger.Levels loglvl)
        {
            loglevel = loglvl;       
        }
        public TestInterfaceEngine()
        {
            loglevel = Logger.Levels.DEBUG;
        }
        private void SetLogLevel()
        {
            switch (loglevel)
            {
                case Logger.Levels.NONE:
                    Log.Level = Logger.Levels.NONE;
                    break;
                case Logger.Levels.INFO:
                    Log.Level = Logger.Levels.INFO;
                    break;
                case Logger.Levels.WARNING:
                    Log.Level = Logger.Levels.WARNING;
                    break;
                case Logger.Levels.ERROR:
                    Log.Level = Logger.Levels.ERROR;
                    break;
                case Logger.Levels.DEBUG:
                    Log.Level = Logger.Levels.DEBUG;
                    break;
                default:
                    break;

            }
        }

        public void ScriptSetup(string path, string scriptname,string scriptcustompath)
        {
            script_customization_path = scriptcustompath; 
            ScriptSetup(path, scriptname); 

        }
        public void ScriptSetup(string path, string scriptname)
        {
            cmdlib = new TestRunner();
            Log = new Logger();

            SetLogLevel();

            CommandExceptions.Log = Log;
            cmdlib.Log = Log;


            string[] dirs = path.TrimEnd('\\').Split('\\'); 
            TestCasePath = path;
            TestCaseFileName = scriptname;
            
            
            TestSuiteName = Regex.Replace(path,@"[a-zA-Z]\:","").Replace('\\','.').Trim('.'); 
            

            if (dirs.Length >= 2)
            {
                TestCaseName = dirs[dirs.Length - 1];
                TestCaseParent = dirs[dirs.Length - 2];
            }

            try
            {
                ProcessScriptSettings();
                ProcessScript(path, scriptname);
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to Process Script. Exception: " + ex.Message); 
            }

            
            if(cmdlib.Variables.ContainsKey("$ROOT"))
                TestSuiteName = Regex.Match(TestSuiteName, cmdlib.Variables["$ROOT"] + @"(.*)").Value; 
               
                
            
           
        }


        public void ScriptExecute()
        {
            cmdlib.Execute();
            OnTestCaseCompleted(EventArgs.Empty);   
        }


        public void ScriptTeardown()
        {
            cmdlib.Teardown();  
        }


        public bool LogToDatabase(string server, string db, string query)
        {
            string sqlresp = String.Empty;

            return LogToDatabase(server, db, query, out sqlresp); 

        }
        public bool LogToDatabase(string server,string db,string query,out string sqlresp)
        {
            sqlresp = String.Empty;

            try
            {

                DataBaseTasks dbtasks = new DataBaseTasks();
                dbtasks.Connect(server, db);
                dbtasks.SendQuery(query, out sqlresp);



            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(@"C:\SharpMuleLoggingDbException.log", ex.Message);
                return false;
            }


            return true;
        }

        private void ProcessScript(string path,string script)
        {
            string content = String.Empty;
            string pattern = @"!attach\s*<(.*?)>";
            string drivepattern = @"[a-zA-Z]\:\\";
            string varpattern = @"\$[a-zA-Z0-9_]*"; 

            string p = String.Empty;
            //Get the script path and file then just strip the path from it because we need it for recursion
            if (Regex.IsMatch(script,drivepattern)){
                p = Path.GetFullPath(script);
            }
            else if(Regex.IsMatch(script,varpattern))
            {
                string key = Regex.Match(script,varpattern).Value;
                if (cmdlib.Variables.ContainsKey(key))
                {
                    path = cmdlib.Variables[key];
                }
                else
                {
                    throw new Exception("Unable to find given key in the dictionary. Key: " + key); 
                }

                script = script.Replace(key, String.Empty); 
                p = Path.GetFullPath(path+script);
            }
            else
            {
                p = Path.GetFullPath(path + script);
            }

            
           
            string f = Path.GetFileName(p);
            path = p.Replace(f, String.Empty); 

            //throw an exception if unable to find the file
            CommandExceptions.Try(() => content = File.ReadAllText(path + f), "Unable to find " + path + script);            
           
            MatchCollection scripts = Regex.Matches(content, pattern);

            //If script has content, then do foreach loop on each included file and process the content
            if (script.Length > 0)
            {
                foreach (Match sc in scripts)
                {
                    string filepattern = "<(.*?)>";
                    string file = String.Empty;

                    if (Regex.IsMatch(sc.Value, pattern))
                        file = Regex.Match(sc.Value, filepattern).Value.TrimStart('<').TrimEnd('>');
                    else
                        Log.LogDebug("<attach keyword> doesnt point to correct file location");

                    ProcessScript(path,file + ".tc");
                }
            }


            ProcessScriptContent(content);

        }

        private void ProcessScriptContent(string content)
        {

            MatchCollection variables = Regex.Matches(content, @"\$[a-zA-Z0-9_]+\s*=\s*\{([^}]*)\}");
            MatchCollection vartypetwo= Regex.Matches(content, "\\$[a-zA-Z0-9_]+\\s*=\\s*\"([^\"]*)\"");
            MatchCollection specialcontrols = Regex.Matches(content, @"\~[a-zA-Z0-9_]+\s*\(([^)]*)\)");
            List<Match> commands = GetListOfMatches(Regex.Matches(content, @"\#?\!?[a-zA-Z0-9_]+\s*\{([^}]*)\}(.*)"));
           

            if(variables.Count>0)
                ProcessVariables(variables);
            if(vartypetwo.Count > 0)
                ProcessVariables(vartypetwo);
            if(commands.Count > 0)
                ProcessCommand(commands);
            if(specialcontrols.Count > 0)
                ProcessSpecialControls(specialcontrols); 
            


        }

        private List<Match> GetListOfMatches(MatchCollection commands)
        {
            List<Match> matches = new List<Match>(); 
            foreach (Match cmd in commands)
            {
                // Don't add commented out commands
                if(!cmd.Value.StartsWith("#"))
                    matches.Add(cmd); 

            }


            return matches; 

        }
        private void ProcessSpecialControls(MatchCollection controls)
        {
            string ctrl_pattern = @"[a-zA-Z0-9_]+\s*";
            string body_pattern = @"(?<=\()(.*)(?=\))";

            string ctrl_key = String.Empty;
            string ctrl_body = String.Empty; 
            
            foreach (Match mc in controls)
            {
                if (Regex.IsMatch(mc.Value, ctrl_pattern))
                    ctrl_key = Regex.Match(mc.Value, ctrl_pattern).Value;
                else
                    Log.LogError("Invalid command found - check your comman syntex");

                if (Regex.IsMatch(mc.Value, body_pattern))
                    ctrl_body = Regex.Match(mc.Value, body_pattern).Value;
                else
                    Log.LogError("Invalid command content found - check your comman content syntex");
  

            }

        }
        private void ProcessCommand(List<Match> cmds)
        {
            string cmd_pattern = @"\!?[a-zA-Z0-9_]+\s*";
            string body_pattern = @"(?<=\{)(.*)(?=\})";
            string cmt_pattern = @"\#(.*)";
            string cmd_key = String.Empty;
            string body_value = String.Empty;
            string comment = String.Empty; 
            

            foreach (Match mc in cmds)
            {
                bool ignore = false;
                bool skip = false; 

                if (Regex.IsMatch(mc.Value, cmd_pattern))
                    cmd_key = Regex.Match(mc.Value, cmd_pattern).Value;
                else
                    Log.LogError("Invalid command found - check your comman syntex");


                if (Regex.IsMatch(mc.Value, body_pattern))
                    body_value = Regex.Match(mc.Value, body_pattern).Value;
                else
                    Log.LogError("Invalid command content found - check your comman content syntex");
  

                
                if (Regex.IsMatch(mc.Value, cmt_pattern))
                    comment = Regex.Match(mc.Value, cmt_pattern).Value.Trim('#');
                else
                    comment = cmd_key.ToUpper(); 
                
                if(Regex.IsMatch(mc.Value,"@Ignore"))
                    ignore = true; 
                if(Regex.IsMatch(mc.Value,"@Skip"))
                    skip = true; 

                cmdlib.Commands.Enqueue(new Command(cmd_key, body_value,comment,ignore,skip)); 

            }
          

        }


        private void ProcessVariables(MatchCollection vars)
        {
            string var_pattern = @"\$[a-zA-Z0-9_]+";
            string body_pattern = @"(?<=\{)([^}]*)(?=\})"; //used to be -> \{([^}]*)\}
            string var_key = String.Empty;
            string body_value = String.Empty;

            foreach (Match v in vars)
            {
                if (Regex.IsMatch(v.Value, var_pattern))
                    var_key = Regex.Match(v.Value, var_pattern).Value;
                else
                    Log.LogDebug("Invalid variable found - check your variable syntex");

                if (Regex.IsMatch(v.Value, body_pattern))
                    body_value = Regex.Match(v.Value, body_pattern).Value;
                else
                    Log.LogDebug("Invalid variable content found - check your variable content syntex");

                string keyword = Regex.Match(body_value, @"\$[a-zA-Z1-9]+").ToString();  // Get just the keyword
                if (RESERVED_KEYWORDS.Contains(keyword))
                {
                    body_value = ProcessReservedVariables(body_value); 
                }

                if (!cmdlib.Variables.ContainsKey(var_key))
                    cmdlib.Variables.Add(var_key, body_value);
                else
                    cmdlib.Variables[var_key]=body_value;
            }

        }

        private string ProcessReservedVariables(string var)
        {
            string keyword = Regex.Match(var, @"\$[a-zA-Z1-9]+").ToString(); 

            switch (keyword)
            {
                case "$GETGUID":
                    return Guid.NewGuid().ToString(); 
                //case "$RANDNUM":
                //case "$RANDSTR":
                case "$GETDATE":
                    string[] args = Regex.Match(var,@"(?<=\()\d+\,(.*?)(?=\))").ToString().Split(',');
                    return cmdlib.GetDate(args[0], args[1]);
                default:
                    Log.LogDebug("Not sure how it got here, check your code [V0020]"); 
                    break; 

            }

            return String.Empty; 

        }

        private void FinalizeVariables()
        {
            string var_pattern = @"\$[a-zA-Z0-9_]+";
            Dictionary<string, string> tempdict = new Dictionary<string, string>();

            foreach (string key in cmdlib.Variables.Keys)
            {
                MatchCollection matches = Regex.Matches(cmdlib.Variables[key], var_pattern);
                string currvalue = cmdlib.Variables[key];

                if (matches.Count > 0)
                {
                    foreach (Match m in matches)
                    {
                        string foundVar = m.Value.Trim();
                        if (cmdlib.Variables.ContainsKey(foundVar))
                        {
                            string foundvarvalue = cmdlib.Variables[foundVar];
                            int index = currvalue.IndexOf(foundVar, 0, currvalue.Length);
                            currvalue = currvalue.Remove(index, foundVar.Length);
                            currvalue = currvalue.Insert(index, foundvarvalue);

                        }
                        else
                        {
                           // Log.LogDebug("Variable found in the value body var="+foundVar+" is not defined anywhere [V0021]"); 
                        }

                        if (!tempdict.ContainsKey(key))
                            tempdict.Add(key, currvalue);
                        else
                            tempdict[key] = currvalue;
                    }
             
                }

               

            }

            foreach (string key in tempdict.Keys)
            {
                cmdlib.Variables[key] = tempdict[key];
            }

        }

        public void ProcessScriptSettings()
        {

            string path = Path.GetFullPath(script_customization_path);

            if (File.Exists(path))
            {
               string pattern = @"\$[a-zA-Z0-9_]+\s*=\s*(.*?)\r\n";

               string settings = File.ReadAllText(path);

               MatchCollection matches = Regex.Matches(settings, pattern);

               foreach (Match mc in matches)
               {
                   string[] key_value_par = mc.Value.Split('=');
                   string key = key_value_par[0].Trim();
                   string value = key_value_par[1].Trim();

                   if (cmdlib.Variables.ContainsKey(key))
                       cmdlib.Variables[key] = value;
                   else
                       cmdlib.Variables.Add(key, value); 

               }

               Console.WriteLine();

            }

        }
        public virtual void OnTestCaseCompleted(EventArgs e)
        {
            if (TestCaseCompleted != null)
                TestCaseCompleted(this, e);
        }


    }
}
