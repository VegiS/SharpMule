using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; 
using System.Net;
using System.IO;
using HtmlAgilityPack;
using SharpMule.Automation.Framework.Essentials; 

namespace TestManager.Shared
{
    public class CommonTaskLibrary
    {
        const string HEALTH_INFO_URL = @"your service endpoint";
        const string HEALTH_SERVICE_PATH = @"path to your health";
  
        // Environment Health Related Methods
        public static string GetEnvironmentHealthState(string env)
        {

            string resp = GET(HEALTH_INFO_URL+env.ToUpper());
            string locator = @"//*[@id='currentEnvironmentStatus']";
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(resp);
            string status = String.Empty; 

            try
            {
                status = html.DocumentNode.SelectNodes(locator)[0].InnerText.TrimEnd(new char[]{'\r','\n',' ','*'});
            }
            catch
            {
                status = "Not Found"; 
            }

            return status; 
        }
        public static string GetServiceHealth(string endpoint)
        {
            string result = String.Empty; 
            try
            {
                string url = "http://" + endpoint + HEALTH_SERVICE_PATH;
                string resp = GET(url);
                string locator = "sibling:SharedResources:Status";

                TaskRepository lib = new TaskRepository();
                result = lib.Find(locator, resp).Value;
                
            }
            catch
            {
                result = "Not Found"; 
            }

            return result;

        }
        public static string GET(string url)
        {

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Proxy = null;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader resStream = new StreamReader(response.GetResponseStream());

                return resStream.ReadToEnd();
            }
            catch
            {
                return String.Empty; 
            }
        }

        

       

    }
}
