using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Reflection; 
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.XmlDiffPatch;

namespace SharpMule.Automation.Framework.Essentials
{
    public partial class TaskRepository
    {

        // Takes only 3 paramaters for now
        public bool HttpBsonRequest(params string[] paramlist)
        {
            WebRequest request = null;
            MongoDB.Bson.Serialization.Options.DocumentSerializationOptions.Defaults = new MongoDB.Bson.Serialization.Options.DocumentSerializationOptions { AllowDuplicateNames = true };


            string method = paramlist.ElementAtOrDefault(0);
            string url = paramlist.ElementAtOrDefault(1);
            string body = paramlist.ElementAtOrDefault(2);


            if (paramlist.Length > 2)
            {
                Log.LogDebug("Currently only to paramters are supported ('get' and 'url')");
            }
            if (!method.ToUpper().Equals("GET"))
            {
                Log.LogError("The method type you provided is not currently supported. Method: " + method);
                return false;
            }

            request = WebRequest.Create(url);

            request.Method = method;
            request.Proxy = null;
            request.Timeout = 120000;

            try
            {
                Stream stream = request.GetResponse().GetResponseStream();
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                byte[] arr = memoryStream.ToArray();

                try
                {
                    var document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(arr);
                    responseBody = document.ToJson();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex.Message);
                    Log.LogDebug("Unable to Deserialize Bson Object");

                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
                Log.LogDebug("Unable to obtain response stream from the following url:" + url);
                return false;
            }


            TestUtilities.JsonFormatter json = new TestUtilities.JsonFormatter();
        

            Log.LogDebug("Response:\n" + json.StringToJsonFormat(responseBody));

            return true;
        }

        // HttpRequest for FastInfoSet Support (takes two params, url and data)
        public bool HttpFastInfoSetRequest(params string[] paramlist)
        {
            string url = String.Empty;
            string data = String.Empty;
            string method = String.Empty;
            int timeout = 120000;

            string response = String.Empty;

            try
            {
                if (paramlist.Length < 3)
                {
                    Log.LogError("Invalid number of arguments. You must provide Url, XmlData and Method");
                    return false;
                }

                method = paramlist.ElementAtOrDefault(0);
                url = paramlist.ElementAtOrDefault(1);
                data = paramlist.ElementAtOrDefault(2);


                Log.LogDebug("Url: " + url);
                Log.LogDebug("\nRequest: " + data);

                ExternalTestRepository.HttpOperations httpOps = new ExternalTestRepository.HttpOperations();
                if (httpOps.SendFastInfoSet(url, data, method, timeout, out response))
                {
                    responseBody = TryGetXmlFormat(response);
                    Log.LogDebug("\nResponse: " + responseBody);
                }
                else
                {
                    Log.LogDebug("An error has occured sending the fastinfoset");
                    return false;

                }

            }
            catch (Exception ex)
            {
                Log.LogDebug(ex.Message);
                return false;
            }



            return true;
        }

        // Http Request that will keep calling until condition is met or timeout reached
        public bool TryHttpRequest(params string[] paramlist)
        {
            try
            {
                string criteria = paramlist.ElementAtOrDefault(6);
                string miliseconds = paramlist.ElementAtOrDefault(7);
                int retrycount = 0;
                DateTime timeout = DateTime.Now.AddMilliseconds(Convert.ToDouble(miliseconds));
                bool isNegativeCheck = false;

                if (criteria.StartsWith("!"))
                {
                    isNegativeCheck = true;
                    criteria = criteria.Trim('!');
                }

                while (timeout >= DateTime.Now)
                {
                    Log.LogClear();
                    Log.LogDebug("Number of retries completed to meet the criteria was: " + retrycount + Environment.NewLine);
                    Log.LogDebug("Last response logged is following: " + Environment.NewLine);
                    if (HttpRequest(paramlist))
                    {

                        if (isNegativeCheck)
                        {
                            if (!Regex.IsMatch(responseBody, criteria, RegexOptions.IgnoreCase))
                                return true;
                        }
                        else
                        {
                            if (Regex.IsMatch(responseBody, criteria, RegexOptions.IgnoreCase))
                                return true;
                        }


                        retrycount++;


                    }



                }
            }
            catch (Exception ex)
            {
                Log.LogError("Exception occured: " + ex.Message);
            }


            return false;
        }

        // Standard http request
        public bool HttpRequest(params string[] paramlist)
        {
            WebRequest request = null;
            // Required
            string method = String.Empty;
            string url = String.Empty;
            string data = String.Empty;
            string headers = String.Empty;

            // Optional
            string expectedStatusCode = "200";
            string expectedStatusText = "Success";
            // Inline procedure
            string proc = String.Empty;


            try
            {
                // Set Unsafe Header Parsin
                if (!ToggleAllowUnsafeHeaderParsing(true))
                    Log.LogDebug("Unable to toggle Unsafe Header Parsing"); 
                //Change SSL checks so that all checks pass
                ServicePointManager.ServerCertificateValidationCallback =
                    new System.Net.Security.RemoteCertificateValidationCallback(
                        delegate
                        { return true; }
                    );
            }
            catch
            {
                Log.LogDebug("Certificate Ignore");
            }


            if (paramlist.Length < 2)
            {
                Log.LogError("HttpRequest - Invalid Number of Arguments");
                return false;

            }
            else
            {

                method = ParseMethod(paramlist.ElementAtOrDefault(0), out proc);
                url = paramlist.ElementAtOrDefault(1);
                data = paramlist.ElementAtOrDefault(2);
                headers = paramlist.ElementAtOrDefault(3);
                expectedStatusCode = paramlist.ElementAtOrDefault(4);

                if (paramlist.ElementAtOrDefault(5) != null)
                    expectedStatusText = paramlist.ElementAtOrDefault(5);




                try
                {
                    Log.LogDebug("RequestUrl: " + url);


                    request = WebRequest.Create(url);
                }
                catch (NotSupportedException ex)
                {
                    Log.LogError("The Url you have provided seems to be invalid");
                    Log.LogDebug(ex.Message);
                    return false;
                }
                catch (UriFormatException ex)
                {
                    Log.LogError(ex.Message);
                    Log.LogDebug("Url: " + url);

                    return false;
                }
                catch (WebException ex)
                {
                    try
                    {
                        responseBody = (new StreamReader(ex.Response.GetResponseStream())).ReadToEnd();
                        Log.LogDebug("Response: \n" + responseBody);
                    }
                    catch
                    {
                        Log.LogDebug("Response: Unable to get any response. It looks like the host might be unreachable. Check the endpoint");
                    }

                    return false;
                }



                if (headers != null && headers != String.Empty)
                {
                    try
                    {
                        WebHeaderCollection col = GetHeaders(headers);
                        if(col["Content-Type"]!=null)
                        {
                            request.ContentType = col["Content-Type"]; 
                            col.Remove("Content-Type"); 

                        }

                        if(col.Count>0)
                            request.Headers = col; 
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex.Message);
                        Log.LogDebug("Invalid Request Header format. Check your request headers");
                    }
                }

                request.Method = method;
                request.Proxy = null;
                request.Timeout = 60000;
                


                if (method.Equals("post") || method.Equals("put"))
                {


                    try { data = TryGetXmlFormat(data); }
                    catch { }
                    
                    Log.LogDebug("RequestBody: \n" + data);

                    String xmlString = data;

                    UTF8Encoding encoding = new UTF8Encoding();
                    Stream newStream = null;

                    byte[] bytesToWrite = encoding.GetBytes(xmlString);
                    request.ContentLength = bytesToWrite.Length;


                    try
                    {
                        newStream = request.GetRequestStream();
                    }
                    catch (WebException ex)
                    {
                        Log.LogError("Unable to Get Request Stream. Check if the end point is in rotation or if you have entered the endpoint correctely");
                        Log.LogDebug("Endpoint:" + url); 
                        CommandExceptions.Try(() => Log.LogDebug(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()), "Unable to read the response stream");
                        return false;
                    }



                    newStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                    newStream.Close();
                }

                try
                {
                    
                    responseBody = TryGetXmlFormat((new StreamReader(request.GetResponse().GetResponseStream())).ReadToEnd());

                    try
                    {
                        string actualStatusText = GetValueByLocalName("Status");

                        if (String.Compare(actualStatusText, expectedStatusText) != 0)
                        {
                            if (String.Compare(actualStatusText, "ERROR[1000]") != 0)
                            {
                                Log.LogInfo("Expected StatusText: " + expectedStatusText + " Actual StatusText: " + actualStatusText);
                                Log.LogDebug("\nResponse: \n" + responseBody);
                                return false;
                            }
                        }
                    }
                    catch
                    {

                    }



                }
                catch (WebException ex)
                {
                    try
                    {
                        responseBody = (new StreamReader(ex.Response.GetResponseStream())).ReadToEnd();

                        string StatusCode = GetValueByLocalName("StatusCode");
                        if (expectedStatusCode != null && !expectedStatusCode.Equals(StatusCode) && !expectedStatusCode.Equals(String.Empty))
                        {
                            Log.LogInfo("Expected StatusCode: " + expectedStatusCode + " Actual StatusCode: " + StatusCode);
                            Log.LogDebug("\nResponse: \n" + responseBody);
                            return false;
                        }



                        Log.LogDebug("Response: \n" + responseBody);

                        return true;
                    }
                    catch
                    {
                        Log.LogDebug("Response: Unable to get any response. It looks like the host might be unreachable. Check the endpoint");
                        if (proc.Equals("ignore"))
                            return true;
                        else
                            return false;
                    }

                    // If you set method.ignore property then web request failure will be ignored


                }


                Log.LogDebug("Response:\n" + responseBody);

            }

          
            return true;
        }

        // Simple Sleep command
        public bool Sleep(params string[] list)
        {
            if (list.Length > 1)
            {
                Log.LogDebug("Sleep takes only one argument. Make sure you have provided only one argument");
                return false;
            }
            else
            {
                try
                {
                    int milliseconds = Convert.ToInt32(list[0]);
                    System.Threading.Thread.Sleep(milliseconds);
                    Log.LogDebug("Done Sleeping for " + milliseconds + " milliseconds");
                }
                catch (Exception ex)
                {
                    Log.LogDebug(ex.Message);
                    return false;
                }
            }

            return true;
        }

        // Convert Json to Xml
        public bool ConvertJsonToXml(params string[] list)
        {
            string xml=String.Empty;
            string varkey = String.Empty;
            string newvar = String.Empty; 
            try
            {
                 newvar = "$"+list.ElementAt(0);
                 varkey = list.ElementAt(1);
            }
            catch
            {
                Log.LogError("ConvertJsonToXml Action takes to paramters. Param(1) newVarName, Param(2) jsonInputString");
                return false; 
            }
            if(varkey.StartsWith("@") && varkey.EndsWith("@"))
            {
                string vkey = varkey.Trim('@'); 
                if (vkey.ToLower().Equals("rootbody"))
                {
                    if (responseBody.StartsWith("[") && responseBody.EndsWith("]"))
                        responseBody = responseBody.Replace("[", "").Replace("]", ""); 
                    xml = TestUtilities.JsonFormatter.JsonToXml(responseBody);
                }
                else
                {
                    string value = variables["$" + vkey];
                    xml = TestUtilities.JsonFormatter.JsonToXml(value); 
                }
                variables.Add(newvar, xml);
                Log.LogDebug(newvar + "=" + xml);
            }
          
            else
            {
                try
                {
                    xml = TestUtilities.JsonFormatter.JsonToXml(varkey);
                    Log.LogDebug(xml); 
                }
                catch
                {
                    Log.LogDebug("You must enclose the variable with @ char. Example @jsonstring@");
                    return false; 
                }
                
            }

            return true; 
        }

        //This command will simply print out info
        public bool Print(params string[] list)
        {
            string info = String.Empty;
            if (list.Length > 10)
            {
                Log.LogError("Print doesnt support more then 10 paramters");
            }
            else
            {
                foreach (string value in list)
                {
                    info += value + Environment.NewLine;
                }
            }

            Log.LogDebug(info);

            return true;
        }

        // Evaluate given data
        public bool Eval(params string[] list)
        {
            string method = list.ElementAtOrDefault(0);
            string variable = "$" + list.ElementAtOrDefault(1);
            string dataSetA = list.ElementAtOrDefault(2);
            string dataSetB = list.ElementAtOrDefault(3);
            string result = String.Empty;

            if (method == null || dataSetA == null || dataSetB == null || variable == null)
            {
                Log.LogDebug("Nothing to evaluate");
                return false;
            }


            switch (method)
            {
                case "text.delta":
                    result = GetTextDelta(dataSetA, dataSetB);
                    break;
                case "text.intersect":
                    result = GetTextIntersect(dataSetA, dataSetB);
                    break;
                default:
                    break;
            }

            StoreNewVariable(variable, result);


            return true;

        }

        // Will add new variable to the collection
        public void StoreNewVariable(string key, string value)
        {
            if (!variables.ContainsKey(key))
            {
                // Commas not allowed
                value = value.Replace(',', ' ');
                variables.Add(key, value);
                Log.LogDebug("New Variable Assigned: " + key + "=" + value);
            }
            else
            {
                // Commas not allowed
                value = value.Replace(',', ' ');
                variables[key] = value;
                Log.LogDebug("Variable Re-Assigned: " + key + "=" + value);
                Log.LogWarning("Varriable with the same name already exists and its original value will be replaced\n");
            }


            UpdateCommands(key);
        }

        // This command will grab the data from the response
        public bool Let(params string[] list)
        {
            if (list.Length < 3)
            {

                Log.LogError("Invalid Number of Arguments");

                return false;
            }
            else
            {
                string method = list[0];
                string variable = list[1];
                string locator = list[2];
                string content = list.ElementAtOrDefault(3);
                string key = "$" + variable.Trim();
                string value = String.Empty;
                string subfunc = String.Empty;
                string subfuncparams = String.Empty;

                if (content == null)
                    content = responseBody;
                if(content.StartsWith("@")&&content.EndsWith("@"))
                    content = variables["$"+content.Trim('@')]; // Special Case for data that contains { }



                if (!CommandExceptions.Try(() => value =
                            ExecuteSubFunction
                            (
                                    GetValueByMethod(
                                         method,
                                         CheckLocatorSubFunctions
                                         (
                                             locator,
                                             out subfunc,
                                             out subfuncparams
                                         ),
                                         content
                                     ),
                                     subfunc,
                                     subfuncparams
                            ) // This will conver the value based on the SubFunction Specified
                        )
                    )
                {
                    Log.LogDebug("Unable to find locator. locator=" + locator);
                    return false;
                }

                if (value.Equals("ERROR[1000]"))
                {
                    Log.LogDebug("Unable to find locator. locator=" + locator);
                    return false;
                }


                StoreNewVariable(key, value);

            }

            return true;
        }

        // This command is use to verify the data
        public bool Verify(params string[] list)
        {
            // make sure number of arguments is valid
            if (list.Count() < 2)
            {
                Log.LogDebug("Invalid number of arguments");
                return false;
            }

            // Inline method procedure value
            string proc = String.Empty;

            // Add params verification to make sure correct num of params has been given
            string method = list[0];
            string locator = list[1];
            string expecedResult = list.ElementAtOrDefault(2);
            string content = list.ElementAtOrDefault(3);
            string ignorelist = list.ElementAtOrDefault(4) == null ? String.Empty : list.ElementAtOrDefault(4); //Used for XmlDiff if you want to specify which nodes to ignore 
            string subfunc = String.Empty;
            string subfuncparams = String.Empty;

            if (content == null)
                content = responseBody;


            string actualResult = ExecuteSubFunction
                                    (
                                            GetValueByMethod(
                                                 method,
                                                 CheckLocatorSubFunctions
                                                 (
                                                     locator,
                                                     out subfunc,
                                                     out subfuncparams
                                                 ),
                                                 content
                                             ),
                                             subfunc,
                                             subfuncparams
                                    ); // This will conver the value based on the SubFunction Specified



            method = ParseMethod(method, out proc);

            //
            // If you provide verify{value.nexist...} is not provided then it will check
            // the actualResult. If actualResult has error code, it will log error and fail
            //
            if (proc.Contains("nexist"))
            {
                if (actualResult.Equals("ERROR[1000]") || actualResult.Equals(String.Empty))
                {
                    Log.LogDebug("Locator " + locator + " does NOT exists!");
                    return true;
                }

                Log.LogDebug("Locator " + locator + " exists!");
                return false;
            }
            else if (proc.Contains("nsame"))
            {
                if (actualResult.Equals("ERROR[1000]") || actualResult.Equals(String.Empty))
                {
                    Log.LogDebug("Locator " + locator + " does NOT exists!");

                }
                else if (!actualResult.Equals(expecedResult))
                {
                    Log.LogDebug("Value " + actualResult + " is not the same!");
                }
                else
                {
                    Log.LogDebug("Locator " + locator + " exists or the Value " + actualResult + " is the same!");
                    return false;
                }


                return true;
            }
            else
            {
                if (proc.Contains("exist"))
                {

                    if (actualResult.Equals("ERROR[1000]"))
                    {
                        Log.LogDebug("Unable to find the value you are locking for");
                        return false;
                    }
                    //if (actualResult.Equals(String.Empty))
                    //{
                    //    Log.LogDebug("Unable to find the value you are locking for");
                    //    return false;
                    //}

                    Log.LogDebug("Locator " + locator + " exists! Value="+actualResult);
                    return true;
                }
                else
                {

                    if (actualResult.Equals("ERROR[1000]"))
                    {
                        Log.LogError("Incorrect value locator, unable to find the node");
                        Log.LogDebug("Locator=" + locator + "\n------Response------\n" + content + "\n------EndResponse------\n");
                        Log.LogDebug("\nExpected:" + expecedResult + "\nActual:" + actualResult);
                        return false;
                    }
                    else
                    {
                        if (method.Equals("xml"))
                        {
                            if (proc.Equals(String.Empty) || proc.Equals("xml") || proc.Equals("find") || proc.Equals("compare-same"))
                            {
                                return XmlDiff(expecedResult, actualResult, 0);
                            }
                            else if (proc.Equals("compare-similar"))
                            {
                                return XmlDiff(expecedResult, actualResult, Convert.ToInt16(content));
                            }
                            else if (proc.Equals("compare-samenorder"))
                            {
                                return XmlDiff2(expecedResult, actualResult, ignorelist);
                            }

                            else
                            {
                                Log.LogDebug("Expected Value: " + expecedResult + " Actual Value: " + actualResult);
                                if (expecedResult.Equals(actualResult))
                                    return true;
                                else
                                    return false;


                            }

                        }
                        else if (method.Equals("regex"))
                        {
                            try
                            {
                                if (Regex.IsMatch(actualResult, expecedResult))
                                {
                                    Log.LogDebug("\nExpected:" + expecedResult + "\nActual:" + actualResult);
                                    return true;
                                }
                                else
                                {
                                    Log.LogDebug("\nExpected:" + expecedResult + "\nActual:" + actualResult);
                                    return false;
                                }
                            }
                            catch(Exception ex)
                            {
                                Log.LogError(ex.Message);
                                return false; 
                            }
                        }
                        else if (method.Equals("compare"))
                        {
                            if (content.Equals(String.Empty))
                            {
                                Log.LogDebug("Last argument is missing (type: boolean)");

                                return false;
                            }
                            else
                            {
                                Log.LogDebug("\nExpected:" + expecedResult + "\nActual:" + actualResult);

                                try
                                {
                                    bool value = (expecedResult.Equals(actualResult) == Convert.ToBoolean(content));

                                    return value;
                                }
                                catch
                                {
                                    Log.LogError("Paramter that was passed in is not of Boolean Type. This procedure requires boolean value for comparison");
                                    return false;
                                }

                            }
                        }
                        else if (method.Equals("value"))
                        {
                            return EvaluateProcedure(proc, actualResult, expecedResult);
                        }

                        else if (method.Equals("contains"))
                        {
                            string[] xmlNodeVals = actualResult.Split(';');
                            foreach (string nodeVal in xmlNodeVals)
                            {
                                if (string.Equals(nodeVal, expecedResult, StringComparison.OrdinalIgnoreCase))
                                {
                                    Log.LogDebug(string.Format("Locator", locator, " exists!"));
                                    return true;
                                }
                            }

                            Log.LogDebug(string.Format("Locator", locator, " does NOT exist!"));
                            return false;
                        }

                        else if (method.Equals("ncontains"))
                        {
                            string[] xmlNodeVals = actualResult.Split(';');
                            foreach (string nodeVal in xmlNodeVals)
                            {
                                if (string.Equals(nodeVal, expecedResult, StringComparison.OrdinalIgnoreCase))
                                {
                                    Log.LogDebug(string.Format("Locator", locator, " exists!"));
                                    return false;
                                }
                            }

                            Log.LogDebug(string.Format("Locator", locator, " does NOT exist!"));
                            return true;
                        }
                        else
                        {

                            Log.LogError("Unknown method type provided: " + method);
                            return false;
                        }
                    }
                }
            }

        }

        // This command is intended to be used to force assign new variables 
        public bool Assign(params string[] list)
        {
            if (list.Length != 2)
            {

                Log.LogError("Invalid Number of Arguments");

                return false;
            }
            else
            {
                string varkey = "$" + list[0];
                string value = list[1];

                if (Variables.ContainsKey(varkey))
                {
                    Log.LogDebug("Variable has been re-assigned. " + varkey + "=" + value);
                    Variables[varkey] = value;
                }
                else
                {
                    Log.LogDebug("New Variable has been added. " + varkey + "=" + value);
                    Variables.Add(varkey, value);
                }

            }

            return true;

        }

        // This command is intended to be used to force update existing variables 
        public bool Update(params string[] list)
        {
            if (list.Length != 4)
            {

                Log.LogError("Invalid Number of Arguments");

                return false;
            }
            else
            {
                string method = list[0];
                string locator = list[1];
                string varkey = "$" + list[2];
                string value = list[3];

                if (Variables.ContainsKey(varkey))
                {
                    string content = Variables[varkey];

                    string updatedContent = UpdateValueByMethod(method, locator, content, value);

                    if (!updatedContent.Equals(String.Empty))
                    {
                        Log.LogDebug("Successfuly update locator: " + locator + " with value: " + value);
                        Variables[varkey] = updatedContent;
                    }
                    else
                    {
                        Log.LogDebug("Failed to update locator: " + locator + " with value: " + value);

                        return false;
                    }
                }
                else
                {
                    Log.LogDebug("Unable to find given variable. Variable: " + varkey);

                    return false;
                }

            }


            return true;

        }

        // This command is to repeate certain command multiple times or within certain period of time
        public bool CallMethod(params string[] list)
        {
            string assemblyname = list.ElementAtOrDefault(0);
            string method = list.ElementAtOrDefault(1);
            string paramstr = list.ElementAtOrDefault(2);


            if (list.Length < 2)
            {
                Log.LogError("Invalid number of arguments. Format: callmethod{assemblyname,method(param1;param2.....paramN)}");
                return false;

            }
            else
            {
                try
                {

                    string[] paramlist = paramstr.Split(',');

                    Assembly assembly = Assembly.LoadFrom(assemblyname);

                    Type[] types = assembly.GetTypes();
                    //Program prog = new Program(); 
                    Type type = types[0];
                    object instance = Activator.CreateInstance(type);
                    MethodInfo info = type.GetMethod(method);
                    PropertyInfo property = type.GetProperty("Log");

                    bool result = Convert.ToBoolean(info.Invoke(instance, new object[] { paramlist }));
                    //string log = property.GetValue(instance, null).ToString(); 

                    //Log.LogDebug(log);

                    return result;
                }
                catch (Exception ex)
                {
                    Log.LogError("Unexpected error occured. Verify your command");
                    Log.LogDebug(ex.Message);

                    return false;
                }
            }

        }

        // Ssh Execute allows you to connect to remote linux machine and run command
        public bool SshExecute(params string[] paramlist)
        {

            string host = paramlist.ElementAtOrDefault(0);
            string user = paramlist.ElementAtOrDefault(1);
            string password = paramlist.ElementAtOrDefault(2);
            string command = paramlist.ElementAtOrDefault(3);
            int timeout = Convert.ToInt32(paramlist.ElementAtOrDefault(4));

            TestUtilities.RemoteOperations.SSH ssh = new TestUtilities.RemoteOperations.SSH();
            if (ssh.Connect(host, user, password))
            {
                responseBody = String.Empty;

                if (ssh.SendCommand(command, timeout, out responseBody))
                {
                    ssh.Disconnect();
                    Log.LogDebug(responseBody);
                    return true;
                }
                else
                {
                    Log.LogError("Unable to Send Command");
                    return false;

                }
            }
            else
            {
                Log.LogError("Unable to connect to remote server");
                return false;
            }

        }

        // Sql Execute will connect to Sql DB and execute command. Returns text format
        public bool SqlExecute(params string[] paramlist)
        {
            string str = String.Empty;

            return SqlExecute("as.xml", out str, paramlist);
        }

        // Sql Execute will connect to Sql DB and execute command. Returns xml format
        public bool SqlExecute2(params string[] paramlist)
        {
            string str = String.Empty;

            return SqlExecute("as.text", out str, paramlist);
        }

        //MySql Execute will connect to Sql DB and execute command.
        public bool MySqlExecute(params string[] paramlist)
        {
            string connectionString = paramlist.ElementAtOrDefault(0);
            string sqlQuery = paramlist.ElementAtOrDefault(1);
            string response = String.Empty;

            try
            {

                DataBaseTasks db = new DataBaseTasks();
                db.DBType = DataBaseTasks.DataBaseType.MY_SQL;

                db.Connect(connectionString);
                db.SendQuery(sqlQuery, out responseBody);
                db.CloseConnection();

                response = responseBody;
                Log.LogDebug("Query:\n" + sqlQuery);
                Log.LogDebug("Result:\n" + response);

            }
            catch (Exception e)
            {
                response = e.Message;
                Log.LogError(e.Message);
                Log.LogDebug("Query: " + sqlQuery);
                return false;
            }
            return true;
        }

        // General sql exectute that can be used to specify either mysql or sql
        public bool SqlExecute(string type, out string response, params string[] paramlist)
        {
            response = String.Empty;

            if (paramlist.Length < 3)
            {
                Log.LogError("Invalid Number of arguments has been passed in. SqlExecute requires 3 parameters {ConnectionString,DbName and Query}");
            }
            else
            {
                string server = paramlist.ElementAtOrDefault(0); // Connection String
                string dbName = paramlist.ElementAtOrDefault(1); // Db Instance Name
                string sqlQuery = paramlist.ElementAtOrDefault(2); // sql query to execute

                if (paramlist.Length >= 3)
                    for (int i = 3; i < paramlist.Length; i++)
                        sqlQuery += "," + paramlist[i];

                if (server.Equals(String.Empty) || dbName.Equals(String.Empty) || sqlQuery.Equals(String.Empty))
                    Log.LogError("One or more of the parameters you have passed in are empty strings. This is not valid input");



                try
                {

                    DataBaseTasks db = new DataBaseTasks();
                    db.DBType = DataBaseTasks.DataBaseType.MS_SQL;
                    db.Connect(server, dbName);

                    switch (type)
                    {
                        case "as.xml":
                            db.SendQuery(sqlQuery, out responseBody);
                            break;
                        case "as.text":
                            db.SendQuery2(sqlQuery, out responseBody);
                            break;
                        default:
                            db.SendQuery(sqlQuery, out responseBody);
                            break;

                    }

                    db.CloseConnection();

                    response = responseBody.Replace(',', ' ');
                    Log.LogDebug("Query:\n" + sqlQuery);
                    Log.LogDebug("Result:\n" + response);

                }
                catch (Exception e)
                {
                    response = e.Message;
                    Log.LogError(e.Message);
                    Log.LogDebug("Query: " + sqlQuery);
                    return false;
                }
            }
            return true;
        }

    }
}
