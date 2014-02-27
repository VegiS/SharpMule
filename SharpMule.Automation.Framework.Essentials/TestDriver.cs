using System;
using System.Reflection; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace SharpMule.Automation.Framework.Essentials
{
    [Serializable]
    public class TestRunner:TaskRepository
    {
        public void Execute()
        {
            foreach (Command cmd in commands)
            {

     
                if (!cmd.Proc.Equals("loop"))
                    cmd.Param = ConvertParamterVariables(cmd.Param);
                else
                    cmd.Param = cmd.Param.Replace(',', ':');


                TestUtilities.Timer ActionsTimer = new TestUtilities.Timer();
                ActionsTimer.Start(); 
                bool result = DoCommand(cmd, GetParameters(cmd.Param));
                ActionsTimer.Stop();
                Log.LogElapsedTime(Convert.ToInt32(ActionsTimer.GetElapsedTime()).ToString()); 


                if (cmd.Proc.StartsWith("!") && !result)
                {
                    Status = false;
                    Log.LogFail(cmd.Desc);
                    Log.FillLogBuffer(cmd);
                    break;
                }

                else if (!result)
                {
                    Status = false;
                    Log.LogFail(cmd.Desc);

                }
                else
                {
                    Log.LogPass(cmd.Desc);
                }

                Log.FillLogBuffer(cmd);
            }



        }

        public void Teardown()
        {
            // Left to implement if needed  
        }

    }

    [Serializable]
    public partial class TaskRepository
    {
            private bool status = true;
            public bool Status { get { return status; } set { status = value; } }
            public Logger Log { get; set; }

            public Dictionary<string, Func<ArrayList, bool>> methods = new Dictionary<string, Func<ArrayList, bool>>();
            
            // Keeps the queue of commands
            public Queue<Command> commands = new Queue<Command>();
            public Queue<Command> Commands
            {
                get { return commands; }
                set { commands = value; }
            }
 
            // Keeps the dictionary of all veriables and their values
            // Example variable $httpendpoint and value [webserviceurl]
            private Dictionary<string, string> variables = new Dictionary<string, string>();
            public Dictionary<string, string> Variables
            {
                get { return variables; }
                set { variables = value; }
            }

            private string responseBody = String.Empty;

            public string Response
            {
                get
                {
                    return responseBody;
                }

            }

            // After each call, response body will be stored internally so you can verify against
            public string ResponseBody
            {
                get { return responseBody; }
                set { responseBody = value; }
            }

            /* Take the paramters string from the input script 
             * Example (Example: httprequest{post,endpoint,body,headers} it will take
             * post,endpoint,body,headers and split it by , then add to dictionary of available variable
             * as well as return the array of parameters back
            */
            public string[] GetParameters(string paramstr)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();


                string[] parameters = paramstr.Split(',');

                for (int i = 0; i < parameters.Length; i++)
                {
                    string key = parameters[i];
                    if (dict.ContainsKey(key))
                        parameters[i] = dict[key].Trim(new char[] { '%' });

                }


                return parameters;
            }

            /*
             *  DoCommand is responsible of mapping and executing the correct methods
             *  It parses your script command (Example: httprequest{post,endpoint,body,headers}
             *  It takes two parameters. 1st parameter would be your command like "httprquest"
             *  while second command would be everything inside of your command {post,endpoint,body,headers}
             *  It uses first paramter and using reflection its invoking that method by passing in the rest of the paramters
             * 
            */
            public bool DoCommand(Command cmd, string[] parameters)
            {
                try
                {
                    string filteredCommand = cmd.Proc.Trim().TrimStart('!').ToLower();
                    MethodInfo actionMethod = typeof(TaskRepository).GetMethod(filteredCommand, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    return Convert.ToBoolean(actionMethod.Invoke(this, new object[] { parameters }));
                }
                catch
                {
                    Log.LogDebug("It looks like command you have entered is not defined. Refer to useage guide for available commands!");
                    return false; 
                }
            
            }

            

        }

}
