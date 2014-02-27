using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Threading;
using Renci.SshNet;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Net;

namespace SharpMule.Automation.Framework.Essentials
{
    public class TestUtilities
    {
        public class Timer
        {
            private static DateTime startTime;
            private static DateTime stopTime;
            private static bool running = false;


            public void Start()
            {
                startTime = DateTime.Now;
                running = true;
            }


            public void Stop()
            {
                stopTime = DateTime.Now;
                running = false;
            }


            // elaspsed time in milliseconds
            public double GetElapsedTime()
            {
                TimeSpan interval;

                if (running)
                    interval = DateTime.Now - startTime;
                else
                    interval = stopTime - startTime;

                return interval.TotalMilliseconds;
            }


            // elaspsed time in seconds
            public double GetElapsedTimeSecs()
            {
                TimeSpan interval;

                if (running)
                    interval = DateTime.Now - startTime;
                else
                    interval = stopTime - startTime;

                return interval.TotalSeconds;
            }

        }
        public static class RemoteOperations
        {
            public class SSH
            {
                SshClient sshClient;
                public bool Connect(string host, string username, string password)
                {
                    try
                    {
                        sshClient = new SshClient(host, username, password);
                        sshClient.Connect();
                        sshClient.KeepAliveInterval = new TimeSpan(5000);
                        sshClient.SendKeepAlive();

                        if (!sshClient.IsConnected)
                            return false;

                    }
                    catch (Exception ex)
                    {
                        TestInterfaceEngine.Log.LogError(ex.Message);
                        return false;
                    }

                    return true;
                }

                public bool SendCommand(string command, int timeoutInSeconds,out string result)
                {
                    result = String.Empty;
                    try
                    {

                        SshCommand cmd = sshClient.CreateCommand(command);
                        cmd.CommandTimeout = new TimeSpan(0, 0, timeoutInSeconds);
                        result = cmd.Execute();
                    }
                    catch (Exception ex)
                    {
                        try { TestInterfaceEngine.Log.LogError(ex.Message); }
                        catch { return false; }
                        return false;
                    }

                    return true;
                }
                public bool SendCommand(string command, out string result)
                {
                    result = String.Empty;
                    try
                    {
                        
                        SshCommand cmd = sshClient.CreateCommand(command);
                        cmd.CommandTimeout = new TimeSpan(0,0,60); 
                        result = cmd.Execute();
                    }
                    catch (Exception ex)
                    {
                        try { TestInterfaceEngine.Log.LogDebug(ex.Message); }
                        catch { return false; }
                        return false;
                    }

                    return true;
                }
                public void Disconnect()
                {
                    sshClient.Disconnect(); 
                }
            }

        }
        public class Algorithams
        {
            
            public static class LevenshteinDistance
            {
                /// <summary>
                /// Compute the distance between two strings.
                /// The Levenshtein distance between two strings is defined as the minimum number of edits needed to 
                /// transform one string into the other, with the allowable edit operations being 
                /// insertion, deletion, or substitution of a single character.
                /// </summary>
                public static int Compute(String sRow, String sCol)
                {
                    int RowLen = sRow.Length;  // length of sRow
                    int ColLen = sCol.Length;  // length of sCol
                    int RowIdx;                // iterates through sRow
                    int ColIdx;                // iterates through sCol
                    char Row_i;                // ith character of sRow
                    char Col_j;                // jth character of sCol
                    int cost;                   // cost

                    /// Test string length
                    if (Math.Max(sRow.Length, sCol.Length) > Math.Pow(2, 31))
                        throw (new Exception("\nMaximum string length in Levenshtein.iLD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(sRow.Length, sCol.Length) + "."));

                    // Step 1

                    if (RowLen == 0)
                    {
                        return ColLen;
                    }

                    if (ColLen == 0)
                    {
                        return RowLen;
                    }

                    /// Create the two vectors
                    int[] v0 = new int[RowLen + 1];
                    int[] v1 = new int[RowLen + 1];
                    int[] vTmp;



                    /// Step 2
                    /// Initialize the first vector
                    for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                    {
                        v0[RowIdx] = RowIdx;
                    }

                    // Step 3

                    /// Fore each column
                    for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
                    {
                        /// Set the 0'th element to the column number
                        v1[0] = ColIdx;

                        Col_j = sCol[ColIdx - 1];


                        // Step 4

                        /// Fore each row
                        for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                        {
                            Row_i = sRow[RowIdx - 1];


                            // Step 5

                            if (Row_i == Col_j)
                            {
                                cost = 0;
                            }
                            else
                            {
                                cost = 1;
                            }

                            // Step 6

                            /// Find minimum
                            int m_min = v0[RowIdx] + 1;
                            int b = v1[RowIdx - 1] + 1;
                            int c = v0[RowIdx - 1] + cost;

                            if (b < m_min)
                            {
                                m_min = b;
                            }
                            if (c < m_min)
                            {
                                m_min = c;
                            }

                            v1[RowIdx] = m_min;
                        }

                        /// Swap the vectors
                        vTmp = v0;
                        v0 = v1;
                        v1 = vTmp;

                    }


                    // Step 7

                    /// Value between 0 - 100
                    /// 0==perfect match 100==totaly different
                    /// 
                    /// The vectors where swaped one last time at the end of the last loop,
                    /// that is why the result is now in v0 rather than in v1
                    System.Console.WriteLine("iDist=" + v0[RowLen]);
                    int max = System.Math.Max(RowLen, ColLen);
                    return ((100 * v0[RowLen]) / max);
                }
            }

        }
        public class Common
        {
            public static string GetResponseNode(TaskRepository cmdlib, string localname)
            {
                string message = cmdlib.LookUpResponse(localname);


                return message;

            }
            public static bool FindTestByName(string testRoot, string testName, out string testPath)
            {
                testPath = String.Empty;

                try
                {
                    string[] tests = Directory.GetDirectories(testRoot, testName, SearchOption.AllDirectories);
               
                    if (tests.Length > 0)
                    {
                        testPath = tests[0];

                        return true;
                    }
                }
                catch
                {
                    return false;
                }


                return false;
            }

            public static List<TestInterfaceEngine> GetAllTestObjects(string path)
            {
                List<TestInterfaceEngine> testObjects = new List<TestInterfaceEngine>();

                string[] filePaths = Directory.GetFiles(path, TestInterfaceEngine.DEFAULT_SCRIPT_MAIN,SearchOption.AllDirectories);

                
                foreach (string file in filePaths)
                {
                    TestInterfaceEngine test = new TestInterfaceEngine();
                    test.ScriptSetup(file.Replace(TestInterfaceEngine.DEFAULT_SCRIPT_MAIN, String.Empty), TestInterfaceEngine.DEFAULT_SCRIPT_MAIN);
                    testObjects.Add(test);

                }


                return testObjects; 
            }

            public static bool GetTestCaseResult(TestInterfaceEngine testCaseInstance)
            {
                
                int steps = testCaseInstance.CommandLib.Commands.Count;

                foreach (Command cmd in testCaseInstance.CommandLib.Commands)
                {
                    DebugLogger log = new DebugLogger();
                    testCaseInstance.CommandLib.Log.LogStorage.TryGetValue(cmd, out log);

                    if (log == null)
                    {
                        // Do nothing for now. 
                    }
                    else
                        if (!log.IsTestPass)
                            return false;


                }

                return true;
            }

            public static Dictionary<Command, string> GetFailedSteps(TestInterfaceEngine testCaseInstance)
            {
                Dictionary<Command, string> steps = new Dictionary<Command, string>();

                foreach (Command cmd in testCaseInstance.CommandLib.Commands)
                {
                    DebugLogger log = new DebugLogger();
                    testCaseInstance.CommandLib.Log.LogStorage.TryGetValue(cmd, out log);

                    if (log == null)
                    {
                        // Do nothing for now. 
                    }
                    else
                        if (!log.IsTestPass)
                        {
                            if(!steps.ContainsKey(cmd))
                             steps.Add(cmd,log.Debug);
                           
                        }
                }


                return steps;

            }

            public static List<TestInterfaceEngine> GetFailedTestCases(List<TestInterfaceEngine> failedtests)
            {
                List<TestInterfaceEngine> List = new List<TestInterfaceEngine>();

                foreach (TestInterfaceEngine test in failedtests)
                {
                    if (!GetTestCaseResult(test))
                        List.Add(test);
                }


                return List; 

            }

            public static bool GetTestCaseResult(TestInterfaceEngine testCaseInstance,out string errorLog)
            {
               
                errorLog = String.Empty; 

                foreach (Command cmd in testCaseInstance.CommandLib.Commands)
                {
                    DebugLogger log = new DebugLogger();
                    testCaseInstance.CommandLib.Log.LogStorage.TryGetValue(cmd, out log);

                    if (log == null)
                    {
                        // Do nothing for now. 
                    }
                    else
                        if (!log.IsTestPass)
                        {
                            errorLog = log.Debug; 
                            return false;
                        }


                }

                return true;
            }
            public static int GetFailedTestCasesCount(List<TestInterfaceEngine> testCaseList)
            {
                int count = 0;

                foreach (TestInterfaceEngine test in testCaseList)
                {
                    if (!GetTestCaseResult(test))
                        count++;
                }

                return count;
            }

            public static bool GetTestSuiteResult(List<TestInterfaceEngine> testCaseList)
            {
                foreach (TestInterfaceEngine test in testCaseList)
                {
                    if (!GetTestCaseResult(test))
                        return false;
                }


                return true;

            }

        }
        public class EmailClient
        {
            List<TestInterfaceEngine> suite;
            string EmailAddress { get; set; }
            string EmailBody { get; set; }

            public string EndPoint {get;set;}
            public string Environment {get;set;}
            public string SuiteName { get; set; }
            public string CurrentUser { get; set; }
            public string Description { get; set; }
            public string SmtpHost { get; set; }

            public EmailClient(string recipient, List<TestInterfaceEngine> listOfTests)
            {
                EmailAddress = recipient; 
                suite = listOfTests;
               
                EndPoint = String.Empty;
                Environment = String.Empty;
                SuiteName = String.Empty;
                CurrentUser = String.Empty;
                Description = String.Empty; 
            }

            public EmailClient(string recipient, TestInterfaceEngine test)
            {
                EmailAddress = recipient;
                suite = new List<TestInterfaceEngine>(); 
                suite.Add(test);

                EndPoint = String.Empty;
                Environment = String.Empty;
                SuiteName = String.Empty;
                CurrentUser = String.Empty;
                Description = String.Empty; 
            }

            private void ConvertResultsToHtml()
            {
                string tablebody = String.Empty;
                int failedTestsCount = Common.GetFailedTestCasesCount(suite);
                string user = System.Environment.UserName;
                string suitename = SuiteName.Equals(String.Empty) ? "Undefined" : SuiteName; 

                string suiteresult = "<font color='green'>Pass</font>";
                string description = failedTestsCount == 0 ? "All Test Cases have Passed" : "Number of tests cases failed:" + failedTestsCount;

                foreach (TestInterfaceEngine test in suite)
                {
                    bool testresult = Common.GetTestCaseResult(test);
                    if(!testresult)
                        suiteresult = "<font color='red'>Fail</font>";

                    string result = testresult ? "<td bgcolor='#3ADF00'>Pass</td>" : "<td bgcolor='red'>Fail</td>";
                    tablebody += "<tr><td>" + test.TestCaseName + "</td>" + result+"</tr>"; 
                     
                }

                string header = "<b>Suite Name: " + suitename + "<b><br>" +
                                "<b>Suite Result: " + suiteresult + "<b><br>" +
                                "<b>Environment: " + Environment + "<b><br>" +
                                "<b>EndPoint: " + EndPoint + "<b><br>" +
                                "<b>User: " + user + "<b><br>" +
                                "<b>Date: " + DateTime.Now + "<b><br>" +
                                "<b>Description: " + description + "<b><br><br><br>";
               
                string tableDefenition = @"<html>
                            <table>
                            <tr bgcolor='DarkSeaGreen'><td width='100'>Test Names</td><td width='100'>Test Results</td></tr>"
                            + tablebody + @"
                            </table>
                            <br>
                            <br>"
                            +GetFormatedFailedTests()+
                            @"</html>";
                            
               
                EmailBody = header+tableDefenition; 

            }

            public string GetFormatedFailedTests()
            {
                List<TestInterfaceEngine> failedTests = Common.GetFailedTestCases(suite);
                string errorlogbody = String.Empty; 
               

                foreach (TestInterfaceEngine ftest in failedTests)
                {
                    string header = String.Empty;
                    string body = String.Empty;
                    


                    Dictionary<Command, string> steps = Common.GetFailedSteps(ftest);
                    header = "<tr  bgcolor='LightGray'><td><b>TestName:</b></td><td>" + ftest.TestCaseName.Trim() + "</td></tr>";
                    header += "<tr><td><b>ErrorMessage:</b></td><td>" + Common.GetResponseNode(ftest.CommandLib,"StatusMsg") + "</td></tr>";
                    foreach (Command cmd in steps.Keys)
                    {
                        body += "<tr><td><b>Step Log:</b></td><td>"+cmd.Desc+"<br>"+steps[cmd]+"</td></tr>";

                    }


                    errorlogbody += "<table>" + header + body + "</table>";
                }

            return errorlogbody;
            }


            public void SendCustomEmail(string text)
            {
                try
                {
         
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(SmtpHost);

                    mail.From = new MailAddress("SharpMule@expedia.com", "SharpMule Usage Email");
                    mail.To.Add(EmailAddress);
                    mail.Subject = "Usage Info for User:"+ System.Environment.UserName; 
                    mail.Body = text;
    

                    SmtpServer.Send(mail);
                    Console.WriteLine("Message Sent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            public void SendEmail()
            {
                try
                {
                    ConvertResultsToHtml(); 

                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(SmtpHost);

                    mail.From = new MailAddress("SharpMule@expedia.com","SharpMule Results");
                    mail.To.Add(EmailAddress);
                    mail.Subject = "SharpMule Test Results (Environment:" + Environment + " Endpoint: " + EndPoint ;
                    mail.Body = EmailBody;
                    mail.IsBodyHtml = true;

                    SmtpServer.Send(mail);
                    Console.WriteLine("Message Sent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

        }
        public class MultiThreading
        {
            public delegate void AddSuiteCompletedHandler(object sender, EventArgs e);
            public event AddSuiteCompletedHandler ThreadsCompleted;
            public List<TestInterfaceEngine> CompletedTests { get; set; }

            int numberOfThreadsRunning;

            public void RunThreads(List<TestInterfaceEngine> testList)
            {
                CompletedTests = new List<TestInterfaceEngine>();
                numberOfThreadsRunning = testList.Count;

                for (int i = 0; i < testList.Count; i++)
                {
                    TestInterfaceEngine test = testList[i];


                    Thread thread = new Thread(() => RunThread(test));
                    thread.Start();


                }

            }
            void RunThread(TestInterfaceEngine test)
            {
                test.TestCaseCompleted += new TestInterfaceEngine.TestCaseCompletedHandler(test_TestCaseCompleted);
                test.ScriptExecute();

            }

            void test_TestCaseCompleted(object sender, EventArgs e)
            {
                CompletedTests.Add(sender as TestInterfaceEngine); 

                if (numberOfThreadsRunning == CompletedTests.Count)
                {
                    OnThreadsCompleted(EventArgs.Empty);
                }
            }

            protected virtual void OnThreadsCompleted(EventArgs e)
            {
                if (ThreadsCompleted != null)
                    ThreadsCompleted(this, e);
            }


        }

        public class JsonFormatter
        {
            #region class members
            const string Space = " ";
            const int DefaultIndent = 0;
            const string Indent = Space;
            static readonly string NewLine = Environment.NewLine;
            #endregion



            public static string PostJson(string url,string json)
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(url); request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";


                // turn our request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(json);

                // this is important - make sure you specify type this way
                request.ContentType = "application/json";
                request.ContentLength = postBytes.Length;
                request.Headers.Add(HttpRequestHeader.Authorization,"Basic c3NsdWdpY0BleHBlZGlhLmNvbTpwYTEyMzRwYQ=="); 
                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                // grab te response and print it out to the console along with the status code
                string result;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    
                    using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                    {
                        result = rdr.ReadToEnd();
                    }
                }
                catch (WebException ex)
                {
                    result = ex.Message; 

                }
                

                return result;

            }

            private enum JsonContextType
            {
                Object, Array
            }

            static void BuildIndents(int indents, StringBuilder output)
            {
                indents += DefaultIndent;
                for (; indents > 0; indents--)
                    output.Append(Indent);
            }


            bool inDoubleString = false;
            bool inSingleString = false;
            bool inVariableAssignment = false;
            char prevChar = '\0';

            Stack<JsonContextType> context = new Stack<JsonContextType>();

            bool InString()
            {
                return inDoubleString || inSingleString;
            }

            public string StringToJsonFormat(string input)
            {
                var output = new StringBuilder(input.Length * 2);
                char c;

                for (int i = 0; i < input.Length; i++)
                {
                    c = input[i];

                    switch (c)
                    {
                        case '{':
                            if (!InString())
                            {
                                if (inVariableAssignment || (context.Count > 0 && context.Peek() != JsonContextType.Array))
                                {
                                    output.Append(NewLine);
                                    BuildIndents(context.Count, output);
                                }
                                output.Append(c);
                                context.Push(JsonContextType.Object);
                                output.Append(NewLine);
                                BuildIndents(context.Count, output);
                            }
                            else
                                output.Append(c);

                            break;

                        case '}':
                            if (!InString())
                            {
                                output.Append(NewLine);
                                context.Pop();
                                BuildIndents(context.Count, output);
                                output.Append(c);
                            }
                            else
                                output.Append(c);

                            break;

                        case '[':
                            output.Append(c);

                            if (!InString())
                                context.Push(JsonContextType.Array);

                            break;

                        case ']':
                            if (!InString())
                            {
                                output.Append(c);
                                context.Pop();
                            }
                            else
                                output.Append(c);

                            break;

                        case '=':
                            output.Append(c);
                            break;

                        case ',':
                            output.Append(c);

                            if (!InString() && context.Peek() != JsonContextType.Array)
                            {
                                BuildIndents(context.Count, output);
                                output.Append(NewLine);
                                BuildIndents(context.Count, output);
                                inVariableAssignment = false;
                            }

                            break;

                        case '\'':
                            if (!inDoubleString && prevChar != '\\')
                                inSingleString = !inSingleString;

                            output.Append(c);
                            break;

                        case ':':
                            if (!InString())
                            {
                                inVariableAssignment = true;
                                output.Append(Space);
                                output.Append(c);
                                output.Append(Space);
                            }
                            else
                                output.Append(c);

                            break;

                        case '"':
                            if (!inSingleString && prevChar != '\\')
                                inDoubleString = !inDoubleString;

                            output.Append(c);
                            break;
                        case ' ':
                            if (InString())
                                output.Append(c);
                            break;

                        default:
                            output.Append(c);
                            break;
                    }
                    prevChar = c;
                }

                return output.ToString();
            }

            public static string JsonToXml(string json)
            {
                try
                {
                    XDocument xml = (XDocument)JsonConvert.DeserializeXNode(json, "root");
                    return xml.ToString(); 

                }
                catch (Exception ex)
                {
                    return ex.Message; 
                }


            }
        }
    }
}
