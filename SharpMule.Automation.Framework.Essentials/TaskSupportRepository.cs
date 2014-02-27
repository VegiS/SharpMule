using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; 
using System.IO; 
using System.Collections;
using System.Net;
using System.Net.Configuration; 
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Reflection;
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

        public bool LogToFile(params string[] paramlist)
        {
            try
            {
                string file = paramlist.ElementAtOrDefault(0);
                string content = String.Empty;

                for (int i = 1; i < paramlist.Length; i++)
                {
                    content += paramlist.ElementAtOrDefault(i) + Environment.NewLine;
                }



                File.WriteAllText(file, content);

                Log.LogDebug("Log has been successfully written to " + file); 
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
                return false;
            }


            return true; 

        }

        //
        //   Helper Functions
        //

        private string CheckLocatorSubFunctions(string locator, out string subfunction,out string parameters)
        {
            subfunction = String.Empty;
            parameters = String.Empty; 

            if (Regex.IsMatch(locator, @"ToLower\(\)"))
            {
                subfunction = "ToLower";
                return Regex.Replace(locator, @"\.ToLower\(\)", "");

            }
            if (Regex.IsMatch(locator, @"ToUpper\(\)"))
            {
                subfunction = "ToUpper";
                return Regex.Replace(locator, @"\.ToUpper\(\)", "");

            }
            if (Regex.IsMatch(locator, @"ToTitle\(\)"))
            {
                subfunction = "ToTitle";
                return Regex.Replace(locator, @"\.ToTitle\(\)", "");

            }
            if (Regex.IsMatch(locator, @"ToUpperLowerMix\(\)"))
            {
                subfunction = "ToUpperLowerMix";
                return Regex.Replace(locator, @"\.ToUpperLowerMix\(\)", "");

            }
            if (Regex.IsMatch(locator, @"ToDateTimeFormat\((.*?)\)"))
            {

                subfunction = "ToDateTimeFormat";
                parameters = Regex.Match(locator,@"(?<=\()(.*?)(?=\))").Value; 

                return Regex.Replace(locator, @"\.ToDateTimeFormat\((.*?)\)", "");

            }
            if (Regex.IsMatch(locator, @"ToDateTimeUTCFormat\((.*?)\)"))
            {

                subfunction = "ToDateTimeUTCFormat";
                parameters = Regex.Match(locator, @"(?<=\()(.*?)(?=\))").Value;

                return Regex.Replace(locator, @"\.ToDateTimeUTCFormat\((.*?)\)", "");

            }
            if (Regex.IsMatch(locator, @"ToXml\(\)"))
            {
                subfunction = "ToXml";

                return Regex.Replace(locator, @"\.ToXml\(\)", "");
            }
            return locator;

        }

        private static string ExecuteSubFunction(string value, string subfunction,string parameters)
        {
            string newValue = String.Empty;
            int i = 0;

            switch (subfunction)
            {
                case "ToLower":
                    return value.ToLower();
                case "ToUpper":
                    return value.ToUpper();
                case "ToTitle":
                    foreach (char c in value)
                    {

                        if (i == 0)
                            newValue += c.ToString().ToUpper();
                        else
                            newValue += c.ToString().ToLower();

                        i++;
                    }
                    return newValue;
                case "ToUpperLowerMix":
                    foreach (char c in value)
                    {

                        if (i % 2 == 0)
                            newValue += c.ToString().ToLower();
                        else
                            newValue += c.ToString().ToUpper();

                        i++;
                    }

                    return newValue;
                case "ToDateTimeFormat":
                    DateTime dt = new DateTime();
                    dt = DateTime.Parse(value);
                    return dt.ToString(parameters); 
                case "ToDateTimeUTCFormat":
                    return DateTime.Parse(value).ToUniversalTime().ToString(parameters); 
                case "ToXml":
                    return TestUtilities.JsonFormatter.JsonToXml(value); 
                default:
                    return value;

                // let{value.from.xml.attribute,XmlBody,flightSummary.ToXml()} 

            }

        }

        public string LookUpResponse(string locator)
        {
            string value = String.Empty;
            try
            {
                value = GetValueByLocalName(locator);
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }


            return value;

        }
        private string TryGetXmlFormat(string xmlstring)
        {
            try
            {
                xmlstring = xmlstring.Trim('\n', '\r');
                return XDocument.Parse(xmlstring).ToString();
            }
            catch
            {
                return xmlstring;
            }


        }
        public static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            //Get the assembly that contains the internal class
            Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created already invoking the property will create it for us.
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }
        private WebHeaderCollection GetHeaders(string headers)
        {
            headers = headers.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
            string[] arrheaders = headers.Split(';');
            WebHeaderCollection headerCollection = new WebHeaderCollection();
            string keepheaders = String.Empty;
            foreach (string header in arrheaders)
            {
                if (!header.Equals(String.Empty))
                {
                    headerCollection.Add(header.Trim());
                    keepheaders += header + Environment.NewLine;
                }
            }

            Log.LogDebug("Headers:\n" + keepheaders);

            return headerCollection;
        }
        
        public XElement Find(string locator, string xml)
        {
            // sibling:StartingNode:TargetNode:
            // descentent:StartingNode:TargetNode:Attribute=attrname

            string[] locsplit = locator.Split('='); // devide string by = sign if it exists
            string[] instructions = locsplit[0].Split(':'); // get all instructions from the left side of = sign
            string relation = instructions.ElementAtOrDefault(0);
            string fromnode = instructions.ElementAtOrDefault(1);
            string findnode = instructions.ElementAtOrDefault(2);
            string attribute = instructions.ElementAtOrDefault(3);
            string attvalue = locsplit.ElementAtOrDefault(1);

            switch (relation)
            {
                case "sibling":
                    XElement findresult = FindByLocalName(fromnode, xml);
                    XElement child = null;

                    if (findresult != null)
                    {
                        XElement parent = findresult.Parent;
                        child = FindByLocalName(findnode, parent.ToString());


                    }

                    return child;

                case "descendent":
                    if (attribute != null)
                        return FindDescendentByLocalNameAndAttribute(fromnode, findnode, attribute, attvalue, xml);
                    else
                        return FindDescendentByLocalName(fromnode, findnode, xml);
                default:
                    break;
            }


            return null;
        }


        public string XmlDiffRemoveIgnoredNodes(string xml,string elementToRemove)
        {

            XElement xs = XElement.Parse(xml);
            List<XElement> elToRemove = new List<XElement>();

            xs = RemoveAllAttributeValuesByName(elementToRemove, xml);

            foreach (XElement x in xs.Descendants())
            {
                if (x.Name.LocalName.Equals(elementToRemove))
                    elToRemove.Add(x);
 
            }

            


            foreach (XElement x in elToRemove)
            {
                //Log.LogWarning("Ignoring Element: " + x.ToString());
                x.Remove();
            }

            return xs.ToString(); 
        }
        public bool XmlDiff2(string exp, string act, string ElementsToIgnore)
        {
            string actual = act;
            string expected = exp;
            string[] IgnoredNodes = ElementsToIgnore.Split(';'); 

            try
            {
                StringBuilder result = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(result);
                StringWriter xmlstring = new StringWriter();

                
                foreach (string ignore in IgnoredNodes)
                {
                    actual = XmlDiffRemoveIgnoredNodes(actual, ignore.Trim());
                    expected = XmlDiffRemoveIgnoredNodes(expected, ignore.Trim());
                }

                ASCIIEncoding myEncoder = new ASCIIEncoding();

                XmlReader expXmlReader = XmlReader.Create((new MemoryStream(myEncoder.GetBytes(expected.ToLower()))));
                XmlReader actXmlReader = XmlReader.Create((new MemoryStream(myEncoder.GetBytes(actual.ToLower()))));

               
                XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder |
                                    XmlDiffOptions.IgnoreNamespaces | 
                                    XmlDiffOptions.IgnoreXmlDecl | 
                                    XmlDiffOptions.IgnoreWhitespace |
                                    XmlDiffOptions.IgnorePrefixes |
                                    XmlDiffOptions.IgnoreDtd | 
                                    XmlDiffOptions.IgnorePI | 
                                    XmlDiffOptions.IgnoreComments);

                bool isIdentical = xmldiff.Compare(expXmlReader, actXmlReader,writer);
                writer.Close();


                XDocument xdoc = XDocument.Parse(result.ToString());
                StringBuilder changes = new StringBuilder();


                foreach (XElement element in xdoc.Descendants())
                {
                    if (element.Name.LocalName.Equals("change"))
                    {
                        changes.Append("Different : ");
                        changes.AppendLine(element.ToString());

                    }

                    if (element.Name.LocalName.Equals("add"))
                    {
                        changes.Append("Added : ");
                        changes.AppendLine(element.ToString());

                    }

                    if (element.Name.LocalName.Equals("remove"))
                    {
                        changes.Append("Removed : ");
                        changes.AppendLine(element.ToString());

                    }
                }




                Log.LogDebug(changes.ToString());
                Log.LogDebug("Expected: " + Environment.NewLine + exp);
                Log.LogDebug("Actual: " + Environment.NewLine + act); 

                return isIdentical;
            }
            catch (Exception ex)
            {
                Log.LogDebug(ex.Message);
                Log.LogDebug("\nExp2:\n" + exp);
                return false;
            }

            
        }

        public bool XmlDiff(string exp, string act, int levenshteinDistance)
        {
            int elementCount = 0;

            string nsprefix = @"(?<=<)\w*:(?=(.*?)>)";
            string nspostfix = @"(?<=</)\w*:(?=(.*?)>)";
            string nsdef = @"(?<=<\w*)\s(.*?)(?=>)";
            string nsdefalt = @"(?<=<\w*)\s(.*?)(?=/>)";

            // Remove all of the namespaces and ns defenitions
            exp = Regex.Replace(exp, nsprefix, String.Empty);
            exp = Regex.Replace(exp, nspostfix, String.Empty);
            exp = Regex.Replace(exp, nsdefalt, String.Empty);
            exp = Regex.Replace(exp, nsdef, String.Empty);
            act = Regex.Replace(act, nsprefix, String.Empty);
            act = Regex.Replace(act, nspostfix, String.Empty);
            act = Regex.Replace(act, nsdefalt, String.Empty);
            act = Regex.Replace(act, nsdef, String.Empty);

            try
            {

                XDocument expectedDoc = XDocument.Load(XmlReader.Create(new StringReader(exp)));
                XDocument actualDoc = XDocument.Load(XmlReader.Create(new StringReader(act)));

                Array expected = expectedDoc.Descendants().ToArray();
                Array actual = actualDoc.Descendants().ToArray();

                if (expected.Length < actual.Length)
                    elementCount = expected.Length;
                else if (expected.Length > actual.Length)
                    elementCount = actual.Length;
                else
                    elementCount = expected.Length;

                for (int i = elementCount - 1; i >= 0; i--)
                {


                    string pattern = @"[ \t\r\n]";

                    string expectedValue = Regex.Replace(expected.GetValue(i).ToString(), pattern, "");
                    string actualValue = Regex.Replace(actual.GetValue(i).ToString(), pattern, "");


                    if (!expectedValue.Equals(actualValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //Compute Distance - If close enough, return true
                        if (TestUtilities.Algorithams.LevenshteinDistance.Compute(expectedValue, actualValue) > levenshteinDistance)
                        {
                            Log.LogDebug("\nExpected Differance between the expected and actual is exceeding the acceptable similarity threshold");
                            Log.LogDebug("\nExpected Node:\n" + expectedValue);
                            Log.LogDebug("\nActual Node:\n" + actualValue);
                            Log.LogDebug("\nExpected:\n" + exp);
                            Log.LogDebug("\nActual:\n" + act);
                            return false;
                        }

                        Log.LogWarning("Following values are similar but not same. \nExpected: " + expectedValue + " \nActual: " + actualValue);
                    }

                }

                Log.LogDebug("\nExpected:\n" + expectedDoc.ToString());
                Log.LogDebug("\nActual:\n" + actualDoc.ToString());
            }
            catch (Exception ex)
            {
                Log.LogDebug(ex.Message);
                Log.LogDebug("\nExp2:\n" + exp);
                return false;
            }

            return true;
        }
        // If there is any variables in the parameters of the command, go and replace those 
        // variables with their values. Also it will recursivly replace any variables in the body of the existing 
        // variables by calling ConvertContent
        public string ConvertParamterVariables(string content)
        {
            string var_pattern = @"\$[a-zA-Z0-9_]+";

            MatchCollection matches = Regex.Matches(content, var_pattern);

            foreach (Match m in matches)
            {
                string key = m.Value;
                if (Variables.ContainsKey(key))
                {
                    string varcontent = ConvertContent(Variables[key]);
                    content = content.Replace(key, varcontent);
                }

            }

            return content;
        }
        // Converts the content of the Variables if any nested variables have been found
        private string ConvertContent(string content)
        {
            string var_pattern = @"\(?\$[a-zA-Z0-9_]+\)?";

            MatchCollection matches = Regex.Matches(content, var_pattern);

            // Find all variables in the content of the other variables and replace it. 
            // Variables can be in two formats ($var) or just $var. We need to trim if its in the first format
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {


                    string fullvar = m.Value;
                    string key = fullvar.Trim('(', ')');

                    if (Variables.ContainsKey(key))
                    {
                        string varcontent = Variables[key];

                        int index = content.IndexOf(fullvar, 0, content.Length);
                        content = content.Remove(index, fullvar.Length);
                        content = content.Insert(index, varcontent);


                    }
                    else
                    {
                        Log.LogDebug("Invalid Variable found. You need to define following variable before you can use it. Variable = " + key + ". Proceeding with this content anyway");
                        return content;
                    }

                }

                return ConvertContent(content);
            }
            else
            {
                return content;
            }



        }
        private bool EvaluateProcedure(string proc, string actualResult, string expectedResult)
        {
            switch (proc)
            {
                case "nequal":

                    bool equals = actualResult.Equals(expectedResult);

                    if (equals)
                        Log.LogDebug("Actual Result " + actualResult + " equals the value " + expectedResult);
                    else
                        Log.LogDebug("Actual Result " + actualResult + " does NOT equal the value " + expectedResult);

                    return !equals;

                case "contains":

                    bool contains = actualResult.Contains(expectedResult);

                    if (contains)
                        Log.LogDebug("Actual Result " + actualResult + " contains the value " + expectedResult);
                    else
                        Log.LogDebug("Actual Result " + actualResult + " does NOT contains the value " + expectedResult);

                    return contains;

                case "in.contains":

                    bool incontains = actualResult.Contains(expectedResult);

                    if (incontains)
                        Log.LogDebug("Actual Result " + actualResult + " contains the value " + expectedResult);
                    else
                        Log.LogDebug("Actual Result " + actualResult + " does NOT contains the value " + expectedResult);

                    return incontains;

                case "ncontains":

                    bool ncontains = actualResult.Contains(expectedResult);

                    if (ncontains)
                        Log.LogDebug("Actual Result " + actualResult + " contains the value " + expectedResult);
                    else
                        Log.LogDebug("Actual Result " + actualResult + " does NOT contains the value " + expectedResult);

                    return !ncontains;

                case "in.ncontains":
                    bool inncontains = actualResult.Contains(expectedResult);

                    if (inncontains)
                        Log.LogDebug("Actual Result " + actualResult + " contains the value " + expectedResult);
                    else
                        Log.LogDebug("Actual Result " + actualResult + " does NOT contains the value " + expectedResult);

                    return !inncontains;


                case "in.nequals":
                    if (String.Compare(actualResult, expectedResult, true) != 0)
                    {
                        Log.LogDebug("\nExpected:" + expectedResult + "\nActual:" + actualResult);
                        return true;
                    }
                    else
                    {
                        Log.LogDebug("\nExpected:" + expectedResult + "\nActual:" + actualResult);
                        return false;
                    }

                case "from.xml.attribute":
                    try
                    {
                        string[] results = actualResult.Split(';');
                        string[] expresults = expectedResult.Split(';');
                        bool testresult = false;
                        Dictionary<string, bool> testresults = new Dictionary<string, bool>(); 

                        foreach (string result in results)
                        {
                            testresult = false; 
                            foreach (string er in expresults)
                            {
                                if (String.Compare(er, result, true) == 0)
                                {
                                    testresult = true; 
                                    Log.LogDebug("\nExpected:" + er + "\nActual:" + result);
                                }
                                else
                                {
                                    testresult = false;
                                    Log.LogDebug("\nExpected:" + er + "\nActual:" + result);
                                }
                               
                            }

                            if (!testresults.ContainsKey(result))
                                testresults.Add(result, testresult);
                            else
                                testresults[result] = testresult; 

                        }

                        //foreach (string key in testresults.Keys)
                        //{
                        //    if (!testresults[key])
                        //    {
                        //        testresult = false; 
                        //        Log.LogDebug("\nExpected:" + key + " but not found");
                        //    }
                        //}
                           


                        return testresult;
                    }
                    catch(Exception ex)
                    {
                        Log.LogDebug("Exception encountered: " + ex.ToString() + "\nMessage: " + ex.Message + "\nInnerException: " + ex.InnerException.ToString());
                        return false;
                    }
                    


                default:
                    // Try RegEx first
                    try
                    {
                        actualResult = actualResult.Trim();
                        expectedResult = expectedResult.Trim(); 
                        if (Regex.IsMatch(actualResult, expectedResult, RegexOptions.IgnoreCase) && actualResult.Length==expectedResult.Length)
                        {
                            Log.LogDebug("\nExpected:" + expectedResult + "\nActual:" + actualResult);
                            return true;
                        }
                        else
                        {
                            Log.LogDebug("\nExpected:" + expectedResult + "\nActual:" + actualResult);
                            return false;
                        }
                    }
                     // Try StringCompare if Regex throws an exception
                    catch  
                    {
                        if (String.Compare(actualResult, expectedResult,true)==0)
                        {
                            Log.LogDebug("\nExpected:" + expectedResult + "\nActual:" + actualResult);
                            return true;
                        }
                        else
                        {
                            Log.LogDebug("\nExpected:" + expectedResult + "\nActual:" + actualResult);
                            return false;
                        }
                    }


            }

        }
        private string ParseMethod(string method, out string procedure)
        {
            procedure = method;

            if (method.Contains('.'))
            {
                string[] mplist = method.Split('.');
                method = mplist[0];
                procedure = procedure.Replace(method, String.Empty).TrimStart('.');
            }

            return method;
        }
        private void UpdateCommands(string key)
        {
            foreach (Command cmd in commands)
            {
                if (cmd.Param.Contains(key))
                {
                    cmd.Param = cmd.Param.Replace(key, variables[key]);
                }
            }
        }
        private string UpdateValueByMethod(string method, string locator, string content, string value)
        {
            switch (method.ToLower())
            {
                case "xml.value":
                    return FindAndUpdateByLocalName(locator, content, value);
                default:
                    Log.LogError("Invalid search method provided - method:" + method + " locator:" + locator);
                    return String.Empty;

            }

        }
        private string GetValueByMethod(string method, string locator)
        {
            return GetValueByMethod(method, locator, responseBody);
        }
        private string GetValueByMethod(string method, string locator, string content)
        {
            switch (method.ToLower())
            {
                case "xml":
                case "xml.compare-same":
                case "xml.compare-similar":
                case "xml.compare-samenorder":
                case "xml.nexist":
                case "xml.exist":
                    return GetInnerXmlByLocalName(locator);
                case "xml.find":
                    return Find(locator, content).ToString();
                case "xml.count":
                    return GetNodeCount(locator);
                case "xml.attribute.count":
                    return FindByAttributeName(locator, content, "name").Count().ToString(); 
                case "value":
                case "value.exist":
                case "value.nexist":
                case "value.contains":
                case "value.ncontains":
                case "value.compare":
                    return GetValueByLocalName(locator);
                case "value.in.contains":
                    return locator; // In case you want to verify the variable content instead of searching locator. 
                case "value.in.equals":
                    return locator;
                case "value.from.xml":
                    return GetValueByLocalName(locator, content);
                case "value.from.xml.attribute.nsame":
                case "value.from.xml.attribute.nexist":
                case "value.from.xml.attribute.exist":
                case "value.from.xml.attribute":
                    return GetValueByAttributeName(locator, content);
                case "value.from.xml.attribute.exist.opt":
                case "value.from.xml.attribute.opt":
                    return GetValueByAttributeNameOpt(locator, content); 
                case "value.from.xml.attribute.multi":
                    return GetValueByAttributeNameMulti(locator, content);
                case "value.find":
                    XElement find = Find(locator, content);
                    return find == null ? "ERROR[1000]" : find.Value;
                case "value.list":
                    return GetListByLocalName(locator, content); 
                case "value.list.math.add":
                    return CalculateValueFromList(locator, content, "add"); 
                case "xpath":
                    return GetInnerXmlByXPath(locator);
                case "html":
                    return GetValueFromHtml(locator);
                case "regex":
                    return GetValueByRegx(locator);
                case "ncontains":
                    return GetValuesForMultipleNodes(locator, content); //multiple nodes of the same name
                case "contains":
                    return GetValuesForMultipleNodes(locator, content); //multiple nodes of the same name
                case "compare":
                    return locator;
                default:
                    Log.LogError("Invalid search method provided - method:" + method + " locator:" + locator);
                    return String.Empty;

            }
        }

        private string GetTextIntersect(string textA, string textB)
        {
            string[] a = textA.Split(new char[] { '\n' });
            string[] b = textB.Split(new char[] { '\n' });



            List<string> listA = a.ToList<string>();
            List<string> listB = b.ToList<string>();

            var diff = listA.Intersect(listB);

            string delta = String.Empty;
            foreach (string s in diff)
            {
                delta = delta + s.Trim(new char[] { '\r', '\n' }) + Environment.NewLine;
            }


            return delta;

        }
        private string GetTextDelta(string textA, string textB)
        {
            string[] a = textA.Split(new char[] { '\n' });
            string[] b = textB.Split(new char[] { '\n' });



            List<string> listA = a.ToList<string>();
            List<string> listB = b.ToList<string>();

            var diff = listA.Except(listB); 

            string delta = String.Empty;
            foreach (string s in diff)
            {
                delta = delta+s.Trim(new char[]{'\r','\n'}) + Environment.NewLine; 
            }


            return delta; 
            
        }
        private string GetValueFromHtml(string locator)
        {
            HtmlDocument html = new HtmlDocument();
            string value = String.Empty;

            CommandExceptions.Try(() => html.LoadHtml(responseBody), "Unable to LoadHtml from ResponseBody or Unable to Select Specific Node [E0010]");


            if (locator.StartsWith("substring"))
            {
                CommandExceptions.Try(() => value = GetSubString(locator, html), "Unable to get Substring you are looking for. Check if the response came back correctely [E0011a]");

            }
            else
            {
                CommandExceptions.Try(() => value = html.DocumentNode.SelectNodes(locator)[0].InnerText, "Unable to get the node you are looking for. Check if the response came back correctely [E0011b]");
            }


            return value;

        }
        private string GetValueByXPath(string xpath)
        {
            string value = String.Empty;
            try
            {
                value = FindByXPath(xpath).Value;
            }
            catch
            {
                Log.LogError("Incorrect xpath, unable to find the node");
                Log.LogDebug("XPath = " + xpath + "\n------Response------\n" + responseBody + "\n------EndResponse------");
            }
            return value;
        }
        private string GetValuesForMultipleNodes(string locator, string content)
        {
            List<XElement> elems = FindAllNodesByLocalName(locator, content);
            string toReturn = "ERROR[1000]";

            foreach (XElement elem in elems)
            {
                if (elem.ToString().ToUpper().Contains(locator.ToUpper()))
                {
                    toReturn += ';' + elem.Value;
                }
            }

            return toReturn;
        }
        
        private string GetInnerXmlByXPath(string xpath)
        {
            string value = String.Empty;
            try
            {
                value = FindByXPath(xpath).InnerXml;
            }
            catch
            {
                Log.LogError("Incorrect xpath, unable to find the node");
                Log.LogDebug("XPath = " + xpath + "\n------Response------\n" + responseBody + "\n------EndResponse------");
            }
            return value;
        }
        private bool UpdateValueByLocalName(string locator, string content, string value)
        {
            try
            {
                XElement element = FindByLocalName(locator, content);
                element.Value = value;
            }
            catch
            {
                Log.LogDebug("Unable to find given locator " + locator);
                return false;
            }

            return true;
        }
        private string GetXmlByAttributeNameAndValue(string locator, string content)
        {




            return String.Empty;
        }
        private string GetValueByLocalName(string locator)
        {
            string value = String.Empty;
            try
            {
                value = FindByLocalName(locator, responseBody).Value;
            }
            catch
            {
                value = "ERROR[1000]";
            }

            return value;
        }
        private string GetValueByLocalName(string locator, string xml)
        {
            string value = String.Empty;
            try
            {
                value = FindByLocalName(locator, xml).Value;
            }
            catch
            {
                value = "ERROR[1000]";
            }

            return value;
        }
        private string GetValueByAttributeName(string locator)
        {
            string value = String.Empty;
            try
            {
                value = FindByAttributeName(locator, responseBody).Value;
            }
            catch
            {
                value = "ERROR[1000]";
            }

            return value;
        }


        private string GetValueByAttributeNameOpt(string locator, string xml)
        {
            string value = String.Empty;
            try
            {
                value = FindByAttributeName(locator, xml).Value;
            }
            catch
            {
                value = "";
            }

            return value;
        }

        private string GetValueByAttributeName(string locator, string xml)
        {

            string value = String.Empty;
            try
            {
                value = FindByAttributeName(locator, xml).Value;
            }
            catch
            {
                value = "ERROR[1000]";
            }

            return value;
        }


        

        private string GetValueByAttributeNameMulti(string locator, string xml)
        {

            string value = String.Empty;
            try
            {
                List<XElement> elems = FindByAttributeName(locator, xml, "name");

                if (elems.Count() == 0)
                    throw new Exception("No element found"); 


                // We need to get all of the descendents if they exist in solr. Otherwise if we dont do this, it will concatinate all the values
                // from the descendents into one big string and this functionality wont work. Instead we will check if there are any descendents.
                // If there are, lets go through all of them and seperate them by ; character
                if (elems.Descendants().Count() > 0)
                    elems = elems.Descendants().ToList(); 
       
                    
                foreach (XElement elem in elems)
                {
                    value += elem.Value + ";";
                }
            }
            catch
            {
                value = "ERROR[1000]";
            }

            return value.TrimEnd(';');
        } 
        private string GetNodeCount(string locator)
        {
            return FindAllNodesByLocalName(locator, responseBody).Count.ToString();
        }
        private string GetValueByRegx(string regexpattern)
        {
            if (Regex.IsMatch(responseBody, regexpattern))
                return Regex.Match(responseBody, regexpattern).Value;
            else
                return "ERROR[1000]";
        }
  
        private string GetInnerXmlByLocalName(string locator)
        {
            string value = String.Empty;

            // if User specifies @root locator give him back entire response body
            if (String.Compare(locator, "@root")==0)
                return responseBody; 

            try
            {
                value = FindByLocalName(locator, responseBody).ToString();
            }
            catch
            {
                return String.Empty; 
            }

            return value;
        }
        public string GetDate(string daysFromToday, string format)
        {
            //  Time Formats 
            //  yyyy-MM-dd for departures,returns,checkins,checkouts
            //  MM/dd/yyyy for atlantis

            DateTime futureTime = DateTime.Today.AddDays(Convert.ToDouble(daysFromToday));
            return futureTime.ToString(format);
        }
        private string GetSubString(string locator, HtmlDocument html)
        {
            //Match substring index and range
            string subindexpattern = @"\[\d+\]";
            string rangepattern1 = @"\(\d+\)";
            string rangepattern2 = @"\(\d+\-\d+\)";
            string subrangepattern = @"(" + rangepattern1 + "|" + rangepattern2 + ")";
            string locatorpattern = @"\((.*?)" + subrangepattern + @"\)";
            string sswindexpattern = @"substring" + locatorpattern + subindexpattern;
            string sswoindexpattern = @"substring" + locatorpattern;
            string finalValue = String.Empty;
            string range = String.Empty;
            string corelocator = String.Empty;
            int index = 0;


            if (Regex.IsMatch(locator, sswindexpattern))
            {
                index = Convert.ToInt32(Regex.Match(locator, subindexpattern).Value.Trim('[', ']'));
                range = Regex.Match(locator, subrangepattern).Value;
                corelocator = Regex.Match(locator, locatorpattern).Value.Replace(range, String.Empty).Trim('(', ')');

            }
            else if (Regex.IsMatch(locator, sswoindexpattern))
            {

                range = Regex.Match(locator, subrangepattern).Value;
                corelocator = Regex.Match(locator, locatorpattern).Value;

            }
            else
            {
                Log.LogError("No Matching Substring Index Pattern found. Check that index was given in correct format");
            }


            // Check range
            if (Regex.IsMatch(range, rangepattern1))
            {
                int startIndex = Convert.ToInt32(Regex.Match(range, @"\d+").Value);

                finalValue = html.DocumentNode.SelectNodes(corelocator)[index].InnerText.Substring(startIndex);

            }
            else if (Regex.IsMatch(range, rangepattern2))
            {
                int startIndex = Convert.ToInt32(Regex.Match(range, @"\d+").Value);
                int endIndex = Convert.ToInt32(Regex.Match(range, @"\d+").NextMatch().Value);

                finalValue = html.DocumentNode.SelectNodes(corelocator)[index].InnerText.Substring(startIndex, endIndex);
            }
            else
            {
                Log.LogError("Unable to find valid range for the substring startIndex and endIndex [E0012]");
            }



            return finalValue;
        }
        private XmlNode FindByXPath(string xpath)
        {
            // this is my way to obtain the XML data, its unimportant to my question.
            XmlDocument oXML = new XmlDocument();
            oXML.LoadXml(responseBody);

            XmlNamespaceManager nsmgr = GetAllNS(new XmlNamespaceManager(oXML.NameTable));
            XmlNode node = oXML.SelectSingleNode(xpath, nsmgr);

            if (node == null)
            {
                Log.LogError("Unable to Select the node");
                //Log.LogDebug("XPath = " + xpath + "\n------Response------\n" + responseBody + "\n------EndResponse------"); 
            }

            return node;
        }
        private string FindAndUpdateByLocalName(string locator, string content, string value)
        {
            XDocument doc = XDocument.Parse(content);
            int atIndex = 0;

            var node = from e in doc.Descendants()
                       where e.Name.LocalName == locator
                       select e;

            node.ElementAt(atIndex).Value = value;

            return doc.ToString();
        }

        private string CalculateValueFromList(string locator, string content, string operation)
        {
            decimal newvalue = 0;
            string[] listofvalues = GetListByLocalName(locator, content).Split(new char[]{'\n','\r',' '});

            foreach (string s in listofvalues)
            {
                

                try
                {
                    newvalue += Convert.ToDecimal(s); 
                }
                catch
                {

                }
            }

            switch (operation)
            {
                case "add":

                    break; 
                case "sub":
                    break; 
                default:
                    break;
            }

            return newvalue.ToString(); 

        }
        private string GetListByLocalName(string locator, string content)
        {
            List<XElement> elements = FindAllNodesByLocalName(locator, content);
            string list = String.Empty; 

            foreach (XElement element in elements)
            {
                list += element.Value + Environment.NewLine; 
            }


            return list; 
        }
       

        private XElement FindByLocalName(string locator, string content)
        {
            string FULL_INDEX_PATTERN = @"\[\d+\]";
            string targetValue = String.Empty;
            int atIndex = 0;

            if (Regex.IsMatch(locator, FULL_INDEX_PATTERN))
            {
                string fullIndex = Regex.Match(locator, FULL_INDEX_PATTERN).Value;
                string value = Regex.Match(fullIndex, @"(?<=\[)\d+(?=\])").Value;
                atIndex = value.Equals(String.Empty) ? Convert.ToInt32(0) : Convert.ToInt32(value);
                locator = locator.Replace(fullIndex, String.Empty);
            }



            return FindAllNodesByLocalName(locator, content).ElementAtOrDefault(atIndex);

        }


        private XElement RemoveAllAttributeValuesByName(string attname, string xmlcontent)
        {
            var doc = XElement.Parse(xmlcontent);

            List<XElement> elemList = doc.Descendants().ToList();
            List<XElement> foundElements = new List<XElement>();
            foreach (XElement elem in elemList)
            {
                try
                {
                    foreach (XAttribute att in elem.Attributes())
                    {
                        if (att != null)
                        {
                            if (att.Name.ToString().ToLower().Equals(attname.ToLower()))
                            {
                                att.Value = String.Empty;

                            }
                        }
                    }
                }
                catch
                {
                    Log.LogError("Unable to remove attribute you are looking for"); 
                }
            }

            return doc; 
        }

        private List<XElement> FindByAttributeName(string locator, string content, string attname)
        {
            var doc = XElement.Parse(content);
            XElement found = null;
            List<XElement> elemList = doc.Descendants().ToList();
            List<XElement> foundElements = new List<XElement>(); 
            foreach (XElement elem in elemList)
            {
                try
                {
                    foreach (XAttribute att in elem.Attributes())
                    {
                        if (att != null)
                        {
                            if (att.Name.ToString().ToLower().Equals(attname))
                            {
                                string value = att.Value;

                                if (value.Equals(locator))
                                {
                                    found = elem;
                                    foundElements.Add(found); 
                                    
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }

            //if (found.Elements().Count() > 0)
            //{
            //    return found.Elements().LastOrDefault();
            //}
            //else
            //{
            //    return foundElements.ElementAt(0);
            //}

            return foundElements; 
        }

        private XElement FindByAttributeName(string locator, string content)
        {
            return FindByAttributeName(locator, content, "name").ElementAt(0); 
        }

        // Usage: Pass in the localname you are searching for and content as xml
        // Result: It will return list of XElements that match the search criteria.
        private List<XElement> FindAllNodesByLocalName(string locator, string content)
        {

         
            var doc = XElement.Parse(content);
            var values = from e in doc.Descendants()
                         where e.Name.LocalName == locator
                         select e;



            return values.ToList();

        }
        // Usage: enter parent localname , descendent localname, attribute for the local name and the content as the xml from which to search
        // Returns: Returns the result as an InnerXML node as an XElement that can be used later to search for other elements or empty string if nothing was found
        private static XElement FindDescendentByLocalNameAndAttribute(string parentlocator, string descendentlocator, string attribute, string attributevalue, string content)
        {
            var doc = XElement.Parse(content);

            // Find Parent in the xml first and get its content
            var values = from e in doc.Descendants()
                         where e.Name.LocalName == parentlocator
                         select e;

            // Find Descendent starting from the Parent found above
            var desc = from e in values.Descendants()
                       where e.Name.LocalName == descendentlocator
                       select e;

            try
            {
                foreach (XElement el in desc)
                    if (el.Attribute(attribute).Value.Equals(attributevalue))
                        return el;
            }
            //ignore if it doesnt find anything
            catch { }

            try
            {
                for (int i = 0; i < desc.Count(); i++)
                {
                    var root = desc.ElementAt(i); 
                    foreach (XElement el in root.Descendants())
                        if (el.Value.Equals(attributevalue))
                            return root;
                }
            }
            //ignore if it doesnt find anything
            catch { }

            return null;

        }
        private static XElement FindDescendentByLocalName(string parentlocator, string descendentlocator, string content)
        {
            var doc = XElement.Parse(content);

            // Find Parent in the xml first and get its content
            var values = from e in doc.Descendants()
                         where e.Name.LocalName == parentlocator
                         select e;

            // Find Descendent starting from the Parent found above
            var desc = from e in values.Descendants()
                       where e.Name.LocalName == descendentlocator
                       select e;

            // Return first occurance of the descendent element
            return desc.ElementAtOrDefault(0);

        }
        private XmlNamespaceManager GetAllNS(XmlNamespaceManager nsmngr)
        {
            string pattern = @"(?<=xmlns:).*?(?=\s+)";
            MatchCollection matches = Regex.Matches(responseBody, pattern);

    

            foreach (Match m in matches)
            {
                string[] kvpairs = m.Value.Split('=');
                string key = kvpairs[0].Trim();
                string value = Regex.Match(kvpairs[1], "(?<=\").*?(?=\")").Value;
                nsmngr.AddNamespace(key, value);
            }

            return nsmngr;
        }

 
        // Public Facing Methods
        public List<XElement> GetXmlValuesByLocator(string locator, string xml)
        {
            return FindAllNodesByLocalName(locator, xml); 
        }

    }

}
